using UnityEngine;
using System.Collections;

public class CoinPickup : Pickup {
	
	public int pointToAdd;
	private Vector3 initialPosition;
	private Vector3 initialSpritePosition;

	private bool counted = false;
	private Transform playerTransform;

	protected override void Awake() {
		base.Awake();
		initialPosition = myTransform.position;

		playerTransform = LevelManager.GetPlayer ().transform;

		despawnTime = 1f;
	}

	protected override void Update () {
		if (Time.timeScale == 0)
			return;

		if (!counted && myTransform.position.x < playerTransform.position.x + 1f) { // On prend un peu d'avance sur le joueur
			CountNewLeaf();
		} else if (myTransform.position.x < LevelManager.levelManager.cameraStartPosition) {
			// On ne veut pas pouvoir interagir avec cette pièce si elle n'est plus visible (cf. AutoCoinPickup)
			// On vérifie que le despawn n'est pas déjà en cours
			if (!despawnCalled)
				base.OnDespawn ();
		}

		base.Update ();
	}
	
	public void Reset() {
		myTransform.position = initialPosition;
		myRender.transform.localPosition = Vector2.zero;
		counted = false;
	}

	protected override void OnEnable() {
		base.OnEnable();
		// On s'assure que le collider n'est plus dans la table
		LevelManager.GetPlayer().RemovePickup( myCollider );
		// Réinitialiser les positions
		Reset();
	}

    protected override void OnPick() {
		base.OnPick();

		// Ajouter les points au joueur
		ScoreManager.AddPoint(pointToAdd, ScoreManager.Types.Coin);
	}

	private void CountNewLeaf() {
		ScoreManager.AddLeaf (1);
		counted = true;
	}
}
