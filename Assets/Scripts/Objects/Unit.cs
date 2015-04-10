using UnityEngine;
using System.Collections.Generic;

public class Unit : Photon.MonoBehaviour {
	public Board board = null;
	Village village;

	UnitType myType;
	public ActionType currentAction = ActionType.ReadyForOrders;
	public Tile tile;
	public List<Tile> currentPath = null;
	public int currentPathIndex;
	public float height = 1f;
	public Player owner = null;
	public float moveSpeed = 5f; // In portion of length
	public GameObject halo;
	public GameObject cannonHalo;

	void Start()
	{
		halo = transform.Find("SelectedHalo").gameObject;
		cannonHalo = transform.Find("CannonHalo").gameObject;
	}

	void Update() {
		if (isMoving()) { // Path animation
			for (int i = 0; i < currentPath.Count-1; i++) { // Debug line
				Vector3 start = board.TileCoordToWorldCoord(currentPath[i].getPixelPos()) + new Vector3(0f, 4, 0f);
				Vector3 end = board.TileCoordToWorldCoord(currentPath[i + 1].getPixelPos()) + new Vector3(0f, 4, 0f);
				Debug.DrawLine(start, end, Color.red);
			}
			var unitPos = new Vector2(transform.position.x, transform.position.z);
			var tilePos = new Vector2(tile.transform.position.x, tile.transform.position.z);
			if (Vector2.Distance(unitPos, tilePos) < 0.1f) {
				MoveToNextTile();
			}
			transform.position = Vector3.Lerp(transform.position, tile.transform.position, moveSpeed * Time.deltaTime);
			setHeightAboveBoard();
		}
	}

	public void MoveToNextTile() {
		if (isMoving()) { // Unit is going somewhere
			currentPathIndex ++; // Next tile!
			if (currentPathIndex < currentPath.Count) { // Unit is still getting there
				setTile(currentPath[currentPathIndex]);
				//WORKS!
				if((this.getUnitType() == UnitType.Knight || this.getUnitType() == UnitType.Soldier) && this.getTile().getLandType() == LandType.Meadow) { 
					this.getTile ().setLandType (LandType.Grass); 	
				}
				
			} else { // Unit has arrived at target
				this.tile.setUnit(this);
				//this.tile.setVillage(this.getVillage());
				if(this.tile.getStructure() == Structure.Village && this.tile.getOwner() != this.getOwner()){
					this.tile.getVillage().moveVillage(callVillageTiles(this.tile)[Random.Range(0, callVillageTiles(this.tile).Count)], LandType.Meadow);
				}
				callCapture(tile.pos.q, tile.pos.r);
				if (this.tile.getLandType() == LandType.Tree || this.tile.getLandType() == LandType.Tombstone) {
					removeTreeOrTombstone(tile.pos.q, tile.pos.r, this.tile.getLandType ());
					//	this.tile.getVillage().GetComponent<PhotonView>().RPC("harvestTree", PhotonTargets.All, tile.pos.q, tile.pos.r);
				}
				HashSet<Tile> borderTiles = this.tile.getAdjacentFriendlyBorder();
				foreach(Tile tempTile in borderTiles){
					this.tile.getVillage().mergeWith(tempTile.getVillage());
				}
				currentPath = null;
				positionOverTile();
			}
		}
	}

