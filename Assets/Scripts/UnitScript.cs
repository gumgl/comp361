using UnityEngine;
using System.Collections.Generic;

public class UnitScript : MonoBehaviour {

	public int tileX;
	public int tileY;


	public TileMap map;

	public List<Node> currentPath = null; 

	// in theory if different units have different movespeed 
	int moveSpeed = 1;
	float remainingMovement=1;

	void Update(){
		if (currentPath != null) {

			int currNode = 0;

			while(currNode < currentPath.Count-1){

				Vector3 start = map.TileCoordToWorldCoord(currentPath[currNode].x, currentPath[currNode].y) + new Vector3(0f,4,0f);
				Vector3 end= map.TileCoordToWorldCoord(currentPath[currNode+1].x, currentPath[currNode+1].y)+ new Vector3(0f,4,0f);

				Debug.DrawLine (start,end, Color.red);

				currNode++;
			}
		}
		if (Vector3.Distance (transform.position, map.TileCoordToWorldCoord (tileX, tileY)) < 0.1f) {
			MoveNextTile ();
		}
		

		transform.position = Vector3.Lerp(transform.position, map.TileCoordToWorldCoord( tileX, tileY ), 5f * Time.deltaTime);
	}

	public void MoveNextTile(){

		if (currentPath == null) {
			return;
		}

		if (remainingMovement <= 0) {		
			return;
		}

		transform.position = map.TileCoordToWorldCoord( tileX, tileY );

		remainingMovement -= map.CostToEnterTile(currentPath [0].x, currentPath [0].y, currentPath [1].x, currentPath [1].y);

		tileX = currentPath[1].x;
		tileY = currentPath[1].y;

		currentPath.RemoveAt (0);
			
		if (currentPath.Count == 1) {
			currentPath = null;
		}

	}

	public void Move(){
		while(currentPath!=null && remainingMovement > 0) {
			MoveNextTile();
		}		

		remainingMovement = moveSpeed;
	}

}
