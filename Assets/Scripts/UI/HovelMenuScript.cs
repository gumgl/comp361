using UnityEngine;
using System.Collections;

public class HovelMenuScript : MonoBehaviour {
	
	
	public GameObject hovelMenuPanel;
	
	private Animator anim;
	
	private bool isPaused = false;
	
	void Start () {
		
		Time.timeScale = 1;
		anim = hovelMenuPanel.GetComponent<Animator>();
		anim.enabled = false;
	}
	
	
	public void Update () {

	}
	
	
	public void BuildMenu(VillageType type){

		this.GetComponent<Game> ().SoldierButton.interactable = false;
		this.GetComponent<Game> ().KnightButton.interactable = false;
		this.GetComponent<Game> ().CannonButton.interactable = false;
		this.GetComponent<Game> ().TowerButton.interactable = false;

		if (type >= VillageType.Town) {
			this.GetComponent<Game> ().SoldierButton.interactable = true;
			this.GetComponent<Game> ().TowerButton.interactable = true;
		}
		if (type >= VillageType.Fort) {
			this.GetComponent<Game> ().KnightButton.interactable = true;
		}
		if (type >= VillageType.Castle) {
			this.GetComponent<Game> ().CannonButton.interactable = true;
		}

		anim.enabled = true;
		anim.Play("HovelMenuSlideIn");
		//set the isPaused flag to true to indicate that the game is paused
		isPaused = true;

	}
	//function to unpause the game
	public void CloseBuildMenu(){
		//set the isPaused flag to false to indicate that the game is not paused
		isPaused = false;
		//play the SlideOut animation
		anim.Play("HovelMenuSlideOut");

	}
	
}