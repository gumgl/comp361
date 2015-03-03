using UnityEngine;
using System.Collections.Generic;
using System.Linq;


public class Board : MonoBehaviour {

	public GameObject selectedUnit;
	public Tile[] landTypePrefabs;

	LandType[,] tileTypes;
	Tile[,] grid;

	int mapSizeX = 10;
	int mapSizeY = 10;

	static float tileHeight = 3.0f;
	static float tileWidth = Mathf.Sqrt(3) / 2 * tileHeight;

	void Start() {

		selectedUnit.GetComponent<Unit>().tileX = (int)selectedUnit.transform.position.x;
		selectedUnit.GetComponent<Unit>().tileY = (int)selectedUnit.transform.position.y;
		selectedUnit.GetComponent<Unit>().map = this;
		GenerateGrid();
	}
    
	void GenerateGrid() {
		grid = new Tile[mapSizeX, mapSizeY];
		tileTypes = new LandType[mapSizeX, mapSizeY];

		for (int q=0; q < mapSizeY; q++) {
			for (int r=0; r < mapSizeX; r++) {
				LandType tt;
				if (r == 0 || r == mapSizeY - 1 || q == 0 || q == mapSizeY - 1)
					tt = LandType.Water;
				else {
					float random = Random.Range(0.0f, 100.0f);
					if (random <= 20)
						tt = LandType.Tree;
					else if (random <= 30)
						tt = LandType.Meadow;
					else
						tt = LandType.Grass;
				}
				tileTypes[r, q] = tt;
				Tile tile = (Tile)Instantiate(
					landTypePrefabs[(int)tt], 
                    new Vector3(0, 0, 0), 
                    Quaternion.Euler(0f, 0f, 0f));
				tile.pos.q = q;
				tile.pos.r = r;
				tile.transform.parent = gameObject.transform;
				tile.transform.localPosition = TileCoordToWorldCoord(tile.getPixelPos());
				//tile.transform.localScale = new Vector3(0.666666f, 0.666666f, 0.666666f);
				tile.map = this;
				tile.type = tt;
				grid[q, r] = tile;
			}
		}

		for (int q=0; q < mapSizeY; q++) {
			for (int r=0; r < mapSizeX; r++) {

				if (r > 0) {
					grid[r, q].neighbours.Add(grid[r - 1, q]);
				}
				if (r < mapSizeX - 1) {
					grid[r, q].neighbours.Add(grid[r + 1, q]);
				}
				if (q > 0) {
					grid[r, q].neighbours.Add(grid[r, q - 1]);
				}
				if (q < mapSizeY - 1) {
					grid[r, q].neighbours.Add(grid[r, q + 1]);
				}

				if (q % 2 == 0) {
					if (r - 1 >= 0 && q - 1 >= 0) {
						grid[r, q].neighbours.Add(grid[r - 1, q - 1]);
					}
					if (r - 1 >= 0 && q != mapSizeY - 1) {
						grid[r, q].neighbours.Add(grid[r - 1, q + 1]);
					}
				} else {
					if (r != mapSizeX - 1 && q - 1 >= 0) {
						grid[r, q].neighbours.Add(grid[r + 1, q - 1]);
					}
					if (r != mapSizeX - 1 && q != mapSizeY - 1) {
						grid[r, q].neighbours.Add(grid[r + 1, q + 1]);
					}
				}
			}
		}
	}
    
	public float CostToEnterTile(Tile source, Tile target) {/*(int sourceX, int sourceY, int targetX, int targetY) {
		LandType tt = tileTypes[targetX, targetY];

		if (UnitCanEnterTile(targetX, targetY) == false)
			return Mathf.Infinity;

		float cost = tt.getMovementCost();

		if (sourceX != targetX && sourceY != targetY)
			cost += 0.001f;

		return cost;*/
		return 1.0f;
	}

	public Vector3 TileCoordToWorldCoord(Vector2 pos) {
		//return new Vector3(x * tileWidth + (y % 2 * tileWidth / 2f), 0, y * 0.75f * tileHeight);
		return new Vector3(pos.x, 0, pos.y);
	}

	public bool UnitCanEnterTile(Tile target) {
		// we should test a unit's walktype on a clickable tile
		return true; // tileTypes [tiles [x, y]].isWalkable;
	}



	public void GeneratePathTo(Tile target) {

		selectedUnit.GetComponent<Unit>().currentPath = null;

		if (UnitCanEnterTile(target) == false) {
			return;
		}

		Dictionary<Tile, float> dist = new Dictionary<Tile, float>();
		Dictionary<Tile, Tile> prev = new Dictionary<Tile, Tile>();

		List<Tile> unvisited = new List<Tile>();

		Tile source = grid[selectedUnit.GetComponent<Unit>().tileX, selectedUnit.GetComponent<Unit>().tileY];

		dist[source] = 0;
		prev[source] = null;

		foreach (Tile v in grid) {
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
				float alt = dist[u] + CostToEnterTile(u, v);
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
