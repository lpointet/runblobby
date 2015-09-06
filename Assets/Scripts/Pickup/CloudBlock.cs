using UnityEngine;
using System.Collections;

public class CloudBlock : MonoBehaviour {

	public static bool nuageActif = false;
	private bool thisNuageActif;

	private SpriteRenderer mySprite;
	private BoxCollider2D myCollider;

	void Awake() {
		mySprite = GetComponent<SpriteRenderer> ();
		myCollider = GetComponent<BoxCollider2D> ();
		thisNuageActif = nuageActif;
	}

	void OnEnable () {
		ActiverNuage (nuageActif);
	}

	void Update () {
		// on ne met à jour que s'il y a un changement de la variable static
		if (thisNuageActif != nuageActif) {
			ActiverNuage (nuageActif);

			thisNuageActif = nuageActif; // on garde la dernière valeur de la variable static
		}
	}

	private void ActiverNuage(bool actif) {
		mySprite.enabled = nuageActif;
		myCollider.enabled = nuageActif;
	}
}
