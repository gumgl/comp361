using UnityEngine;
using System.Collections.Generic;

public class Tile : Photon.MonoBehaviour {
	public Dictionary<Hex.Direction, Tile> neighbours = new Dictionary<Hex.Direction, Tile>();
	public Hex pos;
	public Board board;
	public Unit unit;
	Village village = null;  
	public Structure structure;
	public LandType type;
	static public float size = 1;
	bool acceptsUnit = false;

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
		if (getStructure() != s) { 
			this.structure = s;
		}
		//else do nothing
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
		if (t == LandType.Water) {
			transform.GetChild(0).renderer.material.color = Color.blue;
		} else if (t == LandType.Meadow) {
			this.transform.GetChild(1).gameObject.SetActive(true);
			this.transform.GetChild(1).localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
		} else if (t == LandType.Tree) {
			this.transform.GetChild(2).gameObject.SetActive(true);
			this.transform.GetChild(2).localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
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
		if (getLandType().isMovementAllowed() && getOwner() == unit.getTile().getOwner()) {
			// TODO: check that units in neighbouring tiles are not of higher level
			return true;
		} else
			return false;
	}
	public bool canEnter(Unit unit) {
		// TODO: check that units in neighbouring tiles are not of higher level
		return (getLandType() != LandType.Water);
	}

	public Player getOwner() {
		return getVillage().getOwner();
	}

	/*public void setOwner(Player p) {
		if (p != null) {
			owner = p;
		}
	}

	public void clearOwner() {
		transform.GetChild(0).renderer.material.color = Color.white;
		owner = null;
	}*/
	/*
	public int numAdjacentOwnedTiles() {
		int i = 0;
		foreach (Hex.Direction dir in System.Enum.GetValues(typeof(Hex.Direction))) {
			if (this.owner == getNeighbour(dir).owner)
				i++;
		}
		return i;
	}

	//Returns the tile adjacent to the original tile if they both have the same owner and only one adjacent owned tile
	public Tile adjacentDuo() {
		foreach (Hex.Direction dir in System.Enum.GetValues(typeof(Hex.Direction))) {
			if (this.owner == getNeighbour(dir).owner && getNeighbour(dir).numAdjacentOwnedTiles() == 1)
				return getNeighbour(dir);
		}
		return null;
	}*/
	/*
	public HashSet<Tile> allAdjacentOwnedTiles() {
		HashSet<Tile> tiles = new HashSet<Tile>();
		foreach (Hex.Direction dir in System.Enum.GetValues(typeof(Hex.Direction))) {
			if (this.owner == getNeighbour(dir).owner) {
				tiles.Add(getNeighbour(dir));
			}
		}
		return tiles;
	}*/

/*	[RPC]
	public void deleteTree(int q, int r){
		Tile tempTile = null;
		foreach(Tile t in board.getMap().Values){
			if(t.pos.q == q && t.pos.r == r){
				tempTile = t;
			}
		}
		tempTile.setLandType(LandType.Grass);
		tempTile.getVillage().changeWood(2);
	}*/

	void OnMouseEnter() {
		if(this.getVillage() != null)
			board.distanceText.text = "Wood: " + this.getVillage().getWood().ToString();
		else
			board.distanceText.text = "";
	}
	//void OnMouseExit(){
	//	transform.GetChild(0).renderer.material.color = owner.getColor();
	//}

	static public int HexDistance(Tile a, Tile b) {
		return Hex.Distance(a.pos, b.pos);
	}
	
	void OnMouseUp() {
		if (board.selectedUnit != null) {
			//Debug.Log(board.selectedUnit.getVillage());
			board.selectedUnit.getVillage().GetComponent<PhotonView>().RPC("moveUnit", PhotonTargets.All, board.selectedUnit.getTile().pos.q, board.selectedUnit.getTile().pos.r, this.pos.q, this.pos.r); 
			//board.selectedUnit.MoveTo(this);
		}
		if(this.acceptsUnit){
			village.GetComponent<PhotonView>().RPC("hireVillager", PhotonTargets.All, this.pos.q, this.pos.r);
			village.setUpgradable(false);
			foreach(Tile t in village.getTiles()){
				t.acceptsUnit = false;
				t.transform.GetChild(0).renderer.material.color = village.getOwner().getColor();
				t.transform.GetChild(1).renderer.material.color = village.getOwner().getColor();
				t.transform.GetChild(2).renderer.material.color = village.getOwner().getColor();
				village.transform.GetChild(0).renderer.material.color = Color.green;
			}
		}
	}
}