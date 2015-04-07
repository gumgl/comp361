using UnityEngine;
using ExitGames.Client.Photon;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviour
{
	public Game game;
	public GameObject lobby;
	public Button startGameButton;
	public InputField seedInputField;
	public Text playerListText;
	public Text roomNameText;

	//private Game game;

	// Use this for initialization
	void Start () {
		Connect ();
	}

	void Connect() {
		// connects to the server that is defined in our usersettings file, checking version names match

		PhotonNetwork.ConnectUsingSettings ("game-logic");
	}

	// shows connection status
	void OnGUI() {
		GUILayout.Label (PhotonNetwork.connectionStateDetailed.ToString ());
	}

	// joins a random room, while no rooms exist, so this fails
	void OnJoinedLobby() {
		Debug.Log("OnJoinedLobby");
		PhotonNetwork.JoinRandomRoom ();
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
		//StartGameButton.interactable = PhotonNetwork.isMasterClient;
		roomNameText.text = PhotonNetwork.room.name;
		UpdatePlayerList();
	}

	void OnPhotonCustomRoomPropertiesChanged(Hashtable propertiesThatChanged) {
		Debug.Log("OnPhotonCustomRoomPropertiesChanged");
		if (propertiesThatChanged.ContainsKey("s"))
		{
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
		foreach (var player in PhotonNetwork.playerList) {
			playerListText.text += player.ID;
			if (player.isLocal)
				playerListText.text += " (you)";
			playerListText.text += "\n";
		}
	}
}
