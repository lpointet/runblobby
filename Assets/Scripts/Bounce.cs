﻿using UnityEngine;
using System.Collections;

public class Bounce : MonoBehaviour {

	public float offsetCheckBounce;
	public float bouncePower;

	void OnTriggerEnter2D(Collider2D other) {
		Rigidbody2D herosRb;

		if (other.name == "Heros") {
			herosRb = other.attachedRigidbody;
		
			// Dans le cas où le contact se fait par dessus
			if (other.transform.position.y - offsetCheckBounce > transform.position.y) {

				herosRb.velocity = new Vector2 (herosRb.velocity.x, bouncePower);

				LevelManager.MaybeKill( transform );
			}
		}
	}
}
