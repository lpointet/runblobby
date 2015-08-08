using UnityEngine;
using System.Collections;

public class AutoCoinPickup : Pickup {

	private bool picking = false; 	// Est-ce qu'on a activé le ramassage automatique ?
	private CoinPickup[] coins;   	// Liste des pièces existantes

	private Transform myTransform; 	// Référence vers le transform du bonus
	private Vector3 direction; 	 	// Vecteur entre le joueur et une pièce
	private float timeToLive;		// Temps en secondes qu'il reste avant que le bonus ne fasse plus effet

	protected override void Awake() {
		base.Awake();
		myTransform = transform;
		timeToLive = lifeTime;
	}
	
	protected override void OnPick() {
		// Attacher le bonus au joueur
		transform.parent = LevelManager.getPlayer().transform;

		// Activer le ramassage
		picking = true;
	}

	void Update() {
		if( !picking ) {
			return;
		}

		// Mettre à jour le temps qui reste à vivre
		timeToLive-= Time.deltaTime;

		// Attirer toutes les pièces vers le joueur
		AttractCoins();
	}

	private void AttractCoins() {
		coins = FindObjectsOfType<CoinPickup>();

		for( int i = 0; i < coins.Length; i++ ) {
			// Le vecteur direction nous donne la droite entre la pièce et le bonus, donc le joueur
			direction = coins[i].transform.position - myTransform.position;
			direction.Normalize();

			// Faire venir la pièce vers le joueur
			coins[i].transform.Translate( -0.5f * direction );
		}
	}
}
