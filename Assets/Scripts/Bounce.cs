﻿using UnityEngine;

public class Bounce : MonoBehaviour {

	public float offsetCheckBounce;
	public float bouncePower;

	void OnTriggerEnter2D(Collider2D other) {
		if (other.tag == "Player") {
			PlayerController player = LevelManager.GetPlayer();
            Rigidbody2D herosRb = other.attachedRigidbody;

			// Dans le cas où le contact se fait par dessus
			if (other.transform.position.y - offsetCheckBounce > transform.position.y) {
                herosRb.velocity = new Vector2(0, bouncePower * player.GetJumpHeight());

				LevelManager.MaybeKill( transform );

                // Permettre au héros de sauter même après un bounce
                player.bounced = true;

				// Ne pas déclencher d'autres actions avec ce collider, il a fait son job
				GetComponent<Collider2D>().enabled = false;
			}
		}
	}
}
