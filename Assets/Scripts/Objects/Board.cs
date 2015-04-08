using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class Board : Photon.MonoBehaviour {
	#region Prefabs
	public Village villagePrefab;
	public Tile landPrefab;
	#endregion

	public Game game;
	public Unit selectedUnit;
	public UnityEngine.UI.Text distanceText;
	Village activeVillage; 
	//LandType[,] tileTypes;
	//Tile[,] grid;
	Dictionary<Hex, Tile> map;

//	int mapSizeX = 10;
//	int mapSizeY = 10;
	int mapRadius = 8;

//	static float tileHeight = 3.0f;
//	static float tileWidth = Mathf.Sqrt(3) / 2 * tileHeight;

	public void init(int seed) {
		Debug.Log("Board init!");
		//selectedUnit.GetComponent<Unit>().tileX = (int)selectedUnit.transform.position.x;
		//selectedUnit.GetComponent<Unit>().tileY = (int)selectedUnit.transform.position.y;
		generateHexagonalGrid(seed);
		connectNeighbours();
        createVillages();
	}

	void Start() {}
  
	void Update() {}

	public Dictionary<Hex, Tile> getMap() {
		return map;
	}
	
	public Village getActiveVillage() { 
		return activeVillage; 	
	}
	
	public void setActiveVillage(Village v) {
		this.activeVillage = v;
	}

	public void generateHexagonalGrid(int sd) {
		Debug.Log("Generating grid with seed " + sd.ToString());
		Random.seed = sd;
		map = new Dictionary<Hex, Tile>();
		for (int q = -mapRadius; q <= mapRadius; q++) {
			for (int r = -mapRadius; r <= mapRadius; r++) {
				if (Mathf.Abs(q + r) <= mapRadius) { // Within hex-shaped grid
					Hex pos = new Hex(q, r);
					LandType tt;
					if (Mathf.Abs(q) == mapRadius || Mathf.Abs(r) == mapRadius || Mathf.Abs(q + r) == mapRadius)
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
					Tile tile = (Tile)Instantiate(
						landPrefab, 
						new Vector3(0, 0, 0), 
						Quaternion.Euler(0f, 0f, 0f));
					tile.pos = pos;
					tile.transform.parent = gameObject.transform;
					tile.transform.localPosition = TileCoordToWorldCoord(tile.getPixelPos());
					//if (tt != LandType.Water) {
					//	tile.setOwner(transform.parent.GetComponent<DemoGame>().getRandomPlayer());
					//}
					//tile.setOwner(null);
					tile.board = this;
					tile.setLandType(tt);
					setTile(pos, tile);
				}
			}
		}
	}

	public void connectNeighbours() {
		foreach (KeyValuePair<Hex, Tile> entry in map) {
			foreach (Hex.Direction dir in System.Enum.GetValues(typeof(Hex.Direction))) {
				Hex neighbourPos = entry.Key + dir.getDelta();
				if (map.ContainsKey(neighbourPos))
					entry.Value.setNeighbour(dir, getTile(neighbourPos));
			}
		}
	}

    private void createVillages()
    {
		// Here we now use a temporary (local) array instead of Tile.owner, because the owner is a property of the village
        Dictionary<Tile, Player> owners = new Dictionary<Tile, Player>();
        assignRandomOwners(owners);
        computeVillages(owners);
    }

    private void assignRandomOwners(Dictionary<Tile, Player> owners)
    {
	    foreach (KeyValuePair<Hex, Tile> entry in map) {
		    Player owner;
		    if (entry.Value.getLandType() == LandType.Water)
			    owner = null;
		    else
				owner = game.GetRandomOwner();
			owners.Add(entry.Value, owner);
	    }
    }

	void computeVillages(Dictionary<Tile, Player> owner)
	{
		// Remove non-villages (i.e. singles and doubles)
		foreach (KeyValuePair<Hex, Tile> entry in map) {
			Tile tile = entry.Value;
			var neighbours = getAdjacentOwnedTiles(tile, owner);
			if (tile.type == LandType.Water
				|| (neighbours.Count == 1 && getAdjacentOwnedTiles(neighbours[0], owner).Count == 1)
				|| neighbours.Count < 1)
				owner[tile] = null;
		}
		// Create the villages
		foreach (KeyValuePair<Hex, Tile> entry in map) {
			Tile tile = entry.Value;
			if (tile.getVillage() == null && owner[tile] != null)
			{
				Village newVillage = Instantiate(villagePrefab, TileCoordToWorldCoord(tile.getPixelPos()), Quaternion.Euler(1, Random.Range(0, 6) * 60, 1)) as Village;
				newVillage.init(owner[tile], this, VillageType.Hovel, 7, 0, tile);
				tile.setLandType(LandType.Grass);
				expandVillage(newVillage, tile, owner);
			}
		}
	}

	//Adds tiles to the village upon initial creation,using BFS
	void expandVillage(Village village, Tile tile, Dictionary<Tile, Player> owner) {
		List<Tile> toExplore = getAdjacentOwnedTiles(tile, owner);
		toExplore.Add(tile);
		while (toExplore.Count > 0) {
			Tile t = toExplore[0];
			toExplore.RemoveAt(0);
			t.setVillage(village);
			village.addTile(t);
			foreach (var adjacent in getAdjacentOwnedTiles(t, owner)) {
				if (adjacent.getVillage() == null)
					toExplore.Add(adjacent);
			}
		}
	}

	List<Tile> getAdjacentOwnedTiles(Tile tile, Dictionary<Tile, Player> owner) {
		List<Tile> tiles = new List<Tile>();
		foreach (KeyValuePair<Hex.Direction, Tile> entry in tile.getNeighbours()) {
			Tile neighbour = entry.Value;
			if (owner[tile] == owner[neighbour])
				tiles.Add(neighbour);
		}
		return tiles;
	}

	public Tile getTile(Hex pos) {
		return map[pos];
	}
	public void setTile(Hex pos, Tile tile) {
		map[pos] = tile;
	}

	public Vector3 TileCoordToWorldCoord(Vector2 pos) {
		//return new Vector3(x * tileWidth + (y % 2 * tileWidth / 2f), 0, y * 0.75f * tileHeight);
		return new Vector3(pos.x, 0, pos.y);
	}

	/*public bool UnitCanEnterTile(Tile target) {
		// we should test a unit's walktype on a clickable tile
		return true; // tileTypes [tiles [x, y]].isWalkable;
	}*/
}
