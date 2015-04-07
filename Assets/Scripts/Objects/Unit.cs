using UnityEngine;
using System.Collections.Generic;

public class Unit : Photon.MonoBehaviour {
	public Board board = null;
	Village village;

	UnitType myType;
	ActionType currentAction;
	public Tile tile;
	public List<Tile> currentPath = null;
	public int currentPathIndex;
	public float height = 1f;
	public Player owner = null;
	public float moveSpeed = 5f; // In portion of length
	private GameObject halo;

	void Start()
	{
		halo = transform.Find("SelectedHalo").gameObject;
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
			} else { // Unit has arrived at target
				this.tile.setUnit(this);
				//this.tile.setVillage(this.getVillage());
				callCapture(tile.pos.q, tile.pos.r);
				if (this.tile.getLandType() == LandType.Tree) {
					harvestTree(tile.pos.q, tile.pos.r);
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
				this.tile.getVillage().removeTile (this.tile); 
			}
		}
		this.tile.setVillage(this.getVillage());
		this.getVillage().addTile(this.tile);
		
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
						if (!callCheckPath (t1,withOutT[j])){ 
							Tile potential = isAdjacentToListTiles(separated, withOutT[j]); 
							if (potential == null){ //there is no conflict in separated	
								separated.Add (withOutT[j]);
								pathExists = false; 
							}
							else { 
								if (withOutT[j].getStructure () == Structure.Village) { 
									separated.Remove (potential);
									separated.Add (withOutT[j]); 
								}
							}	
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
					foreach(Tile entry in callVillageTiles(t)){
						if(entry.getStructure() == Structure.Village){
							Debug.Log(callVillageTiles(entry).Count);
							Debug.Log("THISWHATHIT");
							hasVillage = t;
						}
					}
					if(callVillageTiles(toKeep).Count < callVillageTiles(t).Count)
						toKeep = t;
				}
				if(callVillageTiles(hasVillage).Count > 2){
					toKeep = hasVillage;
				}

				if(callVillageTiles(toKeep).Count < 2){
					Debug.Log("CASE 1");
					toKeep.getVillage().delete();
				}
				else if(toKeep == hasVillage){
					Debug.Log("CASE 2");
					foreach(Tile t in separated){
						if(t != toKeep)
							foreach(Tile deadTile in callVillageTiles(t))
								deadTile.killTile();
					}
				}
				else{
					Debug.Log("CASE 3");
					toKeep.getVillage().moveVillage(callVillageTiles(toKeep)[Random.Range(0, callVillageTiles(toKeep).Count)]);
					foreach(Tile t in separated){
						if(t!= toKeep)
							foreach(Tile deadTile in callVillageTiles(t))
								deadTile.killTile();
					}
				}



				//foreach(Tile t2 in separated){
				//	if(t2 != toKeep || callVillageTiles(toKeep).Count < 2){
				//		foreach(Tile deadTile in callVillageTiles(t2))
				//			deadTile.killTile();
				//	}
				//}
			} 
		}
	}
	
	public Tile isAdjacentToListTiles (List<Tile> list, Tile t) { 
		
		if (list.Count > 0){ 
			foreach (Tile l in list) { 
				Dictionary<Hex.Direction, Tile> neighbours = l.getNeighbours();
				foreach (KeyValuePair<Hex.Direction, Tile> pair in neighbours) { 
					if (pair.Value == t) { 
						return pair.Value;
					}
				}
			}
		}	
		return null;  
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
	
	//returns true if the tile has any unit on it, you're not allowed to move onto a tile that has a unit on it. 
	public bool containsUnit (Tile target) { 
		Unit potentialUnit = target.getUnit (); 
		if (potentialUnit != null) { 
			return true; 
		}
		return false; 
	}

	public void MoveTo(Tile target) {
		
		if (containsUnit (target)) {
			board.selectedUnit = null;
			halo.SetActive(false);
			return; 
		}
	 	Unit potentialEnemy = this.getTile().containsEnemyInNeighbour(target); 
	 	if (potentialEnemy != null) { 
	 		if (!combat (potentialEnemy)) { 
				board.selectedUnit = null;
				halo.SetActive(false);
				return; 
	 		}
	 	}
		
		if (isMoving())
			Debug.LogError("Unit already moving");
		else if (target.canEnter(this) == false)
			Debug.LogError("Cannot move to this tile");
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
		}
		board.selectedUnit = null;
		halo.SetActive(false);
		if (potentialEnemy != null) {
			potentialEnemy.kill();
		}
	}

	public void kill(){
		this.getTile().setLandType(LandType.Tombstone);
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

	void callCapture(int q, int r){
		Tile tempTile = null;
		foreach(Tile t in board.getMap().Values){
			if(t.pos.q == q && t.pos.r == r){
				tempTile = t;
			}
		}
		tempTile.getUnit().captureTile();
	}

	void harvestTree(int q, int r){
		Tile tempTile = null;
		foreach(Tile t in board.getMap().Values){
			if(t.pos.q == q && t.pos.r == r){
				tempTile = t;
			}
		}
		tempTile.setLandType(LandType.Grass);
		tempTile.getVillage().changeWood(1);
	}

	void OnMouseUp() {
	//	Debug.Log("Unit OnMouseUp");
		owner = this.tile.getOwner();
		board.selectedUnit = this;
		halo.SetActive(true);
		//this.transform.renderer.material.color = Color.green;
	}

}
