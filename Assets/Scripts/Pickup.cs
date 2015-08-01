using UnityEngine;
using System.Collections;

public class Pickup : MonoBehaviour {

	public int pointToAdd;

	void OnTriggerEnter2D(Collider2D other){
		if (other.name == "Heros") {
			ScoreManager.AddPoint(pointToAdd);
			gameObject.SetActive(false);
		}
	}
}
