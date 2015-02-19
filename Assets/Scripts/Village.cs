using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Village : MonoBehaviour {
    VillageType myType;
    int gold = 0;
    int wood = 0;
    bool cultivating = false;
    bool building = false;
    Player owner; // Should be set in constructor
    HashSet<Tile> tiles;
    HashSet<Unit> units;
    Tile structTile; // Where the HQ is

    public HashSet<Tile> getTiles() {
        return tiles;
    }
	
    public void delete() {
		
    }
	
    public void setStructTile(Tile t) {
        structTile = t;
    }
	
    public Tile getStructTile() {
        return structTile;
    }
	
    public void addTile(Tile t) {
        tiles.Add(t);
    }
	
    public void addTiles(HashSet<Tile> tilesToAdd) {
        foreach (Tile t in tilesToAdd) {
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
		
    }
	
    public void buildPhase(HashSet<Tile> tiles) {
		
    }
	
    public void incomePhase(HashSet<Tile> tiles) {
		
    }
	
    public void paymentPhase(HashSet<Tile> tiles) {
		
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
	
    public void upgradeUnit(Unit u, UnitType newLevel) {
		
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
        units.Add(u);
    }
	
    public HashSet<Tile> getPath() {
        return new HashSet<Tile>();
    }


    void Start() {

    }
    void Update() {

    }
}

