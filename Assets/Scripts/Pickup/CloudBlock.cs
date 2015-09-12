using System;
using UnityEngine;

public class CloudBlock : MonoBehaviour {

	public static bool nuageActif = false;
    public bool thisNuageActif = false;

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
        if (nuageActif)
        {
            ActiverNuage(nuageActif);
        }
        // ou si on appelle CE nuage depuis une autre fonction
        else if (thisNuageActif)
        {
            ActiverNuage(thisNuageActif);
        }
	}

	private void ActiverNuage(bool actif) {
		mySprite.enabled = actif;
        //myCollider.enabled = actif;
        myCollider.isTrigger = !actif;
	}

    public static implicit operator CloudBlock(bool v)
    {
        throw new NotImplementedException();
    }
}
