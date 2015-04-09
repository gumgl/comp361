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
	//public UnityEngine.UI.Text panel;

	private List<Player> players = new List<Player>();
	private int localPlayer; // Index of the local player (on this machine)
	private int currPlayer; // Index of the current player
	//private Phase currPhase;

	private Color[] colors = new Color[]{Color.yellow, Color.red, Color.gray, Color.green, Color.magenta};
	private int colorIterator = 0;

	void Start () {
		currPlayer = 0;
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
		endTurnButton.interactable = (localPlayer == currPlayer);

		TombstonePhase();
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
		HashSet<Tile> trees = new HashSet<Tile>();
		foreach(KeyValuePair<Hex, Tile> entry in board.getMap()) {
			if(entry.Value.getLandType() == LandType.Tree){
				trees.Add(entry.Value);
			}
		}
		foreach(Tile tile in trees)
			foreach (KeyValuePair<Hex.Direction, Tile> t in tile.getNeighbours()) {
				if((t.Value.getLandType() == LandType.Grass || t.Value.getLandType() == LandType.Meadow) && !t.Value.hasStructure() && t.Value.getUnit() == null){
					if(Random.value > 0.95)
						t.Value.setLandType(LandType.Tree);
				}
			}
		TombstonePhase();
	}

	void TombstonePhase(){
		foreach(Village v in players[currPlayer].getVillages())
			foreach(Tile t in v.getTiles())
				if(t.getLandType() == LandType.Tombstone)
					t.setLandType(LandType.Tree);
		BuildPhase();
	}

	void BuildPhase(){
		foreach(Village v in players[currPlayer].getVillages()){
			foreach(Unit u in v.getUnits()){
				if(u.getActionType() == ActionType.Cultivating){
					u.setActionType(ActionType.StillCultivating);
				}
				else if(u.getActionType() == ActionType.StillCultivating){
					u.setActionType(ActionType.ReadyForOrders);
					u.getTile().setLandType(LandType.Meadow);
				}
				else if(u.getActionType() == ActionType.BuildingRoad){
					u.setActionType(ActionType.ReadyForOrders);
					u.getTile().setLandType(LandType.Road);
				}
			}
		}
		IncomePhase();
	}

	void IncomePhase(){
		foreach(Village v in players[currPlayer].getVillages()){
			foreach(Tile t in v.getTiles()){
				if(t.getLandType() == LandType.Grass)
					v.changeGold(1);
				else if(t.getLandType() == LandType.Meadow)
					v.changeGold(2);
			}
		}
		PaymentPhase();
	}

	void PaymentPhase(){
		foreach(Village v in players[currPlayer].getVillages()){
			if(v.getVillageType() == VillageType.Castle)
				v.changeGold(-80);
			foreach(Unit u in v.getUnits()){
				v.changeGold(-u.getUnitType().getUpkeep());
				if(u.getActionType() == ActionType.Moved || u.getActionType() == ActionType.ClearedTile)
					u.setActionType(ActionType.ReadyForOrders);
			}
			if(v.getGold() < 0){
				v.setGold(0);
				if(v.getVillageType() == VillageType.Castle)
					v.setVillageType(VillageType.Fort);
				foreach(Unit toKill in v.getUnits())
					toKill.kill();
			}
		}
		endTurnButton.interactable = (localPlayer == currPlayer);

	}


	public void endTurnButtonAction(){
		//if(localPlayer == currPlayer){
			GetComponent<PhotonView>().RPC("NextTurn", PhotonTargets.All);
		//}
	}
	
	public void selectedUnitBuildRoad () { 
		if (board.selectedUnit != null){
			if (board.selectedUnit.getUnitType () == UnitType.Peasant && (board.selectedUnit.getActionType () == ActionType.ReadyForOrders || board.selectedUnit.getActionType() == ActionType.Moved)){
				board.selectedUnit.setActionType (ActionType.BuildingRoad);
				board.selectedUnit.halo.SetActive (false);
				board.selectedUnit = null; 
				Debug.Log ("Building Road"); 
			}
			else  {
				//Debug.Log("You need to select a Peasant ( one that is ready for orders)"); 
				board.setErrorText("Unit Is Busy This Turn!"); 
				board.selectedUnit.halo.SetActive (false);
				board.selectedUnit = null; 
			}
		}

		else board.setErrorText ("Select a fucking Unit!"); 
			
	}
	
	public void selectedUnitCultivateMeadow (){ 
		if (board.selectedUnit != null){
			if (board.selectedUnit.getUnitType () == UnitType.Peasant && (board.selectedUnit.getActionType () == ActionType.ReadyForOrders || board.selectedUnit.getActionType() == ActionType.Moved)){
				board.selectedUnit.setActionType (ActionType.Cultivating);
				board.selectedUnit.halo.SetActive (false);
				board.selectedUnit = null; 
				Debug.Log ("Cultivating Meadow"); 
			}
			else  {
				//Debug.Log("You need to select a Peasant ( one that is ready for orders)"); 
				board.setErrorText("Unit Is Busy This Turn!");  
				board.selectedUnit.halo.SetActive (false);
				board.selectedUnit = null; 
			}
		}
		else board.setErrorText ("Select a fucking Unit!"); 
		
	}
	
	public void onOkClick () { 
		board.setErrorText (" "); 
	}
	
	/// <summary>Move to next player phase (also modifies currPlayer)</summary>
	[RPC]
	void NextTurn(){
		this.board.unitCostsPanel.text = "";
		currPlayer = NextPlayer();
		endTurnButton.image.color = players[currPlayer].getColor();
		endTurnButton.interactable = false;
		if(currPlayer == 0){
			//currPhase = Phase.TreeGrowth;
			TreeGrowth();
		}
		else{
			//currPhase = Phase.Tombstone;
			TombstonePhase();
		}
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
