using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Game : MonoBehaviour {

	public Board board;
	public NetworkManager nm;
	public GameObject playerColor;
	public Button endTurnButton;

	private List<Player> players = new List<Player>();
	private int localPlayer; // Index of the local player (on this machine)
	private int currPlayer; // Index of the current player
	private Phase currPhase;

	private Color[] colors = new Color[]{Color.yellow, Color.red, Color.gray, Color.green, Color.magenta};
	private int colorIterator = 0;

	/// <summary>Player phase (i.e. done for each player)</summary>
	private enum Phase
	{
		TreeGrowth,
		Tombstone,
		Build,
		Income,
		Payment,
		Move
	}

	void Start () {
		currPlayer = 0;
		currPhase = Phase.Move;
	}

	void Update () {
	}


	[RPC]
	public void InitBoard()
	{
		nm.lobby.SetActive(false);

		AddPlayers();
		playerColor.GetComponent<Image>().color = GetLocalPlayer().getColor();
		playerColor.SetActive(true);
		
		//Debug.Log("About to init board with " + players.Count + " players...");
		board.init((int) PhotonNetwork.room.customProperties["s"]);
		endTurnButton.image.color = players[currPlayer].getColor();
		if(localPlayer == currPlayer)
			endTurnButton.interactable = true;
		else
			endTurnButton.interactable = false;
	}

	void AddPlayers() {
		var list = PhotonNetwork.playerList;
		Array.Sort(list, delegate(PhotonPlayer p1, PhotonPlayer p2) { return p1.ID.CompareTo(p2.ID); });

		foreach (var player in list)
		{
			//Debug.Log("Registering PhotonPlayer ID#" + player.ID + " " + (player.isLocal ? "local" : "remote"));
			//if (player.ID == PhotonNetwork.player.ID)
			if (player.isLocal) // This is us
				RegisterLocalPlayer(player);
			else
				RegisterOtherPlayer(player);
		}
	}

	void TreeGrowth() {
		// TODO
	}

	void TombStonePhase(){

	}



	public void endTurnButtonAction(){
		//if(localPlayer == currPlayer){
			GetComponent<PhotonView>().RPC("NextTurn", PhotonTargets.All);
		//}
	}
	
	public void selectedUnitBuildRoad () { 
		if (board.selectedUnit != null){
			if (board.selectedUnit.getUnitType () == UnitType.Peasant && board.selectedUnit.getActionType () == ActionType.ReadyForOrders){
				board.selectedUnit.setActionType (ActionType.BuildingRoad);
				board.selectedUnit.halo.SetActive (false);
				board.selectedUnit = null; 
				Debug.Log ("Building Road"); 
			}
			else Debug.Log ("You need to select a Peasant ( one that is ready for orders)"); 
			//board.selectedUnit.halo.SetActive (false);
			board.selectedUnit = null; 
		}
		else Debug.Log("Select a Fucking Unit."); 	
	}
	
	public void selectedUnitCultivateMeadow (){ 
		if (board.selectedUnit != null){
			if (board.selectedUnit.getUnitType () == UnitType.Peasant && board.selectedUnit.getActionType () == ActionType.ReadyForOrders){
				board.selectedUnit.setActionType (ActionType.Cultivating);
				board.selectedUnit.halo.SetActive (false);
				board.selectedUnit = null; 
				Debug.Log ("Cultivating Meadow"); 
			}
			else Debug.Log ("You need to select a Peasant ( one that is ready for orders)"); 
			//board.selectedUnit.halo.SetActive (false);
			board.selectedUnit = null; 
		}
		else Debug.Log("Select a Fucking Unit."); 
		
	}

	/// <summary>Move to next player phase (also modifies currPlayer)</summary>
	[RPC]
	void NextTurn(){
		currPlayer = NextPlayer();
		endTurnButton.image.color = players[currPlayer].getColor();
		if(localPlayer == currPlayer)
			endTurnButton.interactable = true;
		else
			endTurnButton.interactable = false;
		if(currPlayer == 0){
			currPhase = Phase.TreeGrowth;
		}
		else
			currPhase = Phase.Tombstone;
		
	}
	/*void NextPhase() {
		if (currPhase == Phase.Move) { // Last phase of a player
			currPhase = Phase.Tombstone;
			if (currPlayer == players.Count - 1) { // Last player
				// TODO refactor
				TreeGrowth();
			}
			currPlayer = NextPlayer();
		}
	}*/

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
