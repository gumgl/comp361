using System.CodeDom;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Game : MonoBehaviour {

	private List<Player> players = new List<Player>();
	private Board board;
	private int localPlayer; // Index of the local player (on this machine)
	private int currPlayer; // Index of the current player
	private Phase currPhase;

	private Color[] colors = new Color[]{Color.yellow, Color.red, Color.gray, Color.green};
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

	public Player GetCurrPlayer() {
		return players[currPlayer];
	}

	public Player GetPlayer(int index) {
		return players[index];
	}

	private void AddPlayer(Player player) {
		player.currGame = this;
		players.Add(player);
	}

	public void RegisterLocalPlayer() {
		Player me = new Player();
		me.setColor(getNextColor());

		AddPlayer(me);
	}

	public void RegisterOtherPlayer() {
		Player them = new Player();
		them.setColor(getNextColor());

		AddPlayer(them);
	}

	private Color getNextColor() {
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
