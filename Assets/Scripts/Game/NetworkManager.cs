using UnityEngine;
using ExitGames.Client.Photon;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviour
{
	public Game game;
	public GameObject lobby;
	public Button connectButton;
	public Button startGameButton;
	public InputField seedInputField;
	public InputField playerNameInputField;
	public Text playerListText;
	public Text roomNameText;

	//private Game game;

	// Use this for initialization
	void Start () {
		//ConnectToLobby ();
	}

	public void ConnectToLobby() {
		// connects to the server that is defined in our usersettings file, checking version names match
		PhotonNetwork.ConnectUsingSettings ("game-lobby");
	}

	public void JoinARoom()
	{
		PhotonNetwork.JoinRandomRoom();
	}

	// shows connection status
	void OnGUI() {
		GUILayout.Label (PhotonNetwork.connectionStateDetailed.ToString ());
	}

	// joins a random room, while no rooms exist, so this fails
	void OnJoinedLobby() {
		Debug.Log("OnJoinedLobby");
		connectButton.interactable = false;

		// TODO: fetch player's name from a file

		JoinARoom ();
	}

	// if no rooms, creates a room
	void OnPhotonRandomJoinFailed() {
		Debug.Log("OnPhotonRandomJoinFailed");
		// replace 'null' with room name
		var options = new RoomOptions();
		options.maxPlayers = 5;
		options.customRoomProperties = new Hashtable(){{"s", Random.Range(0,100000)}};
		options.customRoomPropertiesForLobby = new string[]{"s"};
		PhotonNetwork.CreateRoom(null, options, null); // unique room name
	}

	void OnPhotonPlayerConnected()
	{
		Debug.Log("OnPhotonPlayerConnected");
		UpdatePlayerList();
	}

	void OnPhotonPlayerDisconnected()
	{
		Debug.Log("OnPhotonPlayerConnected");
		UpdatePlayerList();
	}

	void OnJoinedRoom(){
		Debug.Log("OnJoinedRoom");
		startGameButton.interactable = true;
		roomNameText.text = PhotonNetwork.room.name;
		UpdatePlayerList();
	}

	void OnPhotonCustomRoomPropertiesChanged(Hashtable propertiesThatChanged) {
		Debug.Log("OnPhotonCustomRoomPropertiesChanged");
		if (propertiesThatChanged.ContainsKey("s")) {
			seedInputField.text = propertiesThatChanged["s"].ToString();
		}
	}

	public void UpdateSeed() {
		Debug.Log("UpdateSeed");
		var room = PhotonNetwork.room;
		
		if (room != null) { // If we're in a room
			if (seedInputField.text.Equals("") && room.customProperties.ContainsKey("s"))
				seedInputField.text = room.customProperties["s"].ToString();
			else // Only update if seed not blank
				room.SetCustomProperties(new Hashtable() {{"s", int.Parse(seedInputField.text)}});
		}

	}

	void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps) {
		// From PUN's docs: We are using a object[] due to limitations of Unity's GameObject.SendMessage (which has only one optional parameter).
		PhotonPlayer player = playerAndUpdatedProps[0] as PhotonPlayer;
		Hashtable props = playerAndUpdatedProps[1] as Hashtable;
		//playerNameInputField.text = PhotonNetwork.player.customProperties["n"].ToString();
		UpdatePlayerList();
	}

	public void UpdatePlayerName() {
		Debug.Log("UpdatePlayerName");
		if (playerNameInputField.text.Equals("") && PhotonNetwork.player.customProperties.ContainsKey("n"))
			playerNameInputField.text = PhotonNetwork.player.customProperties["n"].ToString();
		else {
			// Only update if seed not blank
			PhotonNetwork.player.SetCustomProperties(new Hashtable() {{"n", playerNameInputField.text}});
			// TODO: STORE NAME TO FILE
		}
	}

	public void RandomSeed()
	{
		seedInputField.text = Random.Range(0, 100000).ToString();
		UpdateSeed();
	}

	public void StartGame() {
		Debug.Log("StartGame");
		PhotonNetwork.room.open = false;
		game.GetComponent<PhotonView>().RPC("InitBoard", PhotonTargets.All);
	}

	void UpdatePlayerList()
	{
		playerListText.text = "";
		var list = PhotonNetwork.playerList;
		System.Array.Sort(list, delegate(PhotonPlayer p1, PhotonPlayer p2) { return p1.ID.CompareTo(p2.ID); });

		foreach (var player in list) {
			string name = player.customProperties.ContainsKey("n") && ! player.customProperties["n"].ToString().Equals("") ? player.customProperties["n"].ToString() : "Anonymous";

			string description = "#" + player.ID.ToString() + ": "+ name;
			if (player.isMasterClient)
				description = "<color=\"yellow\">" + description + "</color>";
			if (player.isLocal)
				description = "<b>" + description + "</b> (you)";
			playerListText.text += description + "\n";
		}
	}
}
