using UnityEngine;
using System.Collections;

public class CoinPickup : Pickup {
	
	public int pointToAdd;
	private Vector3 initialPosition;
	private Quaternion initialRotation;
	
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

	protected override void OnPick() {
		// Ajouter les points au joueur
		ScoreManager.AddPoint(pointToAdd);
	}
}
