using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
	private GameObject currGame;
	private HashSet<Village> villages;
	private bool isActive = false;
	private int wins = 0;
	private int losses = 0;
	//TODO initialize the villages
	void Start () {

	}

	void Update () {

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
}

