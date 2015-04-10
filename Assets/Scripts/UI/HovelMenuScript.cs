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
		if(Input.GetKeyUp(KeyCode.Escape) && !isPaused){
			PauseGame();
		}
		
		else if(Input.GetKeyUp(KeyCode.Escape) && isPaused){
			UnpauseGame();
		}
	}
	
	
	public void PauseGame(){
		
		anim.enabled = true;
		
		anim.Play("HovelMenuSlideIn");
		//set the isPaused flag to true to indicate that the game is paused
		isPaused = true;

	}
	//function to unpause the game
	public void UnpauseGame(){
		//set the isPaused flag to false to indicate that the game is not paused
		isPaused = false;
		//play the SlideOut animation
		anim.Play("HovelMenuSlideOut");

	}
	
}