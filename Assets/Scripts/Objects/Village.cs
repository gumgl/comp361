using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Village : MonoBehaviour {
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
	public void hireVillager(int q, int r, int type) { 
		Tile tempTile = null;
		foreach(Tile t in board.getMap().Values){
			if(t.pos.q == q && t.pos.r == r){
				tempTile = t;
			}
		}
		if(tempTile.getVillage().getGold() >= ((UnitType)type).getCost()){
			if((type == 4 && tempTile.getVillage().getWood() >= 12) || type != 4){
				tempTile.getVillage().changeGold(-((UnitType)type).getCost());
				if(type == 4)
					tempTile.getVillage().changeWood(-12);

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
			}
		}
		else{
			Debug.Log("NOT ENOUGH MONEY");
			tempTile.getVillage().isActive = false;
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
	
	void OnMouseUp () {
		Debug.Log(this.getVillageType());
		foreach(Village v in this.getOwner().getVillages())
			foreach(Unit u in v.getUnits()){
					board.selectedUnit = null;
					u.halo.SetActive(false);
			}		
		if(this.transform.root.GetComponent<Game>().GetCurrPlayer() == this.transform.root.GetComponent<Game>().GetLocalPlayer() && this.getOwner() == this.transform.root.GetComponent<Game>().GetLocalPlayer()){
		if(!this.isActive){
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

}

