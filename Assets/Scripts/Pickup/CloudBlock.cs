using UnityEngine;

public class CloudBlock : MonoBehaviour {

	public static bool nuageActif = false;
    private bool thisNuageActif = false;

	private SpriteRenderer mySprite; // Nécessité de prendre un enfant, pour que le scale ne modifie pas aussi le collider
	private Transform mySpriteTransform;
	private BoxCollider2D myCollider;

	private float delayScaleY; // Permet de décaler le mouvement selon Y de selon X
	private float randomStart; // Permet de ne pas avoir le même mouvement pour tous les nuages
	private float vectorScaleX; // Modification de scale X des nuages
	private float vectorScaleY; // Modification de scale Y des nuages
	private float speedScale = 0.25f; // Vitesse de changement
	private float timeScale; // Unité de mesure du temps pour le scaling (permet de garder une augmentation linéaire du scaling)

	private float timeToPop;
	private float delayToPop = 0.1f;
	private float scalePop;

    void Awake() {
		mySprite = GetComponentInChildren<SpriteRenderer> ();
		mySpriteTransform = mySprite.transform;
		myCollider = GetComponent<BoxCollider2D> ();
	}

    void Start() {
		randomStart = Random.Range (0f, 2 * Mathf.PI); // Démarrage aléatoire
		delayScaleY = Random.Range (Mathf.PI, 3 * Mathf.PI / 2f); // Décalage de l'axe Y
		speedScale = Random.Range(0.15f, 0.35f);
    }

    void OnEnable() {
        ActiverNuage(false);
		timeScale = randomStart;
		scalePop = 0;
    }

	void Update () {
		if (transform.position.x > CameraManager.cameraEndPosition || TimeManager.paused)
            return;
        
        // On ne met à jour que s'il y a un changement de la variable static et que le nuage n'est pas déjà actif
		if (nuageActif && !thisNuageActif)
			ActiverNuage (true);

		// On modifie la taille des nuages (visuel)
		if (thisNuageActif) {
			// On le fait "pop" au démarrage
			if (TimeManager.time < timeToPop) {
				scalePop += 1.5f * TimeManager.deltaTime / delayToPop;
				mySpriteTransform.localScale = new Vector2 (scalePop, scalePop);
			} else { // Une fois qu'il a pop, il fluctue
				timeScale += TimeManager.deltaTime / speedScale;
				vectorScaleX = 1f + 0.15f * Mathf.Sin (timeScale); // Entre 0.9 et 1.1
				vectorScaleY = 1f + 0.15f * Mathf.Sin (timeScale + delayScaleY); // Entre 0.9 et 1.1

				mySpriteTransform.localScale = new Vector2 (vectorScaleX, vectorScaleY);
			}
		}
	}

	public void ActiverNuage(bool actif) {
		if (actif)
			mySprite.sprite = ListManager.current.cloudBlock[Random.Range(0, ListManager.current.cloudBlock.Length)];
		
		mySprite.enabled = actif;
        myCollider.isTrigger = !actif;
		//thisNuageActif = nuageActif;
		thisNuageActif = actif;

		timeToPop = TimeManager.time + delayToPop;
	}
}
