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

		playerTransform = LevelManager.player.transform;

		despawnTime = 0.2f;
	}

	protected override void Update () {
		if (TimeManager.paused)
			return;

		if (!counted && myTransform.position.x < playerTransform.position.x + 1f) // On prend un peu d'avance sur le joueur
			CountNewLeaf();

		if (myTransform.position.x < CameraManager.cameraLeftPosition && !despawnCalled)
			OnDespawn ();

		base.Update ();
	}
	
	public void Reset() {
		myTransform.position = initialPosition;
		mySprite.transform.localPosition = Vector2.zero;
		counted = false;
	}

	protected override void OnEnable() {
		base.OnEnable();
		// On s'assure que le collider n'est plus dans la table (si jamais on va trop vite...)
		//LevelManager.player.RemovePickup (myCollider);
		// Réinitialiser les positions
		Reset();
	}

	// Fonction permettant de "récupérer" les pièces depuis une autre classe
	public void PickLeaf () {
		picked = true;
		OnPick ();
		PickEffect ();
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
