using UnityEngine;
using System.Collections.Generic;

public class Tile : MonoBehaviour {
	public Dictionary<Hex.Direction, Tile> neighbours = new Dictionary<Hex.Direction, Tile>();
	public Hex pos;
	public Board map;
	public LandType type;
	static public float size = 1;
    


	public Tile() {
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

	void OnMouseEnter() {
		map.distanceText.text = HexDistanceTo(map.selectedUnit.tile).ToString();
	}

	void OnMouseDown() {
		Debug.Log("clicked a tile!");
		map.GeneratePathTo(this);
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