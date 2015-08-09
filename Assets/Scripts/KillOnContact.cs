using UnityEngine;
using System.Collections;

public class KillOnContact : MonoBehaviour {

	void OnTriggerEnter2D(Collider2D other){
		if (other.name == "Heros") {	
			LevelManager.Kill( LevelManager.getPlayer() );
		}
	}
}
