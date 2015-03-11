using UnityEngine;
using System.Collections;

public class DemoGame : MonoBehaviour {

	private Player[] players = new Player[2];
	public Player p1;
	public Player p2;

	public void initPlayers(){
		p1.setActive();
		p1.setColor(Color.red);

		p2.setColor(Color.blue);
		
		players[0] = p1;
		players[1] = p2;
	}

	public Player getPlayer(int index){
		return players[index];
	}

	public int getNumPlayers() {
		return players.Length;
	}

	public Player getRandomPlayer(){
		float rand = Random.value;
		if(rand <= 0.33f)
			return players[0];
		else if (rand <= 0.66f)
			return players[1];
		else
			return null;
	}
}
