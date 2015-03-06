using UnityEngine;
using System.Collections.Generic;

public class Tile : MonoBehaviour {
	public Dictionary<Hex.Direction, Tile> neighbours = new Dictionary<Hex.Direction, Tile>();
	public Hex pos;
	public Board map;
	public Unit unit;
	public Village village;  
	public Structure structure; 
	public LandType type;
	static public float size = 1;
	private Player owner;

	public Tile() {
	}
	public bool hasStructure () {
		if (this.structure != Structure.NoStructure) return true;
		else return false;
	}	
	public Structure getStructure () { 
		return this.structure;	
	}
	public void setStructure (Structure s) { 
		if (getStructure () != s) { 
			this.structure = s;
		}
		//else do nothing
	}
	public void removeStructure () { 
		if (hasStructure()) {
			this.structure = Structure.NoStructure;
		}
		//else do nothing
	}
	public void upgradeStruct (Structure s) { //TODO
		//need to rank enum type Structures so that we can prevent downgrading etc. 
	}
	public void setLandType (LandType t) {
		if (getLandType() != t) {
			this.type = t;
		}
	}
	public LandType getLandType () { 
		return this.type;
	}
	public Dictionary<Hex.Direction, Tile> getNeighbours () { 
		return this.neighbours;
	}
	public Unit getUnit () {
		return this.unit; 
	}
	public void setUnit (Unit u){
		this.unit = u;
	}
	public void removeUnit (Unit u) { 
		this.unit = null; 
	}
	public Village getVillage () {
		return this.village;
	}
	public float getWidth() {
		return size * 2;
	}
	public float getHeight() {
		return Mathf.Sqrt(3) / 2 * getWidth();
	}
	public Vector2 getPixelPos() {
		return HexToPixel(pos);
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
		return HexDistance(this.pos, tile.pos);
	}
	public Vector2 HexCorner(int i) {
		float angle = 2.0f * Mathf.PI / 6.0f * (i + 0.5f);
		Vector2 center = HexToPixel(pos);
		return new Vector2(center.x + size * Mathf.Cos(angle), center.y + size * Mathf.Sin(angle));
	}

	public Player getOwner() {
		return owner;
	}

	public void setOwner(Player p) {
		if(p != null){
			owner = p;
			transform.GetChild(0).renderer.material.color = p.getColor();
		}
	}

	public void clearOwner(){
		transform.GetChild(0).renderer.material.color = Color.white;
		owner = null;
	}

	public int numAdjacentOwnedTiles(){
		int i = 0;
		foreach (Hex.Direction dir in System.Enum.GetValues(typeof(Hex.Direction))) {
			if(this.owner == getNeighbour(dir).owner)
				i++;
		}
		return i;
	}
	void OnMouseEnter() {
		map.distanceText.text = HexDistanceTo(map.selectedUnit.tile).ToString();
	}

	void OnMouseDown() {
		Debug.Log("clicked a tile!");
		map.GeneratePathTo(this);
		Debug.Log(this.numAdjacentOwnedTiles());
	}
    
	static public int HexDistance(Tile a, Tile b) {
		return HexDistance(a.pos, b.pos);
	}
	static public int HexDistance(Hex a, Hex b) {
		return (Mathf.Abs(a.q - b.q) + Mathf.Abs(a.q + a.r - b.q - b.r) + Mathf.Abs(a.r - b.r)) / 2;
	}
	static public Vector2 HexToPixel(Hex hex) {
		//Vector2 dq = new Vector2(3.0f / 2.0f, 1.0f);
		//Vector2 dr = new Vector2(0, 2.0f);
		//return size * (dq * hex.q + dr * hex.r);
		float x = size * 3.0f / 2.0f * hex.q;
		float y = size * Mathf.Sqrt(3) * (hex.r + hex.q / 2.0f);
		return new Vector2(x, y);
	}
}