﻿using UnityEngine;

public class DieOnContact : MonoBehaviour {
	
	void OnTriggerEnter2D(Collider2D other){
		if (other.name == "Heros") {
			LevelManager.MaybeKill( transform );
		}
	}
}
