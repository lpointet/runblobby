using UnityEngine;
using System.Collections;

public class AutoCoinPickup : Pickup {

	public float radius = 0f;
	public LayerMask layerCoins;

	private bool picking = false; 							// Est-ce qu'on a activé le ramassage automatique ?
	private Collider2D[] coins = new Collider2D[20];   		// Liste des pièces existantes

	private Transform myTransform; 							// Référence vers le transform du bonus
	private Vector3 direction; 	 							// Vecteur entre le joueur et une pièce
	private Transform initialParent;						// Référence vers le parent initial
	private Camera cam; 									// Référence vers la caméra
	private float camHorizontalExtend;						// Offset vers le bord droit de l'écran
	private int nbCoins = 0; 								// Nombre de pièces à ramasser

	protected override void Awake() {
		base.Awake();
		myTransform = transform;
		initialParent = myTransform.parent;
		cam = Camera.main;
	}
	
	protected override void OnPick() {
		// Attacher le bonus au joueur
		myTransform.parent = LevelManager.getPlayer().transform;

		// Activer le ramassage
		picking = true;
	}
	
	protected override void OnDespawn() {
		// Attacher le bonus à son parent initial
		myTransform.parent = initialParent;
	}

	protected override void Update() {
		if( !picking ) {
			return;
		}

		base.Update();

		// Attirer toutes les pièces vers le joueur
		AttractCoins();
	}

	private void AttractCoins() {
		nbCoins = Physics2D.OverlapCircleNonAlloc( myTransform.position, radius, coins, layerCoins );

		for( int i = 0; i < nbCoins; i++ ) {
			camHorizontalExtend = cam.transform.position.x + cam.orthographicSize * cam.aspect;
			if( coins[i].transform.position.x > myTransform.position.x + camHorizontalExtend ) {
				continue;
			}

			// Le vecteur direction nous donne la droite entre la pièce et le bonus, donc le joueur
			direction = coins[i].transform.position - myTransform.position;
			direction.Normalize();

			// Faire venir la pièce vers le joueur
			coins[i].transform.Translate( -0.5f * direction );
		}
	}
}
