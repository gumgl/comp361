using UnityEngine;
using System.Collections;

public class NetworkUnit : Photon.MonoBehaviour {


	Vector3 realPosition = Vector3.zero;
	Quaternion realRotation = Quaternion.identity;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (photonView.isMine) {
			// do nothing - the character will be tracked normally
		} else {
			transform.position = Vector3.Lerp (transform.position, realPosition, 0.1f);
			transform.rotation = Quaternion.Lerp (transform.rotation, realRotation, 0.1f);
			realPosition = transform.position;
			realRotation = transform.rotation;
		}
		
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info){
		if(stream.isWriting){
			// this is our unit. Send our position to the network
			stream.SendNext (transform.position);
			stream.SendNext (transform.position);
		}
		else{
			// this is someone else's unit. Receieve their position and update our version of the unit
			transform.position = (Vector3) stream.ReceiveNext ();
			transform.rotation = (Quaternion) stream.ReceiveNext ();

		}
	}

}
