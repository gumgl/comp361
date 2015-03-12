using UnityEngine;
using System.Collections.Generic;

public class Unit : Photon.MonoBehaviour {
	public Board board = null;
	Village village;

	UnitType myType;
	ActionType currentAction;
	public Tile tile;
	public List<Tile> currentPath = null;
	public int currentPathIndex;
	public float height = 2f;

	public float moveSpeed = 5f;

	void Update() {
		if (isMoving()) { // Path animation
			for (int i = 0; i < currentPath.Count-1; i++) { // Debug line
				Vector3 start = board.TileCoordToWorldCoord(currentPath[i].getPixelPos()) + new Vector3(0f, 4, 0f);
				Vector3 end = board.TileCoordToWorldCoord(currentPath[i + 1].getPixelPos()) + new Vector3(0f, 4, 0f);

				Debug.DrawLine(start, end, Color.red);
			}
			var unitPos = new Vector2(transform.position.x, transform.position.z);
			var tilePos = new Vector2(tile.transform.position.x, tile.transform.position.z);
			if (Vector2.Distance(unitPos, tilePos) < 0.1f) {
				MoveToNextTile();
			}
			transform.position = Vector3.Lerp(transform.position, tile.transform.position, moveSpeed * Time.deltaTime);
			setHeightAboveBoard();
		}
	}

	public void MoveToNextTile() {
		if (isMoving()) { // Unit is going somewhere
			currentPathIndex ++; // Next tile!
			if (currentPathIndex < currentPath.Count) { // Unit is still getting there
				setTile(currentPath[currentPathIndex]);
			} else { // Unit has arrived at target
				currentPath = null;
				positionOverTile();
			}
		}
	}

	public void MoveTo(Tile target) {

		if (isMoving())
			Debug.LogError("Unit already moving");
		else if (target.canEnter(this) == false)
			Debug.LogError("Cannot move to this tile");
		else {

			Dictionary<Tile, int> dist = new Dictionary<Tile, int>(); // Cost
			Dictionary<Tile, Tile> prev = new Dictionary<Tile, Tile>(); // Came from
		
			List<Tile> frontier = new List<Tile>();

			Tile source = getTile();
			dist[source] = 0;
			prev[source] = null;
		
			/*foreach (Tile v in board) {
				if (v != source) {
					dist[v] = Mathf.Infinity;
					prev[v] = null;
				}
				frontier.Add(v);
			}*/
			frontier.Add(source);
		
			while (frontier.Count > 0) {
			
				Tile current = null;
			
				/*foreach (Tile possibleU in frontier) {
					if (current == null || dist[possibleU] < dist[current]) {
						current = possibleU;
					}
				}*/
				current = frontier[0];
				frontier.RemoveAt(0);
				if (current == target)
					break;
				
				foreach (KeyValuePair<Hex.Direction, Tile> kv in current.getNeighbours()) {
					Tile neighbour = kv.Value;
					int newDist = dist[current] + 1;
					if (neighbour.canWalkThrough(this) &&
						(dist.ContainsKey(neighbour) == false || newDist < dist[neighbour])) {
						dist[neighbour] = newDist;
						// TODO: add the HexDistance heurist and use priority queue
						//priority = new_cost + heuristic(goal, next)
						frontier.Add(neighbour);
						prev[neighbour] = current;
					}
					//pure distance approach
					//float alt = dist[u] + u.DistanceTo(v);
				
					//weighted move cost approach
					/*float alt = dist[current] + board.CostToEnterTile(current, neighbour);
					if (alt < dist[neighbour]) {
						dist[neighbour] = alt;
						prev[neighbour] = current;
					}*/
				}
			}
			if (prev[target] != null) { // If reachable
				currentPath = new List<Tile>();
				Tile curr = target;
		
				while (curr != null) { // Add path by backtracking
					currentPath.Add(curr);
					curr = prev[curr];
				}
				currentPath.Reverse();
				currentPathIndex = 0;
				MoveToNextTile();
			}
		}
	}

	public bool isMoving() {
		return (currentPath != null && currentPath.Count > 0);
	}

	public Tile getTile() {
		return tile;
	}

	public void setTile(Tile t) {
		if (tile != null)
			tile.unit = null;
		tile = t;
		tile.unit = this;
	}
	public void positionOverTile() {
		if (tile != null) {
			transform.position = tile.transform.position;
			setHeightAboveBoard();
		}
	}
	public void setHeightAboveBoard() {
		var tmp = transform.localPosition;
		tmp.y = height;
		transform.localPosition = tmp;
	}
	
	public void placeUnit() { 
		this.transform.position = board.TileCoordToWorldCoord(tile.getPixelPos()) + new Vector3(0, 1f, 0);
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
	public void removeUnit() { 
		
	}

}
