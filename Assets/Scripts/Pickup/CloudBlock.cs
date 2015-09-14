using System;
using UnityEngine;

public class CloudBlock : MonoBehaviour {

	public static bool nuageActif = false;
    public bool thisNuageActif = false;

	private SpriteRenderer mySprite;
	private BoxCollider2D myCollider;

    private float camRightLimit;

    void Awake() {
		mySprite = GetComponent<SpriteRenderer> ();
		myCollider = GetComponent<BoxCollider2D> ();
	}

    void Start() {
        camRightLimit = CameraManager.cameraManager.camRightEnd + 1;
    }

    void OnEnable() {
        ActiverNuage(false);
    }

	void Update () {
        if (transform.position.x > camRightLimit)
            return;
            
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
}
