using UnityEngine;
using System.Collections.Generic;
using System.Linq;


public class TileMap : MonoBehaviour {

	public GameObject selectedUnit;

	public TileType[] tileTypes;

	int[,] tiles;
	Tile[,] graph;


	int mapSizeX = 20;
	int mapSizeY = 20;

	static float tileHeight = 3.0f;
	static float tileWidth = Mathf.Sqrt(3)/2 * tileHeight;

	void Start(){

		selectedUnit.GetComponent<UnitScript> ().tileX = (int)selectedUnit.transform.position.x;
		selectedUnit.GetComponent<UnitScript> ().tileY = (int)selectedUnit.transform.position.y;
		selectedUnit.GetComponent<UnitScript> ().map = this;
		ProcedurallyGenerateWorld ();
		GenerateGraph ();
		DrawWorld ();
	}

	void ProcedurallyGenerateWorld(){
		
		tiles = new int[mapSizeX, mapSizeY];
		
		for (int x=0; x < mapSizeX; x++) {
			tiles [x, 0] = 3;
			tiles [x, mapSizeY-1] = 3;
		}
		
		int random;
		for (int y=1; y < mapSizeY-1; y++) {
			tiles [0, y] = 3;
			tiles [mapSizeX-1, y] = 3;
			
			
			if(y == 1 || y == mapSizeY-2){
				for (int x=1; x < mapSizeX-1; x++) {
					random = Random.Range(1,10);
					if(random <= 2){
						tiles [x, y] = 3;
					}
				}
			}
			random = Random.Range(1,10);
			if(random <= 2){
				tiles [1, y] = 3;
			}
			if(random <= 2){
				tiles [mapSizeX-2, y] = 3;
			}
		}
		
		for (int y=1; y < mapSizeY-1; y++) {
			for (int x=1; x < mapSizeX-1; x++) {
				random = Random.Range(1,10);
				if(random <= 2){
					tiles [x, y] = 1;
				}else if (random == 9){
					tiles [x, y] = 2;
				}
			}
		}
	}

	public float CostToEnterTile(int sourceX, int sourceY, int targetX, int targetY){

		TileType tt = tileTypes [tiles [targetX, targetY]];

		if (UnitCanEnterTile(targetX, targetY) == false) {
			return Mathf.Infinity;
		}

		float cost = tt.movementCost;

		if (sourceX != targetX && sourceY != targetY) {
			cost += 0.001f;
		}

		return cost;
	}


	void GenerateGraph(){
				

		graph = new Tile[mapSizeX, mapSizeY];

		for (int y=0; y < mapSizeY; y++) {
			for (int x=0; x < mapSizeX; x++) {

				graph [x, y] = new Tile ();
			}
		}

		for (int y=0; y < mapSizeY; y++) {
			for (int x=0; x < mapSizeX; x++) {				


				graph[x,y].x = x;
				graph[x,y].y = y;

				if(x > 0){
					graph[x,y].neighbours.Add (graph[x-1,y]);
				}
				if(x < mapSizeX-1){
					graph[x,y].neighbours.Add (graph[x+1,y]);
				}
				if(y > 0){
					graph[x,y].neighbours.Add (graph[x,y-1]);
				}
				if(y < mapSizeY-1){
					graph[x,y].neighbours.Add (graph[x,y+1]);
				}

				if(y%2 == 0){
					if(x-1 >= 0 && y-1 >= 0){
						graph[x,y].neighbours.Add(graph[x-1,y-1]);
					}
					if(x-1 >= 0 && y != mapSizeY -1){
						graph[x,y].neighbours.Add(graph[x-1,y+1]);
					}
				}else{
					if(x != mapSizeX -1 && y-1 >= 0){
						graph[x,y].neighbours.Add(graph[x+1,y-1]);
					}
					if(x != mapSizeX -1 && y != mapSizeY -1){
						graph[x,y].neighbours.Add(graph[x+1,y+1]);
					}
				}
			}
		}
	}

	void DrawWorld(){


		for (int y=0; y < mapSizeY; y++) {
			for (int x=0; x < mapSizeX; x++) {
				TileType tt = tileTypes[tiles[x,y]];
				GameObject go = (GameObject)Instantiate (tt.tileVisualPrefab, new Vector3(x * tileWidth + (y%2 * tileWidth/2f) , 0, y * 0.75f * tileHeight), Quaternion.Euler(0f,90f,0f));
				ClickableTile ct = go.GetComponent<ClickableTile>();
				ct.tileX = x;
				ct.tileY = y;
				ct.map = this;
			}
		}


	}

	public Vector3 TileCoordToWorldCoord(int x, int y){
		return new Vector3(x * tileWidth + (y%2 * tileWidth/2f), 0, y * 0.75f * tileHeight);
	}

	public bool UnitCanEnterTile(int x, int y){

		// we should test a unit's walktype on a clickable tile

		return tileTypes[tiles[x,y]].isWalkable;

	}



	public void GeneratePathTo(int x, int y){

		selectedUnit.GetComponent<UnitScript> ().currentPath = null;

		if (UnitCanEnterTile (x, y) == false) {
			return;
		}

		Dictionary<Tile, float> dist = new Dictionary<Tile, float> ();
		Dictionary<Tile, Tile> prev = new Dictionary<Tile, Tile> ();

		List<Tile> unvisited = new List<Tile>();

		Tile source = graph [selectedUnit.GetComponent<UnitScript> ().tileX, selectedUnit.GetComponent<UnitScript> ().tileY];

		Tile target = graph [x, y];

		dist [source] = 0;
		prev [source] = null;

		foreach (Tile v in graph) {
			if(v != source){
				dist[v] = Mathf.Infinity;
				prev[v] = null;
			}

			unvisited.Add (v);

		}

		while (unvisited.Count > 0) {

			Tile u = null;

			foreach(Tile possibleU in unvisited){
				if (u == null || dist[possibleU] < dist[u]){
					u = possibleU;
				}
			}

			if(u == target){
				break;
			}

			unvisited.Remove (u);

			foreach(Tile v in u.neighbours){

				//pure distance approach
				//float alt = dist[u] + u.DistanceTo(v);

				//weighted move cost approach
				float alt = dist[u] + CostToEnterTile(u.x, u.y, v.x, v.y);
				if(alt < dist[v]){
					dist[v] = alt;
					prev[v] = u;
				}
			}
		}

		if (prev [target] == null) {
			// unreachable
			return;
		}

		List<Tile> currentPath = new List<Tile> ();

		Tile curr = target;

		while (curr != null) {
			currentPath.Add (curr);
			curr = prev[curr];
		}

		currentPath.Reverse ();

		selectedUnit.GetComponent<UnitScript> ().currentPath = currentPath;



	}
}
