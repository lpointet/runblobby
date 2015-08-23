using UnityEngine;
using System.Collections;

public class InviciblePickup : Pickup {

	private Transform myTransform; 				// Référence vers le transform du bonus
	private Transform initialParent;			// Référence vers le parent initial
	
	protected override void Awake() {
		base.Awake();
		myTransform = transform;
		initialParent = myTransform.parent;
	}
	
	protected override void OnPick() {
		PlayerController player = LevelManager.getPlayer();

		// Attacher le bonus au joueur
		myTransform.parent = LevelManager.getPlayer().transform;

		// Activer l'invincibilité
		player.SetInvincible( lifeTime );
	}

	protected override void OnDespawn() {
		// Attacher le bonus à son parent initial
		myTransform.parent = initialParent;
	}
}
