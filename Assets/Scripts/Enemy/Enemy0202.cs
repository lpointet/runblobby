using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/* STINGER LE SCORPION
 * Plante sa queue dans le sol et la fait ressortir sous le héros : assombrit l'écran s'il se fait toucher
 * Envoie une rafale inévitable s'il se fait toucher quand il dresse la queue
 * Hard+ : Les boules d'eau font reculer de moins en moins le mur, jusqu'à le faire avancer (voir script : Waterball)
 */
public class Enemy0202 : Enemy {

	[Header("Atk 1 : Ground Sting")]
	[SerializeField] private float groundStingAttackLength = 4f;

	[SerializeField] private GameObject groundSting;
	private Collider2D groundStingCollider;
	private SpriteRenderer groundStingSprite;
	private ParticleSystem groundStingParticle;
	[SerializeField] private Image groundStingBlackScreen;

	private float groundStingLength; // Temps de déplacement du dard
	[SerializeField] private float groundStingNormalLength;
	[SerializeField] private float groundStingHardLength;
	[SerializeField] private float groundStingHellLength;
	private float groundStingDamage; // Dégât du dard
	[SerializeField] private float groundStingNormalDamage;
	[SerializeField] private float groundStingHardDamage;
	[SerializeField] private float groundStingHellDamage;

	[Header("Atk 2 : Angry Wait")]
	[SerializeField] private float angryWaitAttackLength = 3f;
	private bool isAngryWaiting = false;
	[SerializeField] private GameObject inavoidableSting;

	void Start() {
		//myAnim.SetFloat("ratioHP", 1f);

		groundSting.SetActive (false);
		groundStingCollider = groundSting.GetComponent<CircleCollider2D> ();
		groundStingSprite = groundSting.GetComponent<SpriteRenderer> ();
		groundStingParticle = groundSting.GetComponent<ParticleSystem> ();

		groundStingBlackScreen.color = new Color32 (0, 0, 0, 0); // Noir, pour réinitialiser
	}

	protected override void NormalLoad () {
		groundStingLength = groundStingNormalLength;
		groundStingDamage = groundStingNormalDamage;
	}

	protected override void HardLoad () {
		groundStingLength = groundStingHardLength;
		groundStingDamage = groundStingHardDamage;
	}

	protected override void HellLoad () {
		groundStingLength = groundStingHellLength;
		groundStingDamage = groundStingHellDamage;
	}

	protected override void ChooseAttack (int numberAttack) {
		switch (numberAttack) {
		case 1:
			timeToFire += groundStingAttackLength;

			// Animation du scorpion qui plante dans le sol
			myAnim.SetBool("groundSting", true);

			// L'appel de la fonction d'attaque "GroundSting" se fait dans l'Animator
			break;
		case 2:
			timeToFire += angryWaitAttackLength;

			// Animation du scorpion qui lève la queue
			myAnim.SetBool ("angryWait", true);

			StartCoroutine (AngryWait ());

			break;
		}
	}

	// ATTAQUE 1
	private void GroundSting () {
		StartCoroutine (GroundStingCoroutine ());
	}

	private IEnumerator GroundStingCoroutine () {
		// Ralentissement de Stinger (on ne va pas plus vite que 3, pour ne pas toucher le héros même en normal)
		moveSpeed = Mathf.Max(-LevelManager.player.moveSpeed, -3f);

		// On démarre l'animation du dard
		groundSting.SetActive (true);
		groundStingCollider.enabled = false;
		groundStingSprite.enabled = false;
		//groundStingBlackScreen.enabled = false;
		groundStingParticle.Play ();

		// Montée du dard
		float timeToGoUp = groundStingLength; // Le temps de montée
		float currentTime = 0;

		while (currentTime < timeToGoUp) {
			// Les particules vont depuis la queue du scorpion (ajustement) vers le héros
			groundSting.transform.position = new Vector2 (Mathf.Lerp (myTransform.position.x + 1, LevelManager.player.transform.position.x, currentTime / timeToGoUp), -0.5f);
			currentTime += TimeManager.deltaTime;
			yield return null;
		}

		// Apparition du dard
		groundSting.transform.position = new Vector2 (LevelManager.player.transform.position.x, 0);
		groundStingCollider.enabled = true;
		groundStingSprite.enabled = true;
		groundStingParticle.Stop ();

		// Maintien du dard
		float timeToGoDown = 0.25f; // 0.25s de temps de maintien
		currentTime = 0;

		while (currentTime < timeToGoDown) {
			groundSting.transform.position = new Vector2 (LevelManager.player.transform.position.x, 0);
			currentTime += TimeManager.deltaTime;
			yield return null;
		}

		// Animation du scorpion qui sort le dard du sol
		myAnim.SetBool("groundSting", false);
		yield return null; // On attend une frame que la queue sorte du sol

		// Disparition du dard
		groundStingCollider.enabled = false;
		groundStingSprite.enabled = false;

		// Retour chez lui du scorpion
		moveSpeed = 0;
		mySprite.flipX = true;
		while (myTransform.position.x < startPosition [0]) {
			myTransform.Translate (Vector3.right * 1.5f * LevelManager.levelManager.GetLocalDistance ()); // On force le déplacement (pas de moveSpeed) pour éviter d'éventuels problèmes de collisions
			yield return null;
		}
		mySprite.flipX = false;
	}

