﻿using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using SimpleJSON;

public class Tile : Photon.MonoBehaviour {
	public Dictionary<Hex.Direction, Tile> neighbours = new Dictionary<Hex.Direction, Tile>();
	public Hex pos;
	public Board board;
	public Unit unit;
	Village village = null;  
	public Structure structure = Structure.None;
	public LandType type;
	static public float size = 1;
	bool acceptsUnit = false;
	
	public Button PeasantButton;
	public Button InfantryButton;
	public Button SoldierButton;
	public Button KnightButton;
	public Button TowerButton;
	public Button CannonButton;


	public JSONNode Serialize() {
		var node = new JSONClass();

		node["pos"] = pos.Serialize();
		node["structure"].AsInt = (int) getStructure();
		node["type"].AsInt = (int) getLandType();

		return node;
	}

	public void UnSerialize(JSONNode node)
	{
		pos = new Hex(node["pos"]);
		setStructure((Structure) node["structure"].AsInt);
		setLandType((LandType) node["type"].AsInt);
		transform.position = board.TileCoordToWorldCoord(getPixelPos());
	}

	public bool hasStructure() {
		if (this.structure != Structure.None)
			return true;
		else
			return false;
	}	
	public Structure getStructure() { 
		return this.structure;	
	}
	public void setStructure(Structure s) { 
			this.structure = s;
	}
	public void removeStructure() { 
		if (hasStructure()) {
			this.structure = Structure.None;
		}
		//else do nothing
	}
	public void upgradeStruct(Structure s) { //TODO
		//need to rank enum type Structures so that we can prevent downgrading etc. 
	}

	[RPC]
	public void setLandType(LandType t) {
		this.type = t;
		this.transform.GetChild(1).gameObject.SetActive(false);
		this.transform.GetChild(2).gameObject.SetActive(false);
		this.transform.GetChild(3).gameObject.SetActive(false);
		this.transform.GetChild(4).gameObject.SetActive(false);

		if (t == LandType.Water) {
			transform.GetChild(0).renderer.material.color = Color.blue;
		} else if (t == LandType.Meadow) {
			this.transform.GetChild(1).gameObject.SetActive(true);
			this.transform.GetChild(1).localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
		} else if (t == LandType.Tree) {
			this.transform.GetChild(2).gameObject.SetActive(true);
			this.transform.GetChild(2).localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
		} else if (t == LandType.Tombstone){
			this.transform.GetChild(3).gameObject.SetActive(true);
			this.transform.GetChild(3).localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
		} else if (t == LandType.Road){
			this.transform.GetChild(4).gameObject.SetActive(true);
		} else if (t == LandType.RoadMeadow){
			this.transform.GetChild(1).gameObject.SetActive(true);
			this.transform.GetChild(4).gameObject.SetActive(true);
		}
	}
	public LandType getLandType() { 
		return this.type;
	}
	public Dictionary<Hex.Direction, Tile> getNeighbours() { 
		return this.neighbours;
	}
	
	public void setAcceptsUnit(bool b) {
		acceptsUnit = b;
	}
	
	public Unit getUnit() {
		return this.unit; 
	}
	public void setUnit(Unit u) {
		this.unit = u;
	}
	public void removeUnit(Unit u) { 
		this.unit = null; 
	}
	public Village getVillage() {
		return this.village;
	}
	public void setVillage(Village v) {
		village = v;
		if (v == null)
			transform.GetChild(0).renderer.material.color = Color.white;
		else
			transform.GetChild(0).renderer.material.color = getOwner().getColor();
	}
	public float getWidth() {
		return size * 2;
	}
	public float getHeight() {
		return Mathf.Sqrt(3) / 2 * getWidth();
	}
	public Vector2 getPixelPos() {
		return pos.ToPixel();
	}
	public Tile getNeighbour(Hex.Direction dir) {
		return neighbours[dir];
	}
	public void setNeighbour(Hex.Direction dir, Tile neighbour) {
		neighbours[dir] = neighbour;
	}
	public float DistanceTo(Tile tile) {
		return Vector2.Distance(this.getPixelPos(), tile.getPixelPos());
	}
	public int HexDistanceTo(Tile tile) {
		return Hex.Distance(this.pos, tile.pos);
	}
	public Vector2 HexCorner(int i) {
		float angle = 2.0f * Mathf.PI / 6.0f * (i + 0.5f);
		Vector2 center = pos.ToPixel();
		return new Vector2(center.x + size * Mathf.Cos(angle), center.y + size * Mathf.Sin(angle));
	}
	public bool canWalkThrough(Unit unit) {
		if (getLandType().isMovementAllowed() && getOwner() == unit.getTile().getOwner() && this.containsEnemyInNeighbour(this) == null) {
			// TODO: check that units in neighbouring tiles are not of higher level
			return true;
		} else
			return false;
	}
	public bool canEnter(Unit unit) {
		// TODO: check that units in neighbouring tiles are not of higher level
		return (getLandType() != LandType.Water);
	}
	
