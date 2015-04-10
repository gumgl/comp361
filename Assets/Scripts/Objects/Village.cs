using System;
using UnityEngine;
using SimpleJSON;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class Village : MonoBehaviour {
	VillageType myType;
	int gold = 0;
	int wood = 0;
	bool cultivating = false;
	bool building = false;
	public bool isActive = false;
	bool upgradable = false; 
    Player owner; // Should be set in constructor
	HashSet<Tile> tiles = new HashSet<Tile>();
	HashSet<Unit> units = new HashSet<Unit>(); 
	Board board;
	public Tile structTile; // Where the HQ is
	public Unit unitPrefab;
	public GameObject cannonHalo; 
	public Unit associatedCannon;
	public int health; 

	public JSONNode Serialize() {
		var node = new JSONClass();

		node["type"].AsInt = (int)this.getVillageType();
		node["HQpos"] = getStructTile().pos.Serialize();

		node["wood"].AsInt = this.getWood();
		node["gold"].AsInt = this.getGold();
		//node["building"].AsInt = Convert.ToInt32(this.areBuilding());
		node["health"].AsInt = this.health;
		//node["owner"] = this.getOwner().photonPlayer.name;

		node["tiles"] = new JSONArray();
		foreach (Tile tile in this.getTiles()) {
			node["tiles"][-1] = tile.pos.Serialize();
		}

		node["units"] = new JSONArray();
		foreach (var unit in this.getUnits()) {
			node["units"][-1] = unit.Serialize();
		}
		return node;
	}

	public void UnSerialize(JSONNode node) {
		setVillageType((VillageType) node["type"].AsInt);

		var HQpos = new Hex(node["HQpos"]);
		structTile = board.getTile(HQpos);
		structTile.setStructure(Structure.Village); // let the tile know that it is the HQ

		transform.position = board.TileCoordToWorldCoord(structTile.getPixelPos());

		wood = node["wood"].AsInt;
		gold = node["gold"].AsInt;
		//building = Convert.ToBoolean(node["building"].AsInt);
		health = node["health"].AsInt;
		//node["owner"] = this.getOwner().photonPlayer.name;

		var tileNodes = node["tiles"].AsArray;
		foreach (JSONNode tileNode in tileNodes) {
			var pos = new Hex(tileNode);
			var tile = board.getTile(pos);
			//tiles.(board.getTile(pos));
			addTile(tile);
			tile.setVillage(this);
		}

		var unitNodes = node["units"].AsArray;
		foreach (JSONNode unitNode in unitNodes) {
			Unit unit = Instantiate(unitPrefab, Vector3.zero, Quaternion.identity) as Unit;
			unit.transform.parent = board.transform;
			unit.board = this.board;
			unit.UnSerialize(unitNode);
			unit.setVillage(this);
			units.Add(unit);
		}
		
		if (myType == VillageType.Hovel) {
			transform.GetChild(0).gameObject.SetActive(true);
			transform.GetChild(1).gameObject.SetActive(false);
			transform.GetChild(2).gameObject.SetActive(false);
			transform.GetChild(3).gameObject.SetActive(false);
		}
		if (myType == VillageType.Town)
		{
			transform.GetChild(0).gameObject.SetActive(false);
			transform.GetChild(1).gameObject.SetActive(true);
			transform.GetChild(2).gameObject.SetActive(false);
			transform.GetChild(3).gameObject.SetActive(false);
		} 
		if (myType == VillageType.Fort)
		{
			transform.GetChild(0).gameObject.SetActive(false);
			transform.GetChild(1).gameObject.SetActive(false);
			transform.GetChild(2).gameObject.SetActive(true);
			transform.GetChild(3).gameObject.SetActive(false);
		} 
		if (myType == VillageType.Castle){			
			transform.GetChild(0).gameObject.SetActive(false);
			transform.GetChild(1).gameObject.SetActive(false);
			transform.GetChild(2).gameObject.SetActive(false);
			transform.GetChild(3).gameObject.SetActive(true);
		}
	}

	public void init(Player own, Board bor, VillageType type, int gl, int wd, Tile tile){
		board = bor;
		owner = own;
		tile.setStructure(Structure.Village);
		addTile(tile);
		setStructTile(tile);
		setVillageType(type);
		gold = gl;
		wood = wd;
		own.addVillage(this);
		transform.parent = board.transform;
	}
	
	public VillageType getVillageType () { 
		return myType; 
	}
	
	public HashSet<Tile> getTiles() {
		return tiles;
	}
	public Tile getRandomTile() {
		return null;
		// TODO: Cannot get specific element from HashSet<>, needs refactoring to List<>
		//return tiles[Random.Range(0, tiles.Count - 1)];
	}
	public bool getUpgradable() { 
		return upgradable;
	} 
	public void setUpgradable (bool b) { 
		upgradable = b;
	}

	public void delete(Unit captor) {
		if (captor != null) {
			captor.getTile ().getVillage ().changeGold (this.getGold ());
			captor.getTile ().getVillage ().changeWood (this.getWood ());
		}
		structTile.setLandType(LandType.Tree);
		//set tile ownership to null for all tiles part of the village
		foreach (Tile t in tiles) {
			t.setVillage(null);
		}
		//for all units, find the tile they are on and set landtype to tombstone. Also remove the units from the tiles.
		foreach (Unit u in units) {
			u.kill(true);
		}
		GameObject.Destroy(this.gameObject);
	}

	public void setStructTile(Tile t) {
		structTile = t;
	}
	
	public Tile getStructTile() {
		return structTile;
	}
	
	public void addTile(Tile t) {
		t.setVillage(this);
		tiles.Add(t);
	}
	
	public void addTiles(HashSet<Tile> tilesToAdd) {
		foreach (Tile t in tilesToAdd) {
			addTile(t);
		}
	}
	
	public void removeTile(Tile dest) {
		tiles.Remove(dest);
	}
	
	public HashSet<Unit> getUnits() {
		return units;
	}
	
	public void tombPhase(HashSet<Tile> tiles) {
		foreach (Tile t in tiles) { 
			if (t.getLandType () == LandType.Tombstone) {
				t.setLandType(LandType.Tree);
			}
		}
	}

	//Move the structure of a village to the current tile
	public void moveVillage(Tile targetTile, LandType landtype){
		structTile.setStructure(Structure.None);
		structTile.setLandType(landtype);
		setStructTile(targetTile);
		targetTile.setLandType(LandType.Grass);
		targetTile.setStructure(Structure.Village);
		transform.position = board.TileCoordToWorldCoord(targetTile.getPixelPos());
	}

	public void removeResources () { 
		this.gold = 0;
		this.wood = 0;
	}
	public bool areCultivating() {
		return cultivating;
	}
	
	public bool areBuilding() {
		return building;
	}
	
	public int getGold() {
		return gold;
	}
	
	public int getWood() {
		return wood;
	}
	
	public void changeGold(int amount) {
		gold += amount;
	}

	public void setGold(int amount){
		gold = amount;
	}
	
	public void changeWood(int amount) {
		wood += amount;
	}
	
	public void buildRoad(Unit u) {
		
	}

	public void setBoard(Board bd) {
		board = bd;
	}
	
	public void setVillageType(VillageType type) {
		myType = type;
		if (type == VillageType.Hovel) this.health = 0;
		if (type == VillageType.Town) this.health = 2;
		if (type == VillageType.Fort) this.health = 5;
		if (type == VillageType.Castle) this.health = 10;
	}

	public int[] getResources() {
		return new int[3];
	}
	
	public void checkForBreak(HashSet<Tile> tiles) {
		
	}
	
	public Village checkForMerge() {
		return null;
	}

	public Player getOwner() {
		return owner;
	}

	public void setOwner(Player who) {
		owner = who;
	}
	
	public HashSet<Tile> findPath(Tile start, Tile dest) {
		return new HashSet<Tile>();
	}
	
	public void addUnit(Unit u) {
		this.units.Add(u);
	}
	
	public HashSet<Tile> getPath() {
		return new HashSet<Tile>();
	}


	void Start() {

	}
	void Update() {

		if (Input.GetKey (KeyCode.Escape)) {
			this.board.game.GetComponent<HovelMenuScript>().CloseBuildMenu();
			this.isActive = false;
			this.setUpgradable (false);
			this.transform.GetChild(0).renderer.material.color = Color.clear;
			this.transform.GetChild(1).renderer.material.color = Color.clear;
			this.transform.GetChild(2).renderer.material.color = Color.clear;
			this.transform.GetChild(3).renderer.material.color = Color.clear;

			foreach(Tile t in tiles){
				if((t.getLandType() == LandType.Grass || t.getLandType() == LandType.Meadow) && t != this.getStructTile() && t.containsEnemyInNeighbour(t) == null){
					t.setAcceptsUnit(false);
					t.transform.GetChild(0).renderer.material.color = t.getVillage().getOwner().getColor();
				}
			}
		}

	}


	[RPC]
	public void hireVillager(int q, int r, int type) { 
		Tile tempTile = null;
		foreach(Tile t in board.getMap().Values){
			if(t.pos.q == q && t.pos.r == r){
				tempTile = t;
			}
		}
		if(tempTile.getVillage().getGold() >= ((UnitType)type).getCost()){
			if((type == 4 && tempTile.getVillage().getWood() >= 12) || (type == 5 && tempTile.getVillage().getWood()>= 5) || type <= 3){
				tempTile.getVillage().changeGold(-((UnitType)type).getCost());
				if(type == 4)
					tempTile.getVillage().changeWood(-12);
				if(type == 5)
					tempTile.getVillage().changeWood(-5);
				
				Unit u = Instantiate(unitPrefab, new Vector3(0,0,0), Quaternion.Euler(0, 180, 0)) as Unit;
				u.transform.parent = board.transform;
				u.board = this.board;
				u.setVillage(tempTile.getVillage());
				//u.setUnitType(UnitType.Peasant);
				u.setUnitType((UnitType)type);
				u.setActionType (ActionType.ReadyForOrders); 
				u.setTile (tempTile);
				u.placeUnit();
				tempTile.getVillage().addUnit(u);
				tempTile.getVillage().isActive = false;
				this.board.game.GetComponent<HovelMenuScript>().CloseBuildMenu();
			}
		}
		else{
			board.setErrorText("Insufficient funds to build this unit.");
			Camera.main.GetComponent<CameraController>().shakeScreen();
			tempTile.getVillage().isActive = false;
			this.board.game.GetComponent<HovelMenuScript>().CloseBuildMenu();
		}
	}

	//Merges the current village with the passed in village
	public void mergeWith(Village v){
		//The if decides which village gets destroyed and which will not
		if(this.getVillageType() > v.getVillageType()){
			foreach (Tile tempTile in v.getTiles()){
				tempTile.setVillage(this);
				this.addTile(tempTile);
			}
			foreach (Unit tempUnit in v.getUnits()){
				tempUnit.setVillage(this);
				this.addUnit(tempUnit);
			}
			this.changeGold(v.getGold());
			this.changeWood(v.getWood());
			v.getStructTile().setStructure(Structure.None);
			v.getStructTile().setLandType(LandType.Meadow);
			GameObject.Destroy(v.gameObject);
		}

		else if(v.getVillageType() > this.getVillageType()){
			foreach (Tile tempTile in this.getTiles()){
				tempTile.setVillage(v);
				v.addTile(tempTile);
			}
			foreach (Unit tempUnit in this.getUnits()){
				tempUnit.setVillage(v);
				v.addUnit(tempUnit);
			}
			v.changeGold(this.getGold());
			v.changeWood(this.getWood());
			this.getStructTile().setStructure(Structure.None);
			this.getStructTile().setLandType(LandType.Meadow);
			GameObject.Destroy(this.gameObject);
		}
		else if(this.getTiles().Count >= v.getTiles().Count){
			foreach (Tile tempTile in v.getTiles()){
				tempTile.setVillage(this);
				this.addTile(tempTile);
			}
			foreach (Unit tempUnit in v.getUnits()){
				tempUnit.setVillage(this);
				this.addUnit(tempUnit);
			}
			this.changeGold(v.getGold());
			this.changeWood(v.getWood());
			v.getStructTile().setStructure(Structure.None);
			v.getStructTile().setLandType(LandType.Meadow);
			GameObject.Destroy(v.gameObject);
		}
		else{
			foreach (Tile tempTile in this.getTiles()){
				tempTile.setVillage(v);
				v.addTile(tempTile);
			}
			foreach (Unit tempUnit in this.getUnits()){
				tempUnit.setVillage(v);
				v.addUnit(tempUnit);
			}
			v.changeGold(this.getGold());
			v.changeWood(this.getWood());
			this.getStructTile().setStructure(Structure.None);
			this.getStructTile().setLandType(LandType.Meadow);
			GameObject.Destroy(this.gameObject);
		}
	}

	[RPC]
	void fireCannonAtVillage(int q, int r){
		Village v = board.getTile(new Hex(q, r)).getVillage();
		if (v.health <= 0){
			if (v.getVillageType() == VillageType.Hovel)
				v.moveVillage(v.callVillageTiles(v.getStructTile())[Random.Range(0, v.callVillageTiles(v.getStructTile()).Count)], LandType.Tree);
			else if (v.getVillageType () == VillageType.Town) { 
				v.setVillageType (VillageType.Hovel);
				v.transform.GetChild(0).gameObject.SetActive(true);
				v.transform.GetChild(1).gameObject.SetActive(false);
			}
			else if (v.getVillageType () == VillageType.Fort) { 
				v.setVillageType (VillageType.Town);
				v.transform.GetChild(1).gameObject.SetActive(true);
				v.transform.GetChild(2).gameObject.SetActive(false);
			}
			else if (v.getVillageType () == VillageType.Castle) { 
				v.setVillageType (VillageType.Fort);
				v.transform.GetChild(2).gameObject.SetActive(true);
				v.transform.GetChild(3).gameObject.SetActive(false);
			}
			
		}
	}
	
	void OnMouseUp () {
		//if(EventSystem.current.IsPointerOverGameObject()){
		if (this.cannonHalo.GetActive ()){
			this.cannonHalo.SetActive (false);
			this.health--;
			this.GetComponent<PhotonView>().RPC("fireCannonAtVillage", PhotonTargets.All, this.getStructTile().pos.q, this.getStructTile().pos.r);
			getAssociatedCannon ().setActionType (ActionType.Moved);
			getAssociatedCannon ().halo.SetActive (false);
			getAssociatedCannon ().getVillage ().changeWood (-1); 
			//this.getStructTile().killTile();
			//this.removeTile (this.getStructTile());
			/*if (this.getTiles ().Count < 3) { 
				this.delete (this.getAssociatedCannon ()); 
			}
			return; */	 
		}
		
		Debug.Log(this.getVillageType());
		foreach(Village v in this.getOwner().getVillages()){
			foreach(Unit u in v.getUnits()){
					board.selectedUnit = null;
					u.halo.SetActive(false);
			}		
		}
		if(this.transform.root.GetComponent<Game>().GetCurrPlayer() == this.transform.root.GetComponent<Game>().GetLocalPlayer() && this.getOwner() == this.transform.root.GetComponent<Game>().GetLocalPlayer()){
		if(!this.isActive){
			foreach(Village v in this.getOwner().getVillages()){
				v.isActive = false;
				v.setUpgradable (false);
				foreach(Tile temp in v.getTiles()){
					temp.setAcceptsUnit(false);
					temp.transform.GetChild(0).renderer.material.color = temp.getVillage().getOwner().getColor();
				}
			}
			this.board.game.GetComponent<HovelMenuScript>().BuildMenu(this.getVillageType());
			this.isActive = true;
			this.transform.GetChild(0).renderer.material.color = Color.black;
			this.transform.GetChild(1).renderer.material.color = Color.black;
			this.transform.GetChild(2).renderer.material.color = Color.black;
			this.transform.GetChild(3).renderer.material.color = Color.black;
		
			foreach(Tile t in tiles){
			//Want to be able to do && t.isAdjacenttoEnemyUnit()
				if((t.getLandType() == LandType.Grass || t.getLandType() == LandType.Meadow) && t != this.getStructTile() && t.containsEnemyInNeighbour(t) == null){
					t.setAcceptsUnit(true);
					t.transform.GetChild(0).renderer.material.color = Color.black;
				}
			}
		}
		else
		{
				this.board.game.GetComponent<HovelMenuScript>().CloseBuildMenu();
			this.isActive = false;
			this.transform.GetChild(0).renderer.material.color = Color.clear;
			this.transform.GetChild(1).renderer.material.color = Color.clear;
			this.transform.GetChild(2).renderer.material.color = Color.clear;
			this.transform.GetChild(3).renderer.material.color = Color.clear;
			
			foreach(Tile t in tiles){
				if((t.getLandType() == LandType.Grass || t.getLandType() == LandType.Meadow) && t != this.getStructTile() && t.containsEnemyInNeighbour(t) == null){
					t.setAcceptsUnit(false);
					t.transform.GetChild(0).renderer.material.color = t.getVillage().getOwner().getColor();
				}
			}
		}
			if (getUpgradable ()){
				GetComponent<PhotonView>().RPC("upgradeVillage", PhotonTargets.All, this.getStructTile().pos.q, this.structTile.pos.r);
				//upgradeVillage(this.getStructTile().pos.q, this.structTile.pos.r); 
			}
			else if (getVillageType() != VillageType.Castle) {
				setUpgradable (true);
			}
		}
		//}
	}

	[RPC]
	public void fireCannon(int q, int r){
		Unit u = board.getTile(new Hex(q, r)).getUnit();
		u.kill(true); 
	}

	[RPC]
	public void upgradeUnit(int q, int r){
		Tile tempTile = board.getTile(new Hex(q, r));
		tempTile.getUnit().setUnitType(tempTile.getUnit().getUnitType() + 1);
		tempTile.getVillage().changeGold(-10);
	}

	[RPC]
	public void moveUnit(int q, int r, int q1, int r1){
		Tile tempTile = board.getTile(new Hex(q, r));
		board.selectedUnit = tempTile.getUnit();
		Tile tempTile2 = board.getTile(new Hex(q1, r1));
		tempTile.getUnit().MoveTo(tempTile2);
	}

	[RPC]
	void upgradeVillage(int q, int r) {
		Debug.Log("HITHITHITHITHTI");
		Tile tempTile = board.getTile(new Hex(q,r));
		Debug.Log(tempTile.getVillage().getVillageType());
		if (tempTile.getVillage().getVillageType() == VillageType.Hovel && tempTile.getVillage().getWood() >= VillageType.Town.getUpgradeCost()) { 
			tempTile.getVillage().transform.GetChild(0).gameObject.SetActive(false);
			tempTile.getVillage().transform.GetChild(1).gameObject.SetActive(true);
			tempTile.getVillage().setVillageType(VillageType.Town);
			tempTile.getVillage().changeWood(-VillageType.Town.getUpgradeCost());
			tempTile.getVillage().setUpgradable(false);
			tempTile.getVillage().transform.GetChild(0).renderer.material.color = Color.clear;
			tempTile.getVillage().transform.GetChild(1).renderer.material.color = Color.clear;
			tempTile.getVillage().transform.GetChild(2).renderer.material.color = Color.clear;
			tempTile.getVillage().transform.GetChild(3).renderer.material.color = Color.clear;
			foreach (Tile t in tempTile.getVillage().getTiles()) {
				t.setAcceptsUnit(false);
				t.transform.GetChild(0).renderer.material.color = tempTile.getVillage().getOwner().getColor();
				
			}
		} else if (tempTile.getVillage().getVillageType() == VillageType.Town && tempTile.getVillage().getWood() >= VillageType.Fort.getUpgradeCost()) { 
			tempTile.getVillage().transform.GetChild(1).gameObject.SetActive(false);
			tempTile.getVillage().transform.GetChild(2).gameObject.SetActive(true);
			tempTile.getVillage().setVillageType(VillageType.Fort);
			tempTile.getVillage().changeWood(-VillageType.Fort.getUpgradeCost());
			tempTile.getVillage().setUpgradable(false);
			tempTile.getVillage().transform.GetChild(0).renderer.material.color = Color.clear;
			tempTile.getVillage().transform.GetChild(1).renderer.material.color = Color.clear;
			tempTile.getVillage().transform.GetChild(2).renderer.material.color = Color.clear;
			tempTile.getVillage().transform.GetChild(3).renderer.material.color = Color.clear;
			foreach (Tile t in tempTile.getVillage().getTiles()) {
				t.setAcceptsUnit(false);
				t.transform.GetChild(0).renderer.material.color = tempTile.getVillage().getOwner().getColor();
				
			}
		} else if (tempTile.getVillage().getVillageType() == VillageType.Fort && tempTile.getVillage().getWood() >= VillageType.Castle.getUpgradeCost()) { 
			tempTile.getVillage().transform.GetChild(2).gameObject.SetActive(false);
			tempTile.getVillage().transform.GetChild(3).gameObject.SetActive(true);
			tempTile.getVillage().setVillageType(VillageType.Castle);
			tempTile.getVillage().changeWood(-VillageType.Castle.getUpgradeCost());
			tempTile.getVillage().setUpgradable(false);
			tempTile.getVillage().transform.GetChild(0).renderer.material.color = Color.clear;
			tempTile.getVillage().transform.GetChild(1).renderer.material.color = Color.clear;
			tempTile.getVillage().transform.GetChild(2).renderer.material.color = Color.clear;
			tempTile.getVillage().transform.GetChild(3).renderer.material.color = Color.clear;
			foreach (Tile t in tempTile.getVillage().getTiles()) {
				t.setAcceptsUnit(false);
				t.transform.GetChild(0).renderer.material.color = tempTile.getVillage().getOwner().getColor();
			}
		}
	}
	
	public Unit getAssociatedCannon () { 
		return this.associatedCannon; 
	}
	
	public void setAssociatedCannon (Unit u) { 
		this.associatedCannon = u;
	}
	
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
}