	public void captureTile() {
		bool opponentTile = false; 
		Village possibleOpponentVillage = this.tile.getVillage (); 
		if (possibleOpponentVillage != null){
			if (possibleOpponentVillage.getOwner () != this.getVillage().getOwner ()){
				opponentTile = true; 
				this.setActionType (ActionType.ClearedTile); //invaded tile
				Debug.Log ("Unit actionType should now be ClearedTile due to invasion."); 
				this.tile.getVillage().removeTile (this.tile);
				if (this.tile.getVillage ().getTiles ().Count < 3) { 
					this.tile.getVillage().delete (this); 
				}
			}
		}
		if (this.tile.getOwner () == null) this.setActionType(ActionType.ClearedTile); //captured unowned tile. 
		Debug.Log ("Unit actionType should now be ClearedTile due to unowned tile."); 
		this.getTile().setVillage(this.getVillage()); //fixed just now from this.tile.setVillage(...)
		this.getVillage().addTile(this.tile);

		if (this.getUnitType() == UnitType.Cannon)this.setActionType(ActionType.Moved); 
		
		if (opponentTile){
			Dictionary<Hex.Direction, Tile> neighbours = this.tile.getNeighbours(); 
			List<Tile> ownedByOpponent = new List<Tile>(); 
			foreach (KeyValuePair<Hex.Direction, Tile> pair in neighbours) { 
				if (pair.Value.getOwner () != this.tile.getOwner () && pair.Value.getOwner() != null) { 
					ownedByOpponent.Add (pair.Value); 
				}
			}
			
			bool pathExists = true; 
			List<Tile> separated = new List<Tile>(); 
			
			if (ownedByOpponent.Count >= 2) { 
				for (int i=0; i<ownedByOpponent.Count; i++) { 
					Tile t1 = ownedByOpponent[i];
					List<Tile> withOutT = new List<Tile>(ownedByOpponent); 
					withOutT.Remove (t1); 
					for (int j=0; j<withOutT.Count; j++){
						if (!callCheckPath (t1,withOutT[j]) && !isAdjacentToListTiles(separated, withOutT[j])){ 
							separated.Add (withOutT[j]);
							pathExists = false; 
						}	
					}
				if (!pathExists) separated.Add(t1); 
					break; 
				}			
			}
			
			//FOR PAUL
			if (!pathExists) { 
				Tile hasVillage = null;
				Tile toKeep = separated[0];
				foreach(Tile t in separated) {
					Debug.Log(t.hasStructure());
					foreach(Tile entry in callVillageTiles(t)){
						if(entry.getStructure() == Structure.Village){
							Debug.Log("THISWHATHIT");
							hasVillage = t;
						}
					}
					if(callVillageTiles(toKeep).Count < callVillageTiles(t).Count)
						toKeep = t;
				}
				if(callVillageTiles(hasVillage).Count >= 3){
					toKeep = hasVillage;
				}

				if(callVillageTiles(toKeep).Count < 3){
					toKeep.getVillage().delete(this);
				}
				else if(toKeep == hasVillage){
					foreach(Tile t in separated){
						if(t != toKeep){
							foreach(Tile deadTile in callVillageTiles(t))
								deadTile.killTile();
						}
					}
				}
				else{
					toKeep.getVillage().moveVillage(callVillageTiles(toKeep)[Random.Range(0, callVillageTiles(toKeep).Count)], LandType.Tree);
					foreach(Tile t in separated){
						if(t!= toKeep){
							foreach(Tile deadTile in callVillageTiles(t))
								deadTile.killTile();
						}
					}
				}
			} 
		}
	}
	
	public bool isAdjacentToListTiles (List<Tile> list, Tile t) { 
		
		if (list.Count > 0){ 
			foreach (Tile l in list) { 
				Dictionary<Hex.Direction, Tile> neighbours = l.getNeighbours();
				foreach (KeyValuePair<Hex.Direction, Tile> pair in neighbours) { 
					if (pair.Value == t) { 
						return true; 
					}
				}
			}
		}	
		return false;  
	} 

	//get the list of all tiles belonging to the village Tile t belongs to. 
	public List<Tile> villageTiles(Tile t, List<Tile> visited) { 
		visited.Add (t); 
		//int size = 1; 
		Dictionary<Hex.Direction, Tile> neighbours = t.getNeighbours (); 
		foreach (KeyValuePair<Hex.Direction, Tile> pair in neighbours){ 
			if (!visited.Contains (pair.Value) && pair.Value.getOwner () == t.getOwner ()){
				villageTiles(pair.Value, visited); 
			}
		}
		return visited; 
	}
	//call VillageTiles with an empty list. 
	public List<Tile> callVillageTiles (Tile t) {
	List<Tile> visited = new List<Tile>(); 
	return villageTiles (t, visited);
	} 
	
	//check if a path goes from one tile to another within the same village  
	public bool checkPath (Tile x, Tile y, List<Tile> visited) {
		
		bool toReturn = false; 
		visited.Add (x); 
		Dictionary<Hex.Direction, Tile> neighbours = x.getNeighbours(); 
		foreach (KeyValuePair<Hex.Direction, Tile> pair in neighbours){ 
			if (pair.Value.getOwner() == x.getOwner() && !visited.Contains (pair.Value)) { 
				if (pair.Value == y) return true; 
				else { 
					//visited.Add (pair.Value); 
					toReturn = checkPath (pair.Value, y,visited); 
					if (toReturn == true) return true; 
				}
			}	
		}
		return false; 
		
	}
	//call check path with an empty list. 
	public bool callCheckPath (Tile x, Tile y){
		List<Tile> visited = new List<Tile> (); 
		return checkPath (x,y,visited); 
	}
	
