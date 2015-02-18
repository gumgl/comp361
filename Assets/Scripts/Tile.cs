using UnityEngine;
using System.Collections.Generic;


public class Tile {
	
	public List<Tile> neighbours;
	public int x;
	public int y;
	
	public Tile(){
		neighbours = new List<Tile>();
	}
	
	public float DistanceTo(Tile n){
		return Vector2.Distance (new Vector2(x,y), new Vector2(n.x,n.y));
	}


}