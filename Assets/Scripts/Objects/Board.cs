using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class Board : Photon.MonoBehaviour {
	#region Prefabs
	public Village villagePrefab;
	public Unit unitPrefab;
	public Tile landPrefab;
	#endregion
	public Unit selectedUnit;
	public UnityEngine.UI.Text distanceText;
	Village activeVillage; 
	//LandType[,] tileTypes;
	//Tile[,] grid;
	Dictionary<Hex, Tile> map;

	int mapSizeX = 10;
	int mapSizeY = 10;
	int mapRadius = 8;

	static float tileHeight = 3.0f;
	static float tileWidth = Mathf.Sqrt(3) / 2 * tileHeight;

	void Start() {
		//selectedUnit.GetComponent<Unit>().tileX = (int)selectedUnit.transform.position.x;
		//selectedUnit.GetComponent<Unit>().tileY = (int)selectedUnit.transform.position.y;
		transform.parent.GetComponent<DemoGame>().initPlayers();
		GenerateHexGrid(int.Parse(PhotonNetwork.room.name));
		ConnectNeighbours();
		computeRegions();
		placeVillages();
		selectedUnit = (Unit)Instantiate(unitPrefab, Vector3.zero, Quaternion.identity);
		selectedUnit.setTile(getTile(new Hex(3, 3)));
		selectedUnit.transform.position = selectedUnit.getTile().transform.position + Vector3.up * 2;
		selectedUnit.setUnitType(UnitType.Peasant);
		selectedUnit.board = this;
	}
  
	void Update() {

	}

	public Dictionary<Hex, Tile> getMap(){
		return map;
	}
	
	public Village getActiveVillage() { 
		return activeVillage; 	
	}
	
	public void setActiveVillage(Village v) {
		this.activeVillage = v;
	}

	public void GenerateHexGrid(int sd) {
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
					if (tt != LandType.Water) {
						tile.setOwner(transform.parent.GetComponent<DemoGame>().getRandomPlayer());
					}
					//tile.setOwner(null);
					tile.board = this;
					tile.setLandType(tt);
					setTile(pos, tile);
				}
			}
		}
	}

	public void ConnectNeighbours() {
		foreach (KeyValuePair<Hex, Tile> entry in map) {
			foreach (Hex.Direction dir in System.Enum.GetValues(typeof(Hex.Direction))) {
				Hex neighbourPos = entry.Key + dir.getDelta();
				if (map.ContainsKey(neighbourPos))
					entry.Value.setNeighbour(dir, getTile(neighbourPos));
			}
		}
		/*for (int q=0; q < mapSizeY; q++) {
			for (int r=0; r < mapSizeX; r++) {
				
				if (r > 0) {
					getTile(new Hex(r, q)).neighbours.Add(getTile(new Hex(r - 1, q)));
				}
				if (r < mapSizeX - 1) {
					getTile(new Hex(r, q)).neighbours.Add(getTile(new Hex(r + 1, q)));
				}
				if (q > 0) {
					getTile(new Hex(r, q)).neighbours.Add(getTile(new Hex(r, q - 1)));
				}
				if (q < mapSizeY - 1) {
					getTile(new Hex(r, q)).neighbours.Add(getTile(new Hex(r, q + 1)));
				}
				
				if (q % 2 == 0) {
					if (r - 1 >= 0 && q - 1 >= 0) {
						getTile(new Hex(r, q)).neighbours.Add(getTile(new Hex(r - 1, q - 1)));
					}
					if (r - 1 >= 0 && q != mapSizeY - 1) {
						getTile(new Hex(r, q)).neighbours.Add(getTile(new Hex(r - 1, q + 1)));
					}
				} else {
					if (r != mapSizeX - 1 && q - 1 >= 0) {
						getTile(new Hex(r, q)).neighbours.Add(getTile(new Hex(r + 1, q - 1)));
					}
					if (r != mapSizeX - 1 && q != mapSizeY - 1) {
						getTile(new Hex(r, q)).neighbours.Add(getTile(new Hex(r + 1, q + 1)));
					}
				}
			}
		}*/
	}
	
	void computeRegions() {
		foreach (KeyValuePair<Hex, Tile> entry in map) {
			if (entry.Value.type != LandType.Water) {
				if (entry.Value.numAdjacentOwnedTiles() == 1 && entry.Value.adjacentDuo() != null) {
					entry.Value.adjacentDuo().clearOwner();
					entry.Value.clearOwner();
				} else if (entry.Value.numAdjacentOwnedTiles() < 1) {
					entry.Value.clearOwner();
				}
			}
		}
	}
	
	void placeVillages() {
		foreach (KeyValuePair<Hex, Tile> entry in map) {
			if (entry.Value.getVillage() == null && entry.Value.type != LandType.Water && entry.Value.getOwner() != null) {
				Village newVillage = Instantiate(villagePrefab, TileCoordToWorldCoord(entry.Value.getPixelPos()), Quaternion.identity) as Village;
				entry.Value.setVillage(newVillage);
				newVillage.create(entry.Value.getOwner(), this, VillageType.Hovel, 7, 0, entry.Value);
				entry.Value.setLandType(LandType.Grass);
				createVillageZone(entry.Value);
			}
		}
	}

	//Adds tiles to the village upon initial creation
	void createVillageZone(Tile tile) {
		HashSet<Tile> tiles = tile.allAdjacentOwnedTiles();
		foreach (Tile t in tiles) {
			if (t.getVillage() == null && t.getLandType() != LandType.Water) {
				t.setVillage(tile.getVillage());
				t.getVillage().addTile(t);
				createVillageZone(t);
			}
		}
	}

	public Tile getTile(Hex pos) {
		return map[pos];
	}
	public void setTile(Hex pos, Tile tile) {
		map[pos] = tile;
	}
    
	/*public float CostToEnterTile(Tile source, Tile target) {//(int sourceX, int sourceY, int targetX, int targetY) {
		LandType tt = source.type;

		if (UnitCanEnterTile(target) == false)
			return Mathf.Infinity;

		float cost = tt.isMovementAllowed();

		//if (sourceX != targetX && sourceY != targetY)
		//	cost += 0.001f;

		return cost;
		//return 1.0f;
	}*/

	public Vector3 TileCoordToWorldCoord(Vector2 pos) {
		//return new Vector3(x * tileWidth + (y % 2 * tileWidth / 2f), 0, y * 0.75f * tileHeight);
		return new Vector3(pos.x, 0, pos.y);
	}

	/*public bool UnitCanEnterTile(Tile target) {
		// we should test a unit's walktype on a clickable tile
		return true; // tileTypes [tiles [x, y]].isWalkable;
	}*/

	public void GeneratePathTo(Tile target) {
		/*
		*/
	}
}
