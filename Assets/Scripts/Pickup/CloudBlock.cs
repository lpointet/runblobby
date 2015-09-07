using UnityEngine;
using System.Collections;

public class CloudBlock : MonoBehaviour {

	public static bool nuageActif = false;

	private SpriteRenderer mySprite;
	private BoxCollider2D myCollider;

	void Awake() {
		mySprite = GetComponent<SpriteRenderer> ();
		myCollider = GetComponent<BoxCollider2D> ();
	}

	void OnEnable () {
		ActiverNuage (nuageActif);
	}

	void Update () {
		// on ne met à jour que s'il y a un changement de la variable static
		if (nuageActif) {
			ActiverNuage (nuageActif);
		}
	}

	private void ActiverNuage(bool actif) {
		mySprite.enabled = nuageActif;
		myCollider.enabled = nuageActif;
	}
}