	// ATTAQUE 2
	private IEnumerator AngryWait () {
		isAngryWaiting = true;

		float currentTime = 0;

		// Interruption si le joueur touche le scorpion, ou après 3 secondes
		while (currentTime < angryWaitAttackLength && isAngryWaiting) {

			currentTime += TimeManager.deltaTime;
			yield return null;
		}

		// Animation du scorpion qui baisse la queue
		myAnim.SetBool ("angryWait", false);

		isAngryWaiting = false;
	}

	private void InavoidableStings () {
		Vector2 startingPoint = myTransform.position + Vector3.one * 0.65f; // Position de départ des dards
		Vector2 vectorSting; // Direction des dards, recalculée à chaque itération

		float numberOfSting = Mathf.CeilToInt (CameraManager.cameraUpPosition) - 2; // Le joueur ne peut pas être tout en haut, donc -2

		for (int i = 0; i < numberOfSting; i++) {
			GameObject obj = PoolingManager.current.Spawn (inavoidableSting.name);

			if (obj != null) {
				obj.transform.position = new Vector2 (startingPoint.x, startingPoint.y);

				obj.SetActive (true);

				vectorSting = Vector2.up * (i + 0.5f) - startingPoint; // Calcul de la direction du dard
				obj.GetComponent<Sting> ().SetCourse (3, vectorSting);
			}
		}
	}

	protected override void OnTriggerEnter2D (Collider2D other) {
		// Si l'ennemi est déjà mort, il ne peut plus rien faire...
		if( IsDead() )
			return;
		
		if (other.name == "Heros") {
			// On assume que si le collider du "GroundSting" est actif, le héros s'est fait toucher par ça
			if (groundStingCollider.isActiveAndEnabled) {
				LevelManager.player.Hurt (groundStingDamage, sharp);
				StartCoroutine (FadingBlackScreen ());
			}
		}

		if (other.CompareTag ("Bullet")) {
			// Si le boss est en attente vénère !
			if (isAngryWaiting) {
				InavoidableStings ();
				isAngryWaiting = false;
			}
		}
	}

	private IEnumerator FadingBlackScreen () {
		float timeToFade = 0.25f;
		float currentTime = 0;
		Color oldColor = groundStingBlackScreen.color;
		Color newColor = oldColor;
		newColor.a += (1 - newColor.a) / 2.5f; // On ajoute à chaque fois 1/2.5 de ce qu'il manque en alpha

		while (currentTime < timeToFade) {
			groundStingBlackScreen.color = Color.Lerp (oldColor, newColor, currentTime / timeToFade);
			currentTime += TimeManager.deltaTime;
			yield return null;
		}
	}

	// A la mort, le scorpion se pique
	protected override void Despawn () {
		// On arrête tout
		StopAllCoroutines ();

		// Dissipation de l'écran noir
		StartCoroutine (DissipateBlackScreen ());

		Die ();

		myAnim.SetTrigger ("dead");

		RaycastHit2D hit;
		hit = Physics2D.Raycast (myTransform.position, Vector2.down, 20, layerGround);

		if (hit.collider != null) {
			myTransform.parent = hit.transform;
			myRb.isKinematic = true;
			GetComponentInChildren<EdgeCollider2D> ().enabled = false;
		}
	}

	private IEnumerator DissipateBlackScreen () {
		float timeToDissipate = 1f;
		float currentTime = 0;
		Color currentColor = groundStingBlackScreen.color;
		Color transparentColor = new Color32 (0, 0, 0, 0);

		while (currentTime < timeToDissipate) {
			groundStingBlackScreen.color = Color.Lerp (currentColor, transparentColor, currentTime / timeToDissipate);
			currentTime += TimeManager.deltaTime;
			yield return null;
		}
	}
}

