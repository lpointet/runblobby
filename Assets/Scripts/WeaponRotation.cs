﻿using UnityEngine;
using System.Collections;

public class WeaponRotation : MonoBehaviour {

	public string followName;

	private Vector3 followPosition;
	private Vector3 vecteurWeapon;
	private float rotaZ;
	private Transform myTransform;
	private GameObject follow;

	void Awake() {
		myTransform = transform;
	}

	// Permet à l'arme de suivre quelque chose dans la scène ou la souris (si jamais elle est visible, sinon donne le transform.rotation pour les balles au moins)
	void Update () {
		// On récupère l'objet à suivre à chaque Update pour etre sur qu'il est bien actif dans la scène
		if( null != followName ) {
			follow = GameObject.Find( followName );
			if( null == follow ) {
				return;
			}
		}

		followPosition = null == follow ? Camera.main.ScreenToWorldPoint(Input.mousePosition) : follow.transform.position;

		vecteurWeapon = followPosition - myTransform.position;
		vecteurWeapon.Normalize ();

		rotaZ = Mathf.Atan2 (vecteurWeapon.y, vecteurWeapon.x) * Mathf.Rad2Deg; // Angle entre l'horizontal et le vecteur calculé précédemment

		myTransform.rotation = Quaternion.Euler (0f, 0f, rotaZ);
	}
}
