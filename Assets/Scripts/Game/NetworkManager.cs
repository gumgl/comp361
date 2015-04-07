using UnityEngine;
using System.Collections;

public class NetworkManager : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Connect ();
	}



	void Connect() {
		// connects to the server that is defined in our usersettings file, checking version names match
		PhotonNetwork.ConnectUsingSettings ("NetworkingDemo sdfsdf");

	}

	// shows connection status
	void OnGUI() {
		GUILayout.Label (PhotonNetwork.connectionStateDetailed.ToString ());
	}

	// joins a random room, while no rooms exist, so this fails
	void OnJoinedLobby() {
		PhotonNetwork.JoinRandomRoom ();

	}

	// if no rooms, creates a room
	void OnPhotonRandomJoinFailed(){
		// replace 'null' with room name
		PhotonNetwork.CreateRoom(Random.Range(1,1000).ToString());
	}

	void OnJoinedRoom(){
		SpawnTheMap ();
	}


	void SpawnTheMap(){

		//THIS IS HUGE!
		//All network objects need to be Instantiated with PhotonNetwork.Instantiate, instead of Instantiate
		//So, like, yeah, do that.
		//To everything
		//Things will be instantiated by name
		//MAKE SURE ALL INSTANTIATED OBJECTS COME DIRECTLY FROM THE RESOUCE FOLDER
		//remember to attach a photonview
		//the last parameter is a group number, of which group to identify about any updates

		if (PhotonNetwork.isMasterClient) {
			PhotonNetwork.Instantiate("DemoGame", Vector3.zero, Quaternion.identity, 0);
		}


		// in theory this should be done
		// my map is spawning my test unit
		//PhotonNetwork.Instantiate ("Unit", Vector3.zero, Quaternion.identity, 0);

	}
}
