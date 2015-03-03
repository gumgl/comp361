using UnityEngine;
using System.Collections.Generic;
using System.Linq;


public class Board : MonoBehaviour {

	public GameObject selectedUnit;
	public Tile[] landTypePrefabs;

	LandType[,] tileTypes;
	Tile[,] graph;

	int mapSizeX = 20;
	int mapSizeY = 20;

	static float tileHeight = 3.0f;
	static float tileWidth = Mathf.Sqrt(3) / 2 * tileHeight;

	void Start() {

		selectedUnit.GetComponent<Unit>().tileX = (int)selectedUnit.transform.position.x;
		selectedUnit.GetComponent<Unit>().tileY = (int)selectedUnit.transform.position.y;
		selectedUnit.GetComponent<Unit>().map = this;
		ProcedurallyGenerateWorld();
		GenerateGraph();
		DrawWorld();
	}

	void ProcedurallyGenerateWorld() {
		
		tileTypes = new LandType[mapSizeX, mapSizeY];
		
		/*for (int x=0; x < mapSizeX; x++) {
			tiles[x, 0] = LandType.Water;
			tiles[x, mapSizeY - 1] = LandType.Water;
		}
		
		int random;
		for (int y=1; y < mapSizeY-1; y++) {
			tiles[0, y] = LandType.Water;
			tiles[mapSizeX - 1, y] = LandType.Water;
			
			
			if (y == 1 || y == mapSizeY - 2) {
				for (int x=1; x < mapSizeX-1; x++) {
					random = Random.Range(1, 10);
					if (random <= 2) {
						tiles[x, y] = LandType.Water;
					}
				}
			}
			random = Random.Range(1, 10);
			if (random <= 2) {
				tiles[1, y] = LandType.Water;
			}
			if (random <= 2) {
				tiles[mapSizeX - 2, y] = LandType.Water;
			}
		}
		
		for (int y=1; y < mapSizeY-1; y++) {
			for (int x=1; x < mapSizeX-1; x++) {
				random = Random.Range(1, 10);
				if (random <= 2) {
					tiles[x, y] = LandType.Grass;
				} else if (random == 9) {
					tiles[x, y] = LandType.Tree;
				}
			}
		} */

		for (int y=1; y < mapSizeY-1; y++) {
			for (int x=1; x < mapSizeX-1; x++) {
				if (x == 0 || x == mapSizeY - 1 || y == 0 || y == mapSizeY - 1)
					tileTypes[x, y] = LandType.Water;
				else {
					float random = Random.Range(0.0f, 100.0f);
					if (random <= 20)
						tileTypes[x, y] = LandType.Tree;
					else if (random <= 30)
						tileTypes[x, y] = LandType.Meadow;
				}
			}
		}
	}

	public float CostToEnterTile(int sourceX, int sourceY, int targetX, int targetY) {

		LandType tt = tileTypes[targetX, targetY];

		if (UnitCanEnterTile(targetX, targetY) == false) {
			return Mathf.Infinity;
		}

		float cost = tt.getMovementCost();

		if (sourceX != targetX && sourceY != targetY) {
			cost += 0.001f;
		}

		return cost;
	}


	void GenerateGraph() {
		graph = new Tile[mapSizeX, mapSizeY];

		for (int y=0; y < mapSizeY; y++) {
			for (int x=0; x < mapSizeX; x++) {

				graph[x, y] = new Tile();
			}
		}

		for (int y=0; y < mapSizeY; y++) {
			for (int x=0; x < mapSizeX; x++) {				



				graph[x, y].x = x;
				graph[x, y].y = y;

				if (x > 0) {
					graph[x, y].neighbours.Add(graph[x - 1, y]);
				}
				if (x < mapSizeX - 1) {
					graph[x, y].neighbours.Add(graph[x + 1, y]);
				}
				if (y > 0) {
					graph[x, y].neighbours.Add(graph[x, y - 1]);
				}
				if (y < mapSizeY - 1) {
					graph[x, y].neighbours.Add(graph[x, y + 1]);
				}

				if (y % 2 == 0) {
					if (x - 1 >= 0 && y - 1 >= 0) {
						graph[x, y].neighbours.Add(graph[x - 1, y - 1]);
					}
					if (x - 1 >= 0 && y != mapSizeY - 1) {
						graph[x, y].neighbours.Add(graph[x - 1, y + 1]);
					}
				} else {
					if (x != mapSizeX - 1 && y - 1 >= 0) {
						graph[x, y].neighbours.Add(graph[x + 1, y - 1]);
					}
					if (x != mapSizeX - 1 && y != mapSizeY - 1) {
						graph[x, y].neighbours.Add(graph[x + 1, y + 1]);
					}
				}
			}
		}
	}

	void DrawWorld() {
		for (int y=0; y < mapSizeY; y++) {
			for (int x=0; x < mapSizeX; x++) {
				LandType tt = tileTypes[x, y];
				Tile tile = (Tile)Instantiate(
                    landTypePrefabs[(int)tt], 
                    new Vector3(x * tileWidth + (y % 2 * tileWidth / 2f), 0, y * 0.75f * tileHeight), 
                    Quaternion.Euler(0f, 90f, 0f));
				tile.x = x;
				tile.y = y;
				tile.map = this;
				tile.type = tileTypes[x, y];
				graph[x, y] = tile;
			}
		}
	}

	public Vector3 TileCoordToWorldCoord(int x, int y) {
		return new Vector3(x * tileWidth + (y % 2 * tileWidth / 2f), 0, y * 0.75f * tileHeight);
	}

	public bool UnitCanEnterTile(int x, int y) {

		// we should test a unit's walktype on a clickable tile

		return false; // tileTypes [tiles [x, y]].isWalkable;

	}



	public void GeneratePathTo(int x, int y) {

		selectedUnit.GetComponent<Unit>().currentPath = null;

		if (UnitCanEnterTile(x, y) == false) {
			return;
		}

		Dictionary<Tile, float> dist = new Dictionary<Tile, float>();
		Dictionary<Tile, Tile> prev = new Dictionary<Tile, Tile>();

		List<Tile> unvisited = new List<Tile>();

		Tile source = graph[selectedUnit.GetComponent<Unit>().tileX, selectedUnit.GetComponent<Unit>().tileY];

		Tile target = graph[x, y];

		dist[source] = 0;
		prev[source] = null;

		foreach (Tile v in graph) {
			if (v != source) {
				dist[v] = Mathf.Infinity;
				prev[v] = null;
			}

			unvisited.Add(v);

		}

		while (unvisited.Count > 0) {

			Tile u = null;

			foreach (Tile possibleU in unvisited) {
				if (u == null || dist[possibleU] < dist[u]) {
					u = possibleU;
				}
			}

			if (u == target) {
				break;
			}

			unvisited.Remove(u);

			foreach (Tile v in u.neighbours) {

				//pure distance approach
				//float alt = dist[u] + u.DistanceTo(v);

				//weighted move cost approach
				float alt = dist[u] + CostToEnterTile(u.x, u.y, v.x, v.y);
				if (alt < dist[v]) {
					dist[v] = alt;
					prev[v] = u;
				}
			}
		}

		if (prev[target] == null) {
			// unreachable
			return;
		}

		List<Tile> currentPath = new List<Tile>();

		Tile curr = target;

		while (curr != null) {
			currentPath.Add(curr);
			curr = prev[curr];
		}

		currentPath.Reverse();

		selectedUnit.GetComponent<Unit>().currentPath = currentPath;
	}
}
