﻿using UnityEngine;
using System.Collections.Generic;

public class Unit : MonoBehaviour {

	public int tileX;
	public int tileY;

	public TileMap map;

	public List<Tile> currentPath = null;
	UnitType myType;
	ActionType currentAction;

	// in theory if different units have different movespeed 
	int moveSpeed = 1;
	float remainingMovement=1;

	void Update(){
		if (currentPath != null) {

			int currTile = 0;

			while(currTile < currentPath.Count-1){

				Vector3 start = map.TileCoordToWorldCoord(currentPath[currTile].x, currentPath[currTile].y) + new Vector3(0f,4,0f);
				Vector3 end= map.TileCoordToWorldCoord(currentPath[currTile+1].x, currentPath[currTile+1].y)+ new Vector3(0f,4,0f);

				Debug.DrawLine (start,end, Color.red);

				currTile++;
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

	public Tile getTile() {
		
	}
	
	public ActionType getActionType() {
		
	}
	
	public void setActionType(ActionType at) {
		
	}
	
	public UnitType getUnitType() {
		
	}
	
	public int getSalary() {
		// Depending on myType
	}
	
	public void setUnitType(UnitType ut) {
		
	}
	
	public Village getVillage() {
		
	}
	
	public void setVillage(Village v) {
		
	}
	
	public Player getOwner() {
		
	}

}
