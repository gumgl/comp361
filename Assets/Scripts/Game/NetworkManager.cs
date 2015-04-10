using System;
using System.IO;
using UnityEngine;
using ExitGames.Client.Photon;
using UnityEngine.UI;
using SimpleJSON;
using Random = UnityEngine.Random;

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

	private string profilePath;

	//private Game game;

	// Use this for initialization
	void Start () {
		profilePath = Application.persistentDataPath + "/profile.json";
		//ConnectToLobby ();
		LoadProfile();
		SaveProfile(); // In case it was empty and we just created it
	}

	public void ConnectToLobby() {
		// connects to the server that is defined in our usersettings file, checking version names match
		PhotonNetwork.ConnectUsingSettings ("hexgrid");
	}

	public void JoinARoom() {
		PhotonNetwork.JoinRandomRoom();
	}

	void LoadProfile() {
		var sr = new StreamReader(new FileStream(profilePath, FileMode.OpenOrCreate, FileAccess.Read, FileShare.Read));
		var profile = JSON.Parse(sr.ReadToEnd()) ?? new JSONClass();

		if (profile["name"] == null)
			profile["name"] = "Anon"+Random.Range(100,999); // add 3 random digits to it
		if (profile["gamesPlayed"] == null)
			profile["gamesPlayed"].AsInt = 0;
		if (profile["gamesWon"] == null)
			profile["gamesWon"].AsInt = 0;

		PhotonNetwork.player.SetCustomProperties(new Hashtable()
		{
			{ "n", profile["name"].Value },
			{ "g", profile["gamesPlayed"].AsInt },
			{ "w", profile["gamesWon"].AsInt }
		});
		LoadPlayerName();

		sr.Close();
	}

	void SaveProfile() {
		var sw = new StreamWriter(new FileStream(profilePath, FileMode.Create, FileAccess.Write, FileShare.Write));
		var props = PhotonNetwork.player.customProperties;

		JSONNode profile = new JSONClass();
		if (props.ContainsKey("n"))
			profile["name"] = props["n"] as string;
		if (props.ContainsKey("g"))
			profile["gamesPlayed"].AsInt = (int)props["g"];
		if (props.ContainsKey("w"))
			profile["gamesWon"].AsInt = (int)props["w"];

		sw.Write(profile.ToJSON(1));
		sw.Close();
	}

	// shows connection status
	void OnGUI() {
		GUILayout.Label (PhotonNetwork.connectionStateDetailed.ToString ());
	}

	// joins a random room, while no rooms exist, so this fails
	void OnJoinedLobby() {
		Debug.Log("OnJoinedLobby");
		connectButton.interactable = false;

		JoinARoom ();
	}

	// if no rooms, creates a room
	void OnPhotonRandomJoinFailed() {
		Debug.Log("OnPhotonRandomJoinFailed");
		// replace 'null' with room name
		var options = new RoomOptions();
		options.maxPlayers = 5;
		options.customRoomProperties = new Hashtable(){{"s", seedInputField.text.Equals("") ? Random.Range(0,100000) : int.Parse(seedInputField.text)}};
		options.customRoomPropertiesForLobby = new string[]{"s"};
		PhotonNetwork.CreateRoom(null, options, null); // unique room name
	}

	void OnPhotonPlayerConnected() {
		Debug.Log("OnPhotonPlayerConnected");
		UpdatePlayerList();
	}

	void OnPhotonPlayerDisconnected() {
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
		LoadSeed();
	}

	public void SaveSeed() {
		Debug.Log("SaveSeed");
		
		if (PhotonNetwork.room != null) { // If we're in a room
			if (seedInputField.text.Equals(""))
				LoadSeed();
			else // Only update if seed not blank
				PhotonNetwork.room.SetCustomProperties(new Hashtable() {{"s", int.Parse(seedInputField.text)}});
		}
	}

	public void LoadSeed() {
		if (PhotonNetwork.room.customProperties.ContainsKey("s"))
			seedInputField.text = PhotonNetwork.room.customProperties["s"].ToString();
	}

	void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps) {
		// From PUN's docs: We are using a object[] due to limitations of Unity's GameObject.SendMessage (which has only one optional parameter).
		PhotonPlayer player = playerAndUpdatedProps[0] as PhotonPlayer;
		Hashtable props = playerAndUpdatedProps[1] as Hashtable;
		//playerNameInputField.text = PhotonNetwork.player.customProperties["n"].ToString();
		UpdatePlayerList();
	}

	public void SavePlayerName() {
		Debug.Log("SavePlayerName");
		if (playerNameInputField.text.Equals(""))
			LoadPlayerName();
		else {
			// Only update if seed not blank
			PhotonNetwork.player.SetCustomProperties(new Hashtable() {{"n", playerNameInputField.text}});
			SaveProfile();
		}
	}

	public void LoadPlayerName() {
		if (PhotonNetwork.player.customProperties.ContainsKey("n"))
			playerNameInputField.text = PhotonNetwork.player.customProperties["n"].ToString();
	}

	public void RandomSeed() {
		seedInputField.text = Random.Range(0, 100000).ToString();
		SaveSeed();
	}

	public void StartGame() {
		Debug.Log("StartGame");
		PhotonNetwork.room.open = false;
		game.GetComponent<PhotonView>().RPC("InitBoard", PhotonTargets.All);
	}

	void UpdatePlayerList() {
		playerListText.text = "";
		var list = PhotonNetwork.playerList;
		System.Array.Sort(list, (p1, p2) => p1.ID.CompareTo(p2.ID));

		foreach (var player in list) {
			string name = player.customProperties.ContainsKey("n") && ! player.customProperties["n"].ToString().Equals("") ? player.customProperties["n"].ToString() : "Anonymous";

			string description = String.Format("{0}. {1}", player.ID.ToString(), name);
			if (player.isMasterClient)
				description = "<color=\"yellow\">" + description + "</color>";
			if (player.isLocal)
				description = "<b>" + description + "</b> (you)";

			string stats = "";
			if (player.customProperties.ContainsKey("w") && player.customProperties.ContainsKey("g"))
				stats = String.Format(" <color=\"pink\">[{0}/{1}]</color>", player.customProperties["w"], player.customProperties["g"]);

			playerListText.text += stats + description + "\n";
		}
	}
}
