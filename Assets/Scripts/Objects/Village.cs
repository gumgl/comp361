using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Village : Photon.MonoBehaviour {
	VillageType myType;
	int gold = 0;
	int wood = 0;
	bool cultivating = false;
	bool building = false;
	bool isActive = false;
	bool upgradable = false; 
    Player owner; // Should be set in constructor
	HashSet<Tile> tiles = new HashSet<Tile>();
	HashSet<Unit> units = new HashSet<Unit>(); 
	Board board;
	public Tile structTile; // Where the HQ is
	public Unit unitPrefab;

	public void init(Player own, Board bor, VillageType type, int gl, int wd, Tile tile){
		board = bor;
		owner = own;
		addTile(tile);
		setStructTile(tile);
		setVillageType(type);
		gold = gl;
		wood = wd;
		own.addVillage(this);
		transform.parent = own.transform;
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
	public void delete() {
		Tile hq = getStructTile();
		if (hq.hasStructure()) {
			hq.removeStructure();
		}
		//set tile ownership to null for all tiles part of the village
		HashSet<Tile> tile = getTiles (); 
		foreach (Tile t in tile) {
			t.setVillage (null);
		}
		//for all units, find the tile they are on and set landtype to tombstone. Also remove the units from the tiles.
		HashSet <Unit> units = getUnits();
		foreach (Unit u in units) { 
			Tile unitTile = u.getTile();
			u.removeUnit();
			unitTile.setLandType(LandType.Tombstone);
		}
	}

	public void setStructTile(Tile t) {
		structTile = t;
	}
	
	public Tile getStructTile() {
		return structTile;
	}
	
	public void addTile(Tile t) {
		//t.setOwner(owner);
		t.setVillage(this);
		tiles.Add(t);
	}
	
	public void addTiles(HashSet<Tile> tilesToAdd) {
		foreach (Tile t in tilesToAdd) {
			addTile(t);
		}
	}
	
	public void removeTile(Tile dest) {
		
	}
	
	public HashSet<Unit> getUnits() {
		return units;
	}
	
	public void removeUnit(Unit u) {
		
	}
	
	public void refresh() {
		
	}
	
	public void tombPhase(HashSet<Tile> tiles) {
		foreach (Tile t in tiles) { 
			if (t.getLandType () == LandType.Tombstone) {
				t.setLandType(LandType.Tree);
			}
		}
	}
	
	public void buildPhase(HashSet<Tile> tiles) {

			foreach (Tile t in tiles) { 
				Unit u = t.getUnit();
				if (u != null) { 
					ActionType type = u.getActionType();
					UnitType unitType = u.getUnitType();
					
				if (unitType == UnitType.Peasant){
						if (areCultivating ()){
							if (type == ActionType.StartCultivating) {
								u.setActionType(ActionType.FinishCultivating);
							}
							else if (type == ActionType.FinishCultivating){ 
								u.setActionType(ActionType.ReadyForOrders);
								t.setLandType(LandType.Meadow);
							}
						}
						
						else if (areBuilding ()) { 
							if (type == ActionType.BuildingRoad){
								u.setActionType(ActionType.ReadyForOrders);
								t.setLandType(LandType.Road);
							}		
						}
					}
				}
			}
		
	}
	
	public void incomePhase(HashSet<Tile> tiles) {
		int phaseGold = 0;
		foreach (Tile t in tiles) { 
			LandType type = t.getLandType();	
			if (type == LandType.Meadow || type == LandType.Road) { 
				phaseGold += 2;
			}
			else if (type ==  LandType.Grass){
				phaseGold += 1;
			}
		}
		this.changeGold(phaseGold);	
	}
	
	public void paymentPhase(HashSet<Tile> tiles) {
		foreach (Tile t in tiles) { 
			Unit u = t.getUnit();
			if (u!=null) { 
				changeGold (-u.getSalary ());
			}
			if (getGold () < 0)  {
				removeResources();
				delete();
				break;
			}
		}
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
	
	public void changeWood(int amount) {
		wood += amount;
	}
	
	public void buildRoad(Unit u) {
		
	}
	
	public void setVillageType(VillageType type) {
		myType = type;
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
	
	public void setTiles(HashSet<Tile> tiles) {
		
	}
	
	public void newPath() {
		
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

	}
	[RPC]
	public void hireVillager(int q, int r) { 
		Tile tempTile = null;
		foreach(Tile t in board.getMap().Values){
			if(t.pos.q == q && t.pos.r == r){
				tempTile = t;
			}
		}
		Unit u = Instantiate(unitPrefab, new Vector3(0,0,0), Quaternion.identity) as Unit;
		u.board = this.board;
		u.setVillage(tempTile.getVillage());
		u.setUnitType(UnitType.Peasant);
		u.setActionType (ActionType.ReadyForOrders); 
		u.setTile (tempTile);
		u.placeUnit();
		addUnit(u);
		tempTile.getVillage().isActive = false;
	}

	public void mergeWith(Village v){
		if(this.getVillageType() > v.getVillageType()){
			Debug.Log("THIS WILL STAND FOR IT IS MORE UPGRADED");
		}
		else if (this.getVillageType() < v.getVillageType()){
			Debug.Log("OTHER WILL STAND FOR IT IS MORE UPGRADED");
		}
		else if(this.getTiles().Count >= v.getTiles().Count){
			Debug.Log("THIS WILL STAND FOR IT IS LARGER");
		}
		else{
			Debug.Log("OTHER WILL STAND FOR IT IS LARGER");
		}
	}
	void OnMouseUp () {
		if(!this.isActive){
			this.isActive = true;
			this.transform.GetChild(0).renderer.material.color = Color.black;
			this.transform.GetChild(1).renderer.material.color = Color.black;
			this.transform.GetChild(2).renderer.material.color = Color.black;
		
			foreach(Tile t in tiles){
				if((t.getLandType() == LandType.Grass || t.getLandType() == LandType.Meadow) && t != this.getStructTile()){
					t.setAcceptsUnit(true);
					t.transform.GetChild(0).renderer.material.color = Color.black;
				}
			}
		}
		else
		{
			this.isActive = false;
			this.transform.GetChild(0).renderer.material.color = Color.clear;
			this.transform.GetChild(1).renderer.material.color = Color.clear;
			this.transform.GetChild(2).renderer.material.color = Color.clear;
			
			foreach(Tile t in tiles){
				if((t.getLandType() == LandType.Grass || t.getLandType() == LandType.Meadow) && t != this.getStructTile()){
					t.setAcceptsUnit(false);
					t.transform.GetChild(0).renderer.material.color = t.getVillage().getOwner().getColor();
				}
			}
		}
		if (getUpgradable ()){
			GetComponent<PhotonView>().RPC("upgradeVillage", PhotonTargets.All, this.getStructTile().pos.q, this.structTile.pos.r);
			//upgradeVillage(this.getStructTile().pos.q, this.structTile.pos.r); 
		}
		else if (getVillageType() != VillageType.Fort) {
			setUpgradable (true);
		}
	}

	[RPC]
	public void moveUnit(int q, int r, int q1, int r1){
		Tile tempTile = board.getTile(new Hex(q, r));
		board.selectedUnit = tempTile.getUnit();
		board.selectedUnit.tile.getOwner();
		Tile tempTile2 = board.getTile(new Hex(q1, r1));
	//	foreach(Tile t in board.getMap().Values){
	//		if(t.pos.q == q && t.pos.r == r){
		//		tempTile = t;
		//	}
		//	else if(t.pos.q == q1 && t.pos.r == r1){
		//		tempTile2 = t;
			//}
		//}
		tempTile.getUnit().MoveTo(tempTile2);
	}

	[RPC]
	void upgradeVillage(int q, int r) {
		Tile tempTile = null;
		foreach (Tile t in board.getMap().Values) {
			if (t.pos.q == q && t.pos.r == r) {
				tempTile = t;
			}
		}
		
		if (tempTile.getVillage().getVillageType() == VillageType.Hovel && tempTile.getVillage().getWood() >= 1) { 
			tempTile.getVillage().transform.GetChild(0).gameObject.SetActive(false);
			tempTile.getVillage().transform.GetChild(1).gameObject.SetActive(true);
			tempTile.getVillage().setVillageType(VillageType.Town);
			tempTile.getVillage().changeWood(-1);
			tempTile.getVillage().setUpgradable(false);
			tempTile.getVillage().transform.GetChild(0).renderer.material.color = Color.clear;
			tempTile.getVillage().transform.GetChild(1).renderer.material.color = Color.clear;
			tempTile.getVillage().transform.GetChild(2).renderer.material.color = Color.clear;
			foreach (Tile t in tempTile.getVillage().getTiles()) {
				t.setAcceptsUnit(false);
				t.transform.GetChild(0).renderer.material.color = tempTile.getVillage().getOwner().getColor();
				
			}
		} else if (tempTile.getVillage().getVillageType() == VillageType.Town && tempTile.getVillage().getWood() >= 1) { 
			tempTile.getVillage().transform.GetChild(1).gameObject.SetActive(false);
			tempTile.getVillage().transform.GetChild(2).gameObject.SetActive(true);
			tempTile.getVillage().setVillageType(VillageType.Fort);
			tempTile.getVillage().changeWood(-1);
			tempTile.getVillage().setUpgradable(false);
			tempTile.getVillage().transform.GetChild(0).renderer.material.color = Color.clear;
			tempTile.getVillage().transform.GetChild(1).renderer.material.color = Color.clear;
			tempTile.getVillage().transform.GetChild(2).renderer.material.color = Color.clear;
			foreach (Tile t in tempTile.getVillage().getTiles()) {
				t.setAcceptsUnit(false);
				t.transform.GetChild(0).renderer.material.color = tempTile.getVillage().getOwner().getColor();
				
			}
		}

	}

}

