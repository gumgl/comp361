using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {
	public Board board;

	int scrollDistanceVer = Screen.height / 7; 
	int scrollDistanceHor = Screen.width / 7;
	float scrollSpeed = 14;
	float zoomSpeed = 70;
	float rotSpeed = 15;
	float x = 0;

	// Use this for initialization
	void Start () {
		x = transform.eulerAngles.y;
	}
	
	// Update is called once per frame
	void Update () {
		if (board.isStarted ()) {
			float fYRot = Camera.main.transform.eulerAngles.y;
			float mousePosX = Input.mousePosition.x; 
			float mousePosY = Input.mousePosition.y; 
			if (mousePosX < scrollDistanceHor) { 
				transform.Translate (Vector3.right * -scrollSpeed * Time.deltaTime); 
			} 
			
			if (mousePosX >= Screen.width - scrollDistanceHor) { 
				transform.Translate (Vector3.right * scrollSpeed * Time.deltaTime); 
			}
			
			if (mousePosY < scrollDistanceVer) { 
				transform.Translate (Vector3.forward * -scrollSpeed * Time.deltaTime, Space.World); 
			} 
			
			if (mousePosY >= Screen.height - scrollDistanceVer) { 
				transform.Translate (Vector3.forward * scrollSpeed * Time.deltaTime, Space.World); 
			}

			if (Input.GetAxis ("Mouse ScrollWheel") > 0 && transform.position.y >= 4) {
				transform.Translate (Vector3.forward * zoomSpeed * Time.deltaTime);  
			}
			if (Input.GetAxis ("Mouse ScrollWheel") < 0 && transform.position.y <= 26) {
				transform.Translate (Vector3.forward * -zoomSpeed * Time.deltaTime);  
			}
		}
	}
}