	//return true if wins, false otherwise. If false, you cant even invade that tile.
	//we check what would happen if combat occurs first, and you can only invade a tile if combat returns true. 
	public bool combat(Unit other) { 
		
		UnitType thisType = this.getUnitType(); 
		UnitType otherType = other.getUnitType (); 
		
		if (thisType > otherType) return true;
		else return false;   
		
	}

	/*	
	//return true if an enemy is in a one hex radius
	public Unit containsEnemyInNeighbour (Tile target) { 
		
		Dictionary<Hex.Direction, Tile> neighbours = target.getNeighbours(); 
		foreach (KeyValuePair<Hex.Direction, Tile> pair in neighbours) { 
			if (pair.Value.getUnit() != null) { 
				if (pair.Value.getUnit ().getOwner() != this.getOwner ()) { //is enemy
					return pair.Value.getUnit (); 
				}
			}
		}
		return null;
		
	}
	*/
	
	//returns true if the tile has enemy unit on it. 
	public bool containsEnemyUnit (Tile target) { 
		Unit potentialUnit = target.getUnit (); 
		if (potentialUnit != null && potentialUnit.getOwner () != this.getOwner ()) { 
			return true; 
		}
		return false; 
	}
	
	public bool containsFriendlyUnit (Tile target) { 
		Unit potentialUnit = target.getUnit (); 
		if (potentialUnit != null && potentialUnit.getOwner () == this.getOwner ()) { 
			return true; 
		}
		return false; 
	}
	
	//different villages? FIX TODO
	public bool combineUnits (Unit other) { 
		
		if (this.getUnitType() == UnitType.Peasant && other.getUnitType() == UnitType.Peasant) { 
			int totalGold = 0; 
			if (this.getVillage() == other.getVillage ()) totalGold = this.getVillage().getGold (); 
			else totalGold = this.getVillage().getGold() + other.getVillage().getGold (); 
			if (this.getVillage ().getGold () >= 6) { //only upkeep
				this.setUnitType (UnitType.Infantry); 
				other.getVillage().getUnits().Remove(other);
				other.kill (false); 
				return true; 
			}
		}
		else if ((this.getUnitType() == UnitType.Infantry && other.getUnitType() == UnitType.Peasant) || (this.getUnitType() == UnitType.Peasant && other.getUnitType() == UnitType.Infantry)) { 
			int totalGold = 0; 
			if (this.getVillage() == other.getVillage ()) totalGold = this.getVillage().getGold (); 
			else totalGold = this.getVillage().getGold() + other.getVillage().getGold ();
			if (this.getVillage ().getGold () >= 6) { //only upkeep
				this.setUnitType (UnitType.Soldier); 
				other.getVillage().getUnits().Remove(other);
				other.kill (false); 
				return true; 
			}
		}
		
		else if (this.getUnitType() == UnitType.Infantry && other.getUnitType() == UnitType.Infantry) { 
			int totalGold = 0; 
			if (this.getVillage() == other.getVillage ()) totalGold = this.getVillage().getGold (); 
			else totalGold = this.getVillage().getGold() + other.getVillage().getGold ();
			if (this.getVillage ().getGold () >= 6) { //only upkeep
				this.setUnitType (UnitType.Knight); 
				other.getVillage().getUnits().Remove(other);
				other.kill (false); 
				return true;
			}
		}
		return false; 
		
	}

