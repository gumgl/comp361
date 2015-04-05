using System.CodeDom;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Game : MonoBehaviour {

	private List<Player> players = new List<Player>();
	private Board board;
	private int currPlayer; // Index of the current player
	private Phase currPhase;

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
		//TODO get players (lobby? colors?)
		board.init();
		currPlayer = 0;
		currPhase = Phase.Tombstone;
	}
	void Update () {
	
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

	Player GetCurrPlayer() {
		return players[currPlayer];
	}


	public Player GetRandomOwner() {
		int choice = Random.Range(0, players.Count + 1); // Choose a random player, or null

		if (choice == players.Count) // The extra choice
			return null;
		else
			return players[choice];
	}
}
