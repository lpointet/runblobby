﻿using UnityEngine;
using System.Collections;

public class CoinPickup : Pickup {
	
	public int pointToAdd;
	private Vector3 initialPosition;
	private Quaternion initialRotation;
	private Transform myTransform;

	private Animator anim;
	
	protected override void Awake() {
		initialPosition = transform.localPosition;
		initialRotation = transform.localRotation;
		myTransform = transform;

		anim = GetComponent<Animator> ();
	}
	
	public void Reset() {
		myTransform.localPosition = initialPosition;
		myTransform.localRotation = initialRotation;
		//anim.SetBool ("picked", false);
	}

	void OnEnable() {
		// Réinitialiser les positions
		Reset();
	}

	void OnBecameInvisible() {
		// On ne veut pas pouvoir interagir avec cette pièce si elle n'est plus visible (cf. AutoCoinPickup)
		gameObject.SetActive( false );
	}

	protected override void Update() {
		base.Update();
	}

	protected override void OnPick() {
		// Ajouter les points au joueur
		ScoreManager.AddPoint(pointToAdd);
		// Déclenche les effets visuels dans l'animator
		anim.SetBool ("picked", true);
	}
}