	public void MoveTo(Tile target) {

		if((this.getUnitType() != UnitType.Soldier || this.getUnitType() != UnitType.Knight) && target.getStructure() == Structure.Village && target.getOwner() != this.getOwner()){
			board.setErrorText ("Only soldiers and knights can invade villages.");
			board.selectedUnit = null;
			halo.SetActive(false); 
			return;
		}

		if (this.getUnitType () == UnitType.Cannon && this.getActionType() == ActionType.Moved) { 
			board.setErrorText ("Cannons cannot move more than one tile a turn.");
			board.selectedUnit = null;
			halo.SetActive(false); 
			return;
		}
		
		if ((this.getUnitType () == UnitType.Peasant || this.getUnitType () == UnitType.Cannon) && target.getOwner () != this.getOwner () && target.getOwner () != null) { 
			board.setErrorText ("Peasants and Cannons cannot invade enemy territory");
			board.selectedUnit = null;
			halo.SetActive(false); 
			return;	
		}
		
		if (this.getActionType() == ActionType.ClearedTile){
			board.setErrorText ("Unit has already performed an action this turn");
			board.selectedUnit = null;
			halo.SetActive(false); 
			return;
		}
		
		if (this.getActionType() == ActionType.Cultivating || this.getActionType() == ActionType.BuildingRoad || this.getActionType() == ActionType.StillCultivating){ 
			board.setErrorText ("Unit is busy this turn");
			board.selectedUnit = null;
			halo.SetActive(false); 
			return;
		}

		//this needs to be removed, we want to upgrade units if they are friendly and everything is right. 
		if (containsEnemyUnit (target)) {
			board.selectedUnit = null;
			halo.SetActive(false);
			board.setErrorText ("Tile Occupied By Enemy");
			return; 
		}
		
		if (containsFriendlyUnit (target)) { 
			if (!combineUnits (target.getUnit ())) {
				board.selectedUnit = null;
				halo.SetActive(false); 
				board.setErrorText ("You can either not afford this or it is an invalid combine.");
				return;
			}
		}
		
	 	Unit potentialEnemy = this.getTile().containsEnemyInNeighbour(target); 
	 	if (potentialEnemy != null && potentialEnemy.getUnitType() != UnitType.Cannon) { 
	 		if (!combat (potentialEnemy)) { 
				board.selectedUnit = null;
				halo.SetActive(false);
				board.setErrorText ("Stronger enemy in close proximity! You'll die if you go there");
				return; 
	 		}
	 	}

		if(this.getUnitType() == UnitType.Cannon && this.getTile().DistanceTo(target) > 2){
			board.setErrorText ("Cannons cannot move more than one tile a turn"); 
			board.selectedUnit = null;
			halo.SetActive(false); 
			return;
		}
		
	 	if (this.getUnitType() == UnitType.Knight && (target.getLandType () == LandType.Tree || target.getLandType () == LandType.Tombstone)){ 
	 		//Debug.Log ("Knights and Cannons cannot clear tombstones/trees"); 
	 		board.setErrorText ("Knights and cannons cannot clear tombstones or fell trees"); 
	 		board.selectedUnit = null;
	 		halo.SetActive(false); 
	 		return;  
	 	}
		
		if (isMoving())
			Debug.LogError("Unit is already moving");
		else if (target.canEnter(this) == false)
			board.setErrorText ("Your units can't swim");
			//Debug.LogError("Cannot move to this tile");
		else {

			Dictionary<Tile, int> dist = new Dictionary<Tile, int>(); // Cost
			Dictionary<Tile, Tile> prev = new Dictionary<Tile, Tile>(); // Came from
		
			List<Tile> frontier = new List<Tile>();
			Dictionary<Tile, int> priorities = new Dictionary<Tile, int>(); // Came from

			Tile source = getTile();
			dist[source] = 0;
			prev[source] = null;
			frontier.Add(source);
		
			while (frontier.Count > 0) {
			
				Tile current = null;
				for (int i=0; i<frontier.Count; i++) { // Pick tile with least priority value
					Tile possibleU = frontier[i];
					if (current == null || priorities[possibleU] < priorities[current]) {
						current = possibleU;
					}
				}
				frontier.Remove(current);
				if (current == target)
					break; // First time we find the target, we exit as it is the shortest path
				
				foreach (KeyValuePair<Hex.Direction, Tile> kv in current.getNeighbours()) {
					Tile neighbour = kv.Value;
					int newDist = dist[current] + 1;
					if (((neighbour.canWalkThrough(this) || neighbour == target)) &&
						(dist.ContainsKey(neighbour) == false || newDist < dist[neighbour])) {
						dist[neighbour] = newDist;

						frontier.Add(neighbour);
						priorities[neighbour] = newDist + Hex.Distance(target.pos, neighbour.pos); // heuristic
						prev[neighbour] = current;
					}
				}
			}
			if (prev.ContainsKey(target)) { // If reachable
				currentPath = new List<Tile>();
				Tile curr = target;
		
				while (curr != null) { // Add path by backtracking
					currentPath.Add(curr);
					curr = prev[curr];
				}
				currentPath.Reverse();
				currentPathIndex = 0;
				MoveToNextTile();
			} 
			else { 
			board.setErrorText ("Path Obscure or Blocked.");
			}
		}
		board.selectedUnit = null;
		halo.SetActive(false);
		if (potentialEnemy != null) {
			potentialEnemy.kill(true);
		}
	}

