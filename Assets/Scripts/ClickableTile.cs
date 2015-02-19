using UnityEngine;
using System.Collections;

public class ClickableTile : MonoBehaviour {

	public int tileX;
	public int tileY;

	public Board map;

	void OnMouseUp(){	

		map.GeneratePathTo (tileX, tileY);

	}



}
