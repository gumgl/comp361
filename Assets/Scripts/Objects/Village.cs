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
    Player owner; // Should be set in constructor
	HashSet<Tile> tiles = new HashSet<Tile>();
	HashSet<Unit> units = new HashSet<Unit>(); 
	Board board;
	public Tile structTile; // Where the HQ is
	public Unit unitPrefab;

	public void create(Player own, Board bor, VillageType type, int gl, int wd, Tile tile){
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

	public HashSet<Tile> getTiles() {
		return tiles;
	}
	
	public void delete() {
		Tile hq = getStructTile();
		if (hq.hasStructure()) {
			hq.removeStructure();
		}
		//set tile ownership to null for all tiles part of the village
		HashSet<Tile> tile = getTiles (); 
		foreach (Tile t in tile) {
			t.setOwner (null);
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
		t.setOwner(owner);
		t.setVillage(this);
		tiles.Add(t);
	}
	
	public void addTiles(HashSet<Tile> tilesToAdd) {
		foreach (Tile t in tilesToAdd) {
			t.setOwner(owner);
			t.setVillage(this);
			tiles.Add(t);
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
	public void hireVillager(Tile t) { 
		Unit u = Instantiate(unitPrefab, new Vector3(0,0,0), Quaternion.identity) as Unit;
		u.board = this.board;
		u.setVillage(this);
		u.setUnitType(UnitType.Peasant);
		u.setActionType (ActionType.ReadyForOrders); 
		u.setTile (t);
		u.placeUnit ();
		addUnit(u);
	}
	
	
	void OnMouseUp () { 
		//board.setActiveVillage(this);
		//this.isActive = true;
		this.transform.renderer.material.color = Color.black;
		foreach(Tile t in tiles){
			if(t.getLandType() == LandType.Grass || t.getLandType() == LandType.Meadow){
				t.setAcceptsUnit(true);
				t.transform.GetChild(0).renderer.material.color = Color.black;
			}
		}
	}
}

