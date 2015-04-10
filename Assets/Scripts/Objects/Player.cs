using System;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;

public class Player {
	public Game currGame;
	public readonly PhotonPlayer photonPlayer;
	private HashSet<Village> villages = new HashSet<Village>();
	private int wins = 0;
	private int losses = 0;
	private Color color = Color.clear;
	private int unitToBuild = 6;
	private bool hasLost = false;

	public JSONNode Serialize() {
		var node = new JSONClass();

		node["name"] = SerializeID();
		node["lost"].AsInt = Convert.ToInt32(hasLost);

		node["villages"] = new JSONArray();
		foreach (var village in getVillages()) {
			node["villages"][-1] = village.Serialize();
		}

		return node;
	}

	public void UnSerialize(JSONNode node) {
		hasLost = Convert.ToBoolean(node["lost"]);

		var villages = node["villages"].AsArray;
		foreach (JSONNode villageNode in villages) {
			Debug.Log("Unserializing a village for player "+SerializeID());
			Village village = GameObject.Instantiate(currGame.board.villagePrefab, Vector3.zero, Quaternion.identity) as Village;
			village.transform.parent = currGame.board.transform;
			village.setBoard(currGame.board);
			village.setOwner(this);
			addVillage(village);
			village.UnSerialize(villageNode);
		}
	}

	public JSONNode SerializeID() {
		return GetName();
	}

	public Player(PhotonPlayer pp) {
		photonPlayer = pp;
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

	public void CheckLost() {
		if (villages.Count == 0)
			hasLost = true;
	}

	public bool HasLost() {
		return hasLost;
	}

	public void IncreaseGamesPlayed() {
		if (photonPlayer.customProperties.ContainsKey("g"))
			photonPlayer.customProperties["g"] = (int)photonPlayer.customProperties["g"] + 1;
		else
			photonPlayer.customProperties["g"] = 1;
	}

	public void IncreaseGamesWon()
	{
		if (photonPlayer.customProperties.ContainsKey("w"))
			photonPlayer.customProperties["w"] = (int)photonPlayer.customProperties["w"] + 1;
		else
			photonPlayer.customProperties["w"] = 1;
	}

	public string GetName()
	{
		if (photonPlayer.customProperties.ContainsKey("n"))
			return photonPlayer.customProperties["n"].ToString();
		else
			return "";
	}
}

