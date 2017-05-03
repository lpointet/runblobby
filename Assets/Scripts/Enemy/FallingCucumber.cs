using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FallingCucumber : MonoBehaviour {

	private Transform myTransform;
	private Rigidbody2D myRb;
	private SpriteRenderer mySprite;
	private Collider2D myCollider;

	private bool isHeroHit;
	private bool isDespawned;

	[SerializeField] private ParticleSystem endEffect;

	private float damageToGive;
	[SerializeField] private float normalDamageToGive;
	[SerializeField] private float hardDamageToGive;
	[SerializeField] private float hellDamageToGive;

	private float moveSpeed;
	[SerializeField] private float normalMoveSpeed;
	[SerializeField] private float hardMoveSpeed;
	[SerializeField] private float hellMoveSpeed;

	void Awake () {
		myTransform = transform;
		myRb = GetComponent<Rigidbody2D> ();
		mySprite = GetComponent<SpriteRenderer> ();
		myCollider = GetComponent<Collider2D> (); // Retourne le premier

		// Chargement différent selon la difficulté et le mode
		if (LevelManager.levelManager.IsStory ()) {
			switch (LevelManager.levelManager.GetCurrentDifficulty ()) {
			// Normal
			case 0:
				moveSpeed = normalMoveSpeed;
				damageToGive = normalDamageToGive;
				break;
			// Hard
			case 1:
				moveSpeed = hardMoveSpeed;
				damageToGive = hardDamageToGive;
				break;
			// Hell
			case 2:
				moveSpeed = hellMoveSpeed;
				damageToGive = hellDamageToGive;
				break;
			}
		}
	}

	void OnEnable () {
		Reset ();

		// On choisit l'angle de chute en fonction de sa position de départ et de la taille de l'écran
		myTransform.position = new Vector2 (Random.Range (CameraManager.cameraLeftPosition, CameraManager.cameraRightPosition), CameraManager.cameraUpPosition + 1.0f);

		if (myTransform.position.x < CameraManager.cameraManager.xOffset)
			myRb.velocity = Vector2.right * moveSpeed;
		else
			myRb.velocity = Vector2.left * moveSpeed;
	}

	void Update () {
		// Si l'objet à "touché" le sol, il n'a plus de vitesse, et on le fait disparaître
		if (!isDespawned && myTransform.position.y < 2.0f && Mathf.Abs (myRb.velocity.y) <= 0.01f)
			HitGround ();
	}

	void OnTriggerEnter2D (Collider2D other) {
		if (isHeroHit || LevelManager.levelManager.GetEnemyEnCours () == null)
			return;

		if (other.name == "Heros") {
			LevelManager.player.Hurt (damageToGive, LevelManager.levelManager.GetEnemyEnCours ().sharp);
			isHeroHit = true;
			Despawn ();
		}
	}

	public void Despawn () {
		if (isDespawned)
			return;
		
		isDespawned = true;

		mySprite.enabled = false;
		myCollider.isTrigger = true;
		endEffect.Play ();

		Invoke ("Disable", 5.0f * Time.timeScale);
	}

	private void Reset () {
		isHeroHit = false;
		isDespawned = false;
		mySprite.enabled = true;
		myCollider.isTrigger = false;
	}

	private void Disable () {
		endEffect.Stop ();
		endEffect.Clear ();

		gameObject.SetActive (false);
	}

	private void HitGround () {
		CameraManager.cameraManager.ShakeScreen (1, 0.25f);
		Despawn ();
	}

	private void TouchDetected () {
		Despawn ();
	}
}