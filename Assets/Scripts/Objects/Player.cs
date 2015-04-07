using System.Collections.Generic;
using UnityEngine;

public class Player
{
	public Game currGame;
	public readonly PhotonPlayer photonPlayer;
	private HashSet<Village> villages = new HashSet<Village>();
	private bool isActive = false;
	private int wins = 0;
	private int losses = 0;
	private Color color = Color.clear;

	public Player(PhotonPlayer pp) {
		photonPlayer = pp;
	}

	public void setActive() {

	}

	public HashSet<Village> getVillages() {
		return villages;
		//TODO set as readOnly
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

