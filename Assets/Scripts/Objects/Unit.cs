using UnityEngine;
using System.Collections.Generic;

public class Unit : Photon.MonoBehaviour {
	public Board board = null;
	Village village;

	UnitType myType;
	ActionType currentAction;
	public Tile tile;
	public List<Tile> currentPath = null;

	// in theory if different units have different movespeed 
	int moveSpeed = 1;
	float remainingMovement = 1;

<<<<<<< HEAD
	void Update() {
		/*if (currentPath != null) {
=======
	void Update() { /*
		if (currentPath != null) {
>>>>>>> origin/master

			int currTile = 0;

			while (currTile < currentPath.Count-1) {

				Vector3 start = board.TileCoordToWorldCoord(currentPath[currTile].getPixelPos()) + new Vector3(0f, 4, 0f);
				Vector3 end = board.TileCoordToWorldCoord(currentPath[currTile + 1].getPixelPos()) + new Vector3(0f, 4, 0f);

				Debug.DrawLine(start, end, Color.red);

				currTile++;
			}
		}
		if (Vector3.Distance(transform.position, board.TileCoordToWorldCoord(tile.getPixelPos())) < 0.1f) {
			MoveNextTile();
		}
		

<<<<<<< HEAD
		transform.position = Vector3.Lerp(transform.position, board.TileCoordToWorldCoord(tile.getPixelPos()), 5f * Time.deltaTime);*/
	}
=======
		transform.position = Vector3.Lerp(transform.position, board.TileCoordToWorldCoord(tile.getPixelPos()), 5f * Time.deltaTime);
	*/}
>>>>>>> origin/master

	public void MoveNextTile() {
		/*
		if (currentPath == null) {
			return;
		}

		if (remainingMovement <= 0) {		
			return;
		}

		transform.position = board.TileCoordToWorldCoord(tile.getPixelPos());

		remainingMovement -= board.CostToEnterTile(currentPath[0], currentPath[1]);

		//tileX = currentPath[1].x;
		//tileY = currentPath[1].y;
		tile = currentPath[1];

		currentPath.RemoveAt(0);
			
		if (currentPath.Count == 1) {
			currentPath = null;
		}*/
	}

	public void Move() {/*
		while (currentPath!=null && remainingMovement > 0) {
			MoveNextTile();
		}		

		remainingMovement = moveSpeed;*/
	}

	public void MoveTo(Tile target) {
		/*
		currentPath = null;
		
		if (CanEnterTile(target) == false) {
			return;
		}
		
		Dictionary<Tile, float> dist = new Dictionary<Tile, float>();
		Dictionary<Tile, Tile> prev = new Dictionary<Tile, Tile>();
		
		List<Tile> unvisited = new List<Tile>();
		
		Tile source = selectedUnit.GetComponent<Unit>().getTile();
		
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
				float alt = dist[u] + board.CostToEnterTile(u, v);
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
		
		selectedUnit.GetComponent<Unit>().currentPath = currentPath;*/
	}

	public Tile getTile() {
		return tile;
	}

	public void setTile(Tile t){
		tile = t;
	}
	
	public void setTile(Tile t) { 
		tile = t;
	} 
	
	public void placeUnit () {
		board = GameObject.Find("DemoGame/Board") as Board;
		transform.position = board.TileCoordToWorldCoord(tile.getPixelPos());
	}
	
	public ActionType getActionType() {
		return currentAction;
	}
	
	public void setActionType(ActionType at) {
		currentAction = at;
	}
	
	public UnitType getUnitType() {
		return myType;
	}
	
	public void setUnitType(UnitType ut) {
		myType = ut;
	}
	
	public int getSalary() {
		// Depending on myType
		return 0;
	}
	
	public Village getVillage() {
		return village;
	}
	
	public void setVillage(Village v) {
		village = v;
	}
	
	public Player getOwner() {
		return village.getOwner();
	}
	public void removeUnit () { 
		
	}

}
