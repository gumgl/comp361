using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class Game : MonoBehaviour {

	public Board board;
	public NetworkManager nm;

	private List<Player> players = new List<Player>();
	private int localPlayer; // Index of the local player (on this machine)
	private int currPlayer; // Index of the current player
	private Phase currPhase;

	private Color[] colors = new Color[]{Color.yellow, Color.red, Color.gray, Color.green, Color.magenta};
	private int colorIterator = 0;

	/// <summary>Player phase (i.e. done for each player)</summary>
	private enum Phase
	{
		Tombstone,
		Build,
		Income,
		Payment,
		Move
	}

	void Start () {
		currPlayer = 0;
		currPhase = Phase.Tombstone;
	}
	void Update () {
	
	}

	[RPC]
	public void InitBoard()
	{
		nm.lobby.SetActive(false);
		var list = PhotonNetwork.playerList;
		Array.Sort(list, delegate(PhotonPlayer p1, PhotonPlayer p2) { return p1.ID.CompareTo(p2.ID); });

		foreach (var player in list)
		{
			//Debug.Log("Registering PhotonPlayer ID#" + player.ID);
			//if (player.ID == PhotonNetwork.player.ID)
			if (player.isLocal) // This is us
				RegisterLocalPlayer(player);
			else
				RegisterOtherPlayer(player);
		}
		//Debug.Log("About to init board with " + players.Count + " players...");
		board.init((int) PhotonNetwork.room.customProperties["s"]);
	}

	void TreeGrowth() {
		// TODO
	}

	/// <summary>Move to next player phase (also modifies currPlayer)</summary>
	void NextPhase() {
		if (currPhase == Phase.Move) { // Last phase of a player
			currPhase = Phase.Tombstone;
			if (currPlayer == players.Count - 1) { // Last player
				// TODO refactor
				TreeGrowth();
			}
			currPlayer = NextPlayer();
		}
	}

	int NextPlayer() {
		return (currPlayer + 1) % players.Count;
	}

	public Player GetCurrPlayer() {
		return players[currPlayer];
	}

	public Player GetLocalPlayer() {
		return players[localPlayer];
	}

	public Player GetPlayer(int index) {
		return players[index];
	}

	private void AddPlayer(PhotonPlayer pp)
	{
		Player player = new Player(pp);
		//player.transform.parent = this.transform;
		player.setColor(GetNextColor());
		player.currGame = this;
		players.Add(player);
	}

	public void RegisterLocalPlayer(PhotonPlayer pp) {
		//Debug.Log("RegisterLocalPlayer");
		localPlayer = players.Count;
		AddPlayer(pp);
	}

	public void RegisterOtherPlayer(PhotonPlayer pp) {
		//Debug.Log("RegisterOtherPlayer");
		AddPlayer(pp);
	}

	private Color GetNextColor() {
		Color toReturn = colors[colorIterator];
		colorIterator = (colorIterator + 1) % colors.Length;
		return toReturn;
	}

	public Player GetRandomOwner() {
		int choice = Random.Range(0, players.Count + 1); // Choose a random player, or null

		if (choice == players.Count) // The extra choice
			return null;
		else
			return players[choice];
	}
}