	/*
	* FOR PAUL: checks that the unit around target tile is enemy to owner of "this" tile. No bugs as far I can tell. 
	*/
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

	public void killTile(){
		if(this.getUnit() != null){
			GameObject.Destroy(this.getUnit().gameObject);
			this.setLandType(LandType.Tombstone);
		}
		this.getVillage().removeTile(this);
		this.setVillage(null);
	}

	//Returns a neighbouring tile that is of the same owner but from a different village
	//Used to merge villages. Returns null if no merge is neccessary
	public HashSet<Tile> getAdjacentFriendlyBorder(){
		HashSet<Tile> borderTiles = new HashSet<Tile>();
		foreach (KeyValuePair<Hex.Direction, Tile> entry in this.getNeighbours()) {
			Tile neighbour = entry.Value;
			if (neighbour.getOwner() == this.getOwner() && neighbour.getVillage() != this.getVillage()){
				//NOT FINISHED MUST IGNORE MULTIPLE TILES FROM SAME VILLAGE
				bool sameVillage = false;
				if(borderTiles.Count > 0){
					foreach(Tile tile in borderTiles){
						if(tile.getVillage() == neighbour.getVillage())
							sameVillage = true;
					}
				}
				if(!sameVillage){
					borderTiles.Add(neighbour);
				}
			}
		}
		return borderTiles;
	}

	public Player getOwner() {
		if(getVillage() == null)
			return null;
		return getVillage().getOwner();
	}

	void OnMouseEnter() {
		if(this.getOwner() == this.transform.root.GetComponent<Game>().GetLocalPlayer()){
			if(this.getVillage() != null){
				board.woodText.text = "Wood: " + this.getVillage().getWood().ToString();
				board.goldText.text = "Gold: " + this.getVillage().getGold().ToString();
			}
			else{
				board.woodText.text = "";
				board.goldText.text = "";
			}
		}
		else{
			board.woodText.text = "";
			board.goldText.text = "";
		}
	}

	//void OnMouseExit(){
	//	transform.GetChild(0).renderer.material.color = owner.getColor();
	//}

	static public int HexDistance(Tile a, Tile b) {
		return Hex.Distance(a.pos, b.pos);
	}


	void Update () {
		if(this.acceptsUnit){
			if(Input.GetKeyDown("1"))
				this.getOwner().setUnitToBuild(0);
			else if(Input.GetKeyDown("2"))
				this.getOwner().setUnitToBuild(1);
			else if(Input.GetKeyDown("3") && this.getVillage().getVillageType() >= VillageType.Town)
				this.getOwner().setUnitToBuild(2);
			else if(Input.GetKeyDown("4") && this.getVillage().getVillageType() >= VillageType.Fort)
				this.getOwner().setUnitToBuild(3);
			else if(Input.GetKeyDown("5") && this.getVillage().getVillageType() >= VillageType.Castle)
				this.getOwner().setUnitToBuild(4);
			else if(Input.GetKeyDown("6") && this.getVillage().getVillageType() >= VillageType.Town)
				this.getOwner().setUnitToBuild(5);
		}
	}

	void OnMouseDown() {
		//Debug.Log(this.getVillage().getTiles().Count);
	}
	
	void OnMouseUp() {
//		Debug.Log("Tile " + pos.ToString() + " OnMouseUp");
		//if(EventSystem.current.IsPointerOverGameObject()){
		if(this.transform.root.GetComponent<Game>().GetCurrPlayer() == this.transform.root.GetComponent<Game>().GetLocalPlayer()){
			if (board.selectedUnit != null) {
				//Debug.Log(board.selectedUnit.getVillage());
				board.selectedUnit.getVillage().GetComponent<PhotonView>().RPC("moveUnit", PhotonTargets.All, board.selectedUnit.getTile().pos.q, board.selectedUnit.getTile().pos.r, this.pos.q, this.pos.r); 
				//board.selectedUnit.MoveTo(this);
			}
			if(this.acceptsUnit && this.getOwner().getUnitToBuild() != 6){
				if(this.getVillage().getStructTile().HexDistanceTo(this) > 1.8 && this.getOwner().getUnitToBuild() == 4){
					board.setErrorText ("Cannons must be built adjacent to castles.");
				}
				else{
					village.GetComponent<PhotonView>().RPC("hireVillager", PhotonTargets.All, this.pos.q, this.pos.r, this.getOwner().getUnitToBuild());
					village.setUpgradable(false);
				
					this.getOwner().setUnitToBuild(6);
					foreach(Tile t in village.getTiles()){
						t.setAcceptsUnit(false);
						t.transform.GetChild(0).renderer.material.color = village.getOwner().getColor();
						t.transform.GetChild(1).renderer.material.color = village.getOwner().getColor();
						t.transform.GetChild(2).renderer.material.color = village.getOwner().getColor();
						village.transform.GetChild(0).renderer.material.color = Color.green;
					}
				}
			}
		}
		//}
	}
}