using UnityEngine;
using System.Collections;

public class InviciblePickup : Pickup {

	private Transform myTransform; 	// Référence vers le transform du bonus
	
	protected override void Awake() {
		base.Awake();
		myTransform = transform;
	}
	
	protected override void OnPick() {
		PlayerController player = LevelManager.getPlayer();

		// Attacher le bonus au joueur
		myTransform.parent = LevelManager.getPlayer().transform;

		// Activer l'invincibilité
		player.SetInvincible( lifeTime );
	}
}
