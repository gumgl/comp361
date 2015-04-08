using System.Collections.Generic;
using UnityEngine;

public class Player
{
	public Game currGame;
	public readonly PhotonPlayer photonPlayer;
	private HashSet<Village> villages = new HashSet<Village>();
//	private bool isActive = false;
	private int wins = 0;
	private int losses = 0;
	private Color color = Color.clear;
	private int unitToBuild = 5;

	public Player(PhotonPlayer pp) {
		photonPlayer = pp;
	}

	public void setActive() {

	}
	public void setUnitToBuild(int index){
		unitToBuild = index;
	}

	public int getUnitToBuild(){
		return unitToBuild;
	}

	public HashSet<Village> getVillages() {
		return villages;
	}

	public void takeTurn() {

	}

	public void addVillage(Village v) {
		villages.Add(v);
	}

	public void removeVillage(Village v) {
		villages.Remove(v);
	}

	public void incrementWins() {
		wins ++;
	}

	public void incrementLosses() {
		losses ++;
	}

	public void setColor(Color cl){
		color = cl;
	}

	public Color getColor() {
		return color;
	}
}

