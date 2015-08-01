using UnityEngine;
using System.Collections;

public class HurtOnContact : MonoBehaviour {

	public int damageToGive;

	void OnTriggerEnter2D(Collider2D other){
		if (other.name == "Heros") {
			LevelManager.getPlayer().HurtPlayer(damageToGive);
		}
	}
}
