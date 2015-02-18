using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Village : MonoBehaviour
{
	VillageType myType;
	int gold = 0;
	int wood = 0;
	bool cultivating = false;
	bool building = false;
	Player owner; // Should be set in constructor
	HashSet<Tile> tiles;
	HashSet<Unit> units;
	Tile structTile; // Where the HQ is

	public Set<Tile> getTiles() {
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
		
	}
	
	public void addTiles(Set<Tile> tiles) {
		
	}
	
	public void removeTile(Tile dest) {
		
	}
	
	public Set<Unit> getUnits() {
		
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
	
	public void addGold(int gold) {
		
	}
	
	public void addWood() {
		
	}
	
	public void decreaseGold(int gold) {
		
	}
	
	public int getGold() {
		
	}
	
	public void setResources(int gold, int wood) {
		
	}
	
	public void addResources(int gold, int wood) {
		
	}
	
	public void buildRoad(Unit u) {
		
	}
	
	public int getWood() {
		
	}
	
	public void setVillageType(VillageType type) {
		
	}
	
	public void decreaseWood(int wood) {
		
	}
	
	public void upgradeUnit(Unit u, UnitType newLevel) {
		
	}
	
	public int[] getResources() {
		
	}
	
	public void checkForBreak(HashSet<Tile> tiles) {
		
	}
	
	public Village checkForMerge() {
		
	}
	
	public Player getOwner() {
		
	}
	
	public void setTiles(HashSet<Tile> tiles) {
		
	}
	
	public void newPath() {
		
	}
	
	public HashSet<Tile> findPath(Tile start, Tile dest) {
		
	}
	
	public void addUnit(Unit u) {
		
	}
	
	public HashSet<Tile> getPath() {
		
	}


	void Start () {

	}
	void Update () {

	}
}

