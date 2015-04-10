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
		Debug.Log(profilePath);
		//ConnectToLobby ();
		LoadProfile();
	}

	public void ConnectToLobby() {
		// connects to the server that is defined in our usersettings file, checking version names match
		PhotonNetwork.ConnectUsingSettings ("andrej");
	}

	public void JoinARoom() {
		PhotonNetwork.JoinRandomRoom();
	}

	void LoadProfile() {
		StreamReader sr = null;
		try {
			sr = new StreamReader(profilePath);
		} catch (Exception e) {
			Debug.LogError("Error loading profile from disk");
		}
		finally
		{
			string fileContent = sr.ReadToEnd();

			var profile = JSON.Parse(fileContent);
			if (profile["name"] == null)
				profile["name"] = new JSONData("");
			if (profile["gamesPlayed"] == null)
				profile["gamesPlayed"] = new JSONData(0);
			if (profile["gamesWon"] == null)
				profile["gamesWon"] = new JSONData(0);

			PhotonNetwork.player.SetCustomProperties(new Hashtable()
			{
				{ "n", profile["name"].Value },
				{ "g", profile["gamesPlayed"].Value },
				{ "w", profile["gamesWon"].Value }
			});
			LoadPlayerName();
			if (sr != null)
				sr.Close();
		}
		/*var profile = JSONClass.LoadFromFile(profilePath);
		if (profile["name"] == null)
			profile["name"] = new JSONData("");
		if (profile["gamesPlayed"] == null)
			profile["gamesPlayed"] = new JSONData(0);
		if (profile["gamesWon"] == null)
			profile["gamesWon"] = new JSONData(0);
		PhotonNetwork.player.SetCustomProperties(new Hashtable()
			{
				{ "n", profile["name"].Value },
				{ "g", profile["gamesPlayed"].Value },
				{ "w", profile["gamesWon"].Value }
			});
		LoadPlayerName();*/
	}

	void SaveProfile() {
		StreamWriter sw = null;
		try {
			sw = new StreamWriter(profilePath);
			var props = PhotonNetwork.player.customProperties;

			JSONNode profile = new JSONClass();
			if (props.ContainsKey("n"))
				profile["name"] = new JSONData(props["n"] as string);
			if (props.ContainsKey("g"))
				profile["gamesPlayed"] = new JSONData((int)props["g"]);
			if (props.ContainsKey("w"))
				profile["gamesWon"] = new JSONData((int)props["w"]);
			sw.Write(profile.ToJSON(4));
			//profile.SaveToFile(profilePath);
		} catch (Exception e) {
			Debug.LogError("Error saving profile to disk");
		} finally {
			if (sw != null)
				sw.Close();
		}
		/*var profile = new JSONClass();
		//profile["name"] = new JSONData(playerNameInputField.text);
		profile["test"] = "wow";
		profile.SaveToFile(profilePath);*/
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
		options.customRoomProperties = new Hashtable(){{"s", Random.Range(0,100000)}};
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
