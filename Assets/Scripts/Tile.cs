using UnityEngine;
using System.Collections.Generic;


public class Tile : MonoBehaviour {
	public List<Tile> neighbours;
	public int x;
	public int y;
	public Board map;
	public LandType type;
	
	public Tile() {
		neighbours = new List<Tile>();
	}
	
	public float DistanceTo(Tile n) {
		return Vector2.Distance(new Vector2(x, y), new Vector2(n.x, n.y));
	}
		
	void OnMouseUp() {   
		map.GeneratePathTo(x, y);
	}
}