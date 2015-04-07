using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class NetworkManager : MonoBehaviour
{
	public Game game;
	public GameObject lobby;
	public Button StartGameButton;
	public UnityEngine.UI.Text playerListText;

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
		PhotonNetwork.CreateRoom(Random.Range(1,1000).ToString());
	}

	void OnPhotonPlayerConnected()
	{
		Debug.Log("OnPhotonPlayerConnected");
		//game.RegisterOtherPlayer();
		UpdatePlayerList();
	}

	void OnPhotonPlayerDisconnected()
	{
		Debug.Log("OnPhotonPlayerConnected");
		//game.RegisterOtherPlayer();
		UpdatePlayerList();
	}

	void OnJoinedRoom(){
		Debug.Log("OnJoinedRoom");
		//StartGameButton.interactable = PhotonNetwork.isMasterClient;
		// TODO: Add all users already in the game

		// TODO: Add ourselves to the game
		UpdatePlayerList();
		//game.RegisterLocalPlayer();

		//StartGame();
	}

	public void StartGame() {
		Debug.Log("StartGame");
		game.GetComponent<PhotonView>().RPC("InitBoard", PhotonTargets.All);
	}

	void UpdatePlayerList()
	{
		playerListText.text = "";
		foreach (var player in PhotonNetwork.playerList) {
			playerListText.text += player.ID + "\n";
		}
	}
}
