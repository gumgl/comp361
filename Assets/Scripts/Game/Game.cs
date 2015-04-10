using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using SimpleJSON;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Game : MonoBehaviour {

	public Board board;
	public NetworkManager nm;
	public GameObject playerColor;
	public Button endTurnButton;
	public UnityEngine.UI.Text panel; 

	private List<Player> players = new List<Player>();
	private int localPlayer; // Index of the local player (on this machine)
	private int currPlayer; // Index of the current player
	//private Phase currPhase;

	private Color[] colors = new Color[]{Color.yellow, Color.red, Color.gray, Color.green, Color.magenta};
	private int colorIterator = 0;

	public JSONNode Serialize()
	{
		var node = new JSONClass();

		node["currPlayer"] = GetCurrPlayer().SerializeID();

		node["players"] = new JSONArray();
		foreach (var player in players) {
			node["players"][-1] = player.Serialize();
		}

		node["tiles"] = new JSONArray();
		foreach (var entry in board.getMap()) {
			node["tiles"][-1] = entry.Value.Serialize();
		}

		return node;
	}

	public void UnSerialize(JSONNode node) {
		// Create all the tiles, position them and add them to map
		board.getMap().Clear();
		foreach (JSONNode tileNode in node["tiles"].AsArray) {
			Tile tile = Instantiate(board.landPrefab, Vector3.zero, Quaternion.identity) as Tile;
			tile.transform.parent = board.transform;
			tile.board = this.board;
			tile.UnSerialize(tileNode);
			board.getMap().Add(tile.pos, tile);
		}
		board.connectNeighbours();

		foreach (JSONNode playerNode in node["players"].AsArray) {
			string name = playerNode["name"]; // name in saved game
			PhotonPlayer photonPlayer = null;
			foreach (var p in PhotonNetwork.playerList) {
				if (p.customProperties.ContainsKey("n") && p.customProperties["n"].Equals(name))
					photonPlayer = p;
			}
			if (photonPlayer == null) {
				Debug.LogError("We have a player in saved game \"" + name + "\" but no one with that name is currently connected.");
			} else {
				Debug.Log("We found network player \"" + name + "\" ("+(photonPlayer.isLocal?"local":"remote")+")");
				Player player = photonPlayer.isLocal ? RegisterLocalPlayer(photonPlayer) : RegisterOtherPlayer(photonPlayer);
				player.currGame = this;
				player.UnSerialize(playerNode);
			}
		}
		// Set the correct current player according to saved ID
		for (int i=0; i<players.Count; i++) {
			if (players[i].SerializeID().Equals(node["currPlayer"]))
				currPlayer = i;
		}
	}

	void Start () {
		currPlayer = 0;
	}

	void Update () {
		if (Input.GetKeyDown(KeyCode.S))
			GetComponent<PhotonView>().RPC("SaveGame", PhotonTargets.All);
		else if (Input.GetKeyDown(KeyCode.L))
			GetComponent<PhotonView>().RPC("LoadGame", PhotonTargets.All);
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
	}

	[RPC]
	public void LoadGame()
	{
		nm.lobby.SetActive(false);
		string filePath = Application.persistentDataPath + "/load.json";
		Debug.Log("LoadGame() from " + filePath);

		var stream = new StreamReader(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read));
		var game = JSON.Parse(stream.ReadToEnd());
		stream.Close();
		UnSerialize(game);

		//board.init((int)PhotonNetwork.room.customProperties["s"]);
		endTurnButton.image.color = GetCurrPlayer().getColor();
		endTurnButton.interactable = (localPlayer == currPlayer);
		playerColor.GetComponent<Image>().color = GetLocalPlayer().getColor();
		playerColor.SetActive(true);
	}

	[RPC]
	public void SaveGame()
	{
		string filePath = Application.persistentDataPath + "/"+GetSeed().ToString()+".json";
		Debug.Log("SaveGame() to " + filePath);
		var sw = new StreamWriter(new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Write));
		JSONNode toSave = Serialize();
		sw.Write(toSave.ToJSON(1));
		sw.Close();
		/*StreamWriter sw = null;
		try
		{
			sw = new StreamWriter(filePath);

			JSONNode toSave = Serialize();

			sw.Write(toSave.ToJSON(1));
			Debug.Log("Saving successful!");
		}
		catch (Exception e)
		{
			Debug.LogError("Error saving gamestate to disk");
		}
		finally
		{
			if (sw != null)
				sw.Close();
		}*/
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
			foreach(Unit u in v.getUnits()){
				v.changeGold(-u.getUnitType().getUpkeep());
			}
			if(v.getGold() < 0){
				v.setGold(0);
				foreach(Unit toKill in v.getUnits())
					toKill.kill();
			}
		}
		if(localPlayer == currPlayer)
			endTurnButton.interactable = true;
		else
			endTurnButton.interactable = false;
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
			else  {
				//Debug.Log("You need to select a Peasant ( one that is ready for orders)"); 
				panel.text = "You need to select a Peasant ( one that is ready for orders)"; 
				board.selectedUnit.halo.SetActive (false);
				board.selectedUnit = null; 
			}
		}
		else panel.text = "Select a fucking Unit!"; 
			
	}
	
	public void selectedUnitCultivateMeadow (){ 
		if (board.selectedUnit != null){
			if (board.selectedUnit.getUnitType () == UnitType.Peasant && board.selectedUnit.getActionType () == ActionType.ReadyForOrders){
				board.selectedUnit.setActionType (ActionType.Cultivating);
				board.selectedUnit.halo.SetActive (false);
				board.selectedUnit = null; 
				Debug.Log ("Cultivating Meadow"); 
			}
			else  {
				//Debug.Log("You need to select a Peasant ( one that is ready for orders)"); 
				panel.text = "You need to select a Peasant ( one that is ready for orders)"; 
				board.selectedUnit.halo.SetActive (false);
				board.selectedUnit = null; 
			}
		}
		else panel.text = "Select a fucking Unit!"; 
		
	}

	/// <summary>Move to next player phase (also modifies currPlayer)</summary>
	[RPC]
	void NextTurn(){
		currPlayer = NextPlayer();
		endTurnButton.image.color = players[currPlayer].getColor();
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

	private Player AddPlayer(PhotonPlayer pp)
	{
		Player player = new Player(pp);
		//player.transform.parent = this.transform;
		player.setColor(GetNextColor());
		player.currGame = this;
		players.Add(player);
		return player;
	}

	public Player RegisterLocalPlayer(PhotonPlayer pp) {
		//Debug.Log("RegisterLocalPlayer");
		localPlayer = players.Count;
		return AddPlayer(pp);
	}

	public Player RegisterOtherPlayer(PhotonPlayer pp) {
		//Debug.Log("RegisterOtherPlayer");
		return AddPlayer(pp);
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

	public int GetSeed() {
		if (PhotonNetwork.room != null && PhotonNetwork.room.customProperties.ContainsKey("s"))
			return (int) PhotonNetwork.room.customProperties["s"];
		else
			return 0;
	}
}
