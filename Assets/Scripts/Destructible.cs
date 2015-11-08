using UnityEngine;
using System.Collections;

public class Destructible : MonoBehaviour {
	
	void OnTriggerEnter2D(Collider2D other){
		if (other.name == "Heros") {
			KillObject();
		}
	}
	
	private void KillObject() {
		gameObject.SetActive (false);
	}
}
