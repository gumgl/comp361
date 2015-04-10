using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {
	public Board board;
	bool centered = false;
	int scrollDistanceVer = Screen.height / 16; 
	int scrollDistanceHor = Screen.width / 16;
	float scrollSpeed = 14;
	float zoomSpeed = 70;
	float shake = 0;
	float ShakeAmount = 1;
	float decreaseFactor = 0.1f;
	float maxShake = 15.0f;
	Vector3 preShake = Vector3.zero;
	bool startShake = false;
	float smooth = 1.5f;
	
	
	// Use this for initialization
	void Start () {
	}

	/*
	// Update is called once per frame
	void Update () {
		if (board.isStarted ()) {
			float mousePosX = Input.mousePosition.x; 
			float mousePosY = Input.mousePosition.y;
			
			if (Input.GetKeyDown (KeyCode.Escape)) {
				this.shake = 4;
				preShake = this.transform.position;
				startShake = true;
			}
			if (this.shake > 0.0f) {
				transform.position = Vector3.Lerp(transform.position, new Vector3 (this.transform.localPosition.x + Random.Range (-maxShake, maxShake), this.transform.localPosition.y, this.transform.localPosition.z + Random.Range (-maxShake, maxShake)), smooth * Time.deltaTime);
				this.shake -= 0.5f;
				maxShake -= 0.0002f;
			} else if (startShake) {
				startShake = false;
				maxShake = 15.0f;
				transform.position = preShake;
				//transform.position = Vector3.Lerp(transform.position, preShake, smooth * Time.deltaTime);
			}
			
			
			//transform.localPosition = preShake;
			
			
			if(!centered){
				if (mousePosX <= Screen.width - scrollDistanceHor*2 && mousePosX > scrollDistanceHor*2 && mousePosY > scrollDistanceVer*2 && mousePosY <= Screen.height - 2*scrollDistanceVer) { 
					centered = true;
				}
			}
			else{
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
				if (Input.GetAxis ("Mouse ScrollWheel") < 0 && transform.position.y <= 17) {
					transform.Translate (Vector3.forward * -zoomSpeed * Time.deltaTime);  
				}
			}
		}
	}
	*/
}
