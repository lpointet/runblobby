using UnityEngine;

public class CloudBlock : MonoBehaviour {

	public static bool nuageActif = false;
    private bool thisNuageActif = false;

	private SpriteRenderer mySprite;
	private Transform mySpriteTransform;
	private BoxCollider2D myCollider;

    private float camRightLimit;

	private float delayScaleY; // Permet de décaler le mouvement selon Y de selon X
	private float randomStart; // Permet de ne pas avoir le même mouvement pour tous les nuages
	private float vectorScaleX; // Modification de scale X des nuages
	private float vectorScaleY; // Modification de scale Y des nuages
	private float speedScale = 0.5f; // Vitesse de changement
	private float timeScale; // Unité de mesure du temps pour le scaling (permet de garder une augmentation linéaire du scaling)

    void Awake() {
		mySprite = GetComponentInChildren<SpriteRenderer> ();
		mySpriteTransform = mySprite.transform;
		myCollider = GetComponent<BoxCollider2D> ();
	}

    void Start() {
        camRightLimit = CameraManager.cameraManager.camRightEnd + 1;
		randomStart = Random.Range (0f, Mathf.PI); // Démarrage aléatoire
		delayScaleY = Random.Range (Mathf.PI / 2f, Mathf.PI); // Décalage de l'axe Y
    }

    void OnEnable() {
        ActiverNuage(false);
		timeScale = randomStart;
    }

	void Update () {
        if (transform.position.x > camRightLimit)
            return;
        
        // On ne met à jour que s'il y a un changement de la variable static
		if (thisNuageActif != nuageActif)
        {
            ActiverNuage(nuageActif);
        }

		// On modifie la taille des nuages (visuel)
		if (thisNuageActif) {
			timeScale += Time.deltaTime / speedScale;
			vectorScaleX = 1f + 0.1f* Mathf.Sin (timeScale); // Entre 0.9 et 1.1
			vectorScaleY = 1f + 0.1f* Mathf.Sin (timeScale + delayScaleY); // Entre 0.9 et 1.1

			mySpriteTransform.localScale = new Vector2(vectorScaleX, vectorScaleY);
		}
	}

	public void ActiverNuage(bool actif) {
		mySprite.sprite = ListManager.current.cloudBlock[Random.Range(0, ListManager.current.cloudBlock.Length)];
		mySprite.enabled = actif;
        myCollider.isTrigger = !actif;
		thisNuageActif = nuageActif;
	}
}
