using UnityEngine;
using System.Collections;

public class CoinPickup : Pickup {
	
	public int pointToAdd;
	private Vector3 initialPosition;
	private Quaternion initialRotation;

	private float mouvement = 0;
	
	protected override void Awake() {
		initialPosition = transform.localPosition;
		initialRotation = transform.localRotation;
	}
	
	public void Reset() {
		transform.localPosition = initialPosition;
		transform.localRotation = initialRotation;
	}

	void OnEnable() {
		// Réinitialiser les positions
		Reset();
	}

	void OnBecameInvisible() {
		// On ne veut pas pouvoir interagir avec cette pièce si elle n'est plus visible (cf. AutoCoinPickup)
		gameObject.SetActive( false );
	}

	void Update() {
		// Un mouvement de haut en bas
		mouvement += Time.deltaTime * 4;
		transform.localPosition = new Vector2(transform.localPosition.x, initialPosition.y + Mathf.Sin (mouvement) / 7f);
	}

	protected override void OnPick() {
		// Ajouter les points au joueur
		ScoreManager.AddPoint(pointToAdd);
	}
}