	public void kill(bool tomb){
		if (tomb == true)this.getTile().setLandType(LandType.Tombstone);
		GameObject.Destroy(this.gameObject);
	}

	public bool isMoving() {
		return (currentPath != null && currentPath.Count > 0);
	}

	public Tile getTile() {
		return tile;
	}

	public void setTile(Tile t) {
		if (tile != null)
			tile.unit = null;
		tile = t;
		tile.unit = this;
	}
	public void positionOverTile() {
		if (tile != null) {
			transform.position = tile.transform.position;
			setHeightAboveBoard();
		}
	}
	public void setHeightAboveBoard() {
		var tmp = transform.localPosition;
		tmp.y = height;
		transform.localPosition = tmp;
	}
	
	public void placeUnit() { 
		this.transform.position = board.TileCoordToWorldCoord(tile.getPixelPos()) + new Vector3(0, 1f, 0);
	}
	
	public ActionType getActionType() {
		return currentAction;
	}
	
	public void setActionType(ActionType at) {
		currentAction = at;
	}
	
	public UnitType getUnitType() {
		return myType;
	}

	public void setUnitType(UnitType ut)
	{
		myType = ut;
		
		// Hide all meshes
		for (int i = 0; i < System.Enum.GetNames(typeof(UnitType)).Length; i++)
			transform.GetChild(i).gameObject.SetActive(false);

		// Show the correct one
		transform.GetChild((int)myType).gameObject.SetActive(true);
	}

	public void setUnitTypeRandom()
	{
		setUnitType((UnitType) Random.Range(0,System.Enum.GetNames(typeof(UnitType)).Length));
	}
	
	public int getSalary() {
		// Depending on myType
		return 0;
	}
	
	public Village getVillage() {
		return village;
	}
	
	public void setVillage(Village v) {
		village = v;
	}
	
	public Player getOwner() {
		return getVillage().getOwner();
	}

	public void upgrade(){
		if(this.getUnitType() < UnitType.Knight && this.getVillage().getGold() >= 10)
			this.getVillage().GetComponent<PhotonView>().RPC("upgradeUnit", PhotonTargets.All, this.getTile().pos.q, this.getTile().pos.r);
		else if(this.getUnitType() == UnitType.Knight){
			board.setErrorText ("Knights can't be ugraded any further");
		}
		else if(this.getUnitType() == UnitType.Tower){
			board.setErrorText("Towers are not upgradeable units");
		}
		else if(this.getUnitType() != UnitType.Cannon)
			board.setErrorText ("Not enough money to upgrade");
	}

	void callCapture(int q, int r){
		Tile tempTile = null;
		foreach(Tile t in board.getMap().Values){
			if(t.pos.q == q && t.pos.r == r){
				tempTile = t;
			}
		}
		tempTile.getUnit().captureTile();
	}

	void removeTreeOrTombstone(int q, int r, LandType type){
		Tile tempTile = null;
		foreach(Tile t in board.getMap().Values){
			if(t.pos.q == q && t.pos.r == r){
				tempTile = t;
			}
		}
		tempTile.setLandType(LandType.Grass);
		if (type == LandType.Tree)tempTile.getVillage().changeWood(1);
		this.setActionType(ActionType.ClearedTile);
	}

	void OnMouseUp() {
		if(this.transform.root.GetComponent<Game>().GetCurrPlayer() == this.transform.root.GetComponent<Game>().GetLocalPlayer() && this.getOwner() == this.transform.root.GetComponent<Game>().GetLocalPlayer()){
			if(board.selectedUnit != this){
				board.selectedUnit = this;
				foreach(Village v in this.getOwner().getVillages())
					foreach(Unit u in v.getUnits())
				u.halo.SetActive(false);
				halo.SetActive(true);
			}
			else{
				board.selectedUnit = null;
				halo.SetActive(false);
				this.upgrade();
			}
		}
	}

}
