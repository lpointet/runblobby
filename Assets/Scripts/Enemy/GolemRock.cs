using System.Collections;
using UnityEngine;

public class GolemRock : MonoBehaviour {

	private Transform myTransform;
	private Rigidbody2D myRb;
	private CircleCollider2D groundCollider;
	private SpriteRenderer mySprite;

	private Enemy golemParent;							// Origine de la pierre

	[SerializeField] private LayerMask playerBulletLayer; 	// Layer sur laquelle évolue les balles du joueur
	[SerializeField] private LayerMask enemyDropLayer; 		// Layer des tirs ennemis
	[SerializeField] private LayerMask groundLayer; 		// Layer du sol
	private int damageOnGolem; 							// Les dégâts à infliger au golem en cas de retour
	[SerializeField] private int normalDamageOnGolem;
	[SerializeField] private int hardDamageOnGolem;
	[SerializeField] private int hellDamageOnGolem;
	private bool hasBeenHitByHero;						// Permet de savoir si le joueur a touché le rocher avec un tir
	private bool isGrounded = false;					// Permet de savoir s'il faut doubler les dégâts sur le golem (si au sol au moment du contact avec le tir, dégât simple, sinon dégât double)

	[SerializeField] private ExplosionRadius explosionEffect;
	private bool exploded = false;

	void Awake () {
		myTransform = transform;
		myRb = GetComponent<Rigidbody2D> ();
		mySprite = GetComponent<SpriteRenderer> ();
		groundCollider = GetComponentsInChildren<CircleCollider2D> () [1];
	}

	void OnEnable () {
		myRb.isKinematic = false;
		hasBeenHitByHero = false;
		isGrounded = false;
		exploded = false;

		myRb.isKinematic = false;
		mySprite.enabled = true;
		groundCollider.isTrigger = false;
	}

	void Start () {
		golemParent = LevelManager.levelManager.GetEnemyEnCours ();

		// Chargement différent selon la difficulté et le mode
		if (LevelManager.levelManager.IsStory ()) {
			switch (LevelManager.levelManager.GetCurrentDifficulty ()) {
			// Normal
			case 0:
				damageOnGolem = normalDamageOnGolem;
				break;
			// Hard
			case 1:
				damageOnGolem = hardDamageOnGolem;
				break;
			// Hell
			case 2:
				damageOnGolem = hellDamageOnGolem;
				break;
			}
		}
	}

	void Update () {
		if (myTransform.position.x < CameraManager.cameraLeftPosition - 1.0f)
			Despawn ();
	}

	void OnTriggerEnter2D (Collider2D other) {
		if (other.name == "Heros") {
			Explode ();
		}

		// Si touché par une autre pierre, double explosion !
		if ((1 << other.gameObject.layer & enemyDropLayer) != 0) {
			Explode ();
		}

		// Si touché par une balle, retour à l'envoyeur !
		if ((1 << other.gameObject.layer & playerBulletLayer) != 0) {
			if (exploded)
				return;
			hasBeenHitByHero = true;
			ReturnToGolem ();
		}

		// Double dégât tant qu'il est en l'air
		// Fait sauter le héros s'il est au sol au premier impact
		if ((1 << other.gameObject.layer & groundLayer) != 0) {
			if (LevelManager.player.IsGrounded () && !isGrounded) {
				LevelManager.player.ShakePlayer (0.25f, true, true, true);
			}
			isGrounded = true;

			CameraManager.cameraManager.ShakeScreen (4, 0.25f);
		}

		// On ne peut toucher le golem que si le héros a déjà touché la pierre avec un tir
		if (!hasBeenHitByHero)
			return;
		
		if (other.GetComponent<Enemy> () != null) {
			other.GetComponent<Enemy> ().Hurt (isGrounded ? damageOnGolem : damageOnGolem * 2.0f, 0, true);

			Explode ();
		}
	}

	private void ReturnToGolem () {
		if (golemParent == null)
			Explode ();
		
		myRb.velocity = (golemParent.transform.position - myTransform.position).normalized * 2.5f;
		myRb.isKinematic = true;
	}

	private void Explode () {
		if (exploded)
			return;

		myRb.velocity = Vector2.zero;
		myRb.angularVelocity = 0;
		myRb.isKinematic = true;
		groundCollider.isTrigger = true;
		mySprite.enabled = false;

		explosionEffect.StartExplosion ();

		Invoke ("Despawn", 1.0f + Time.timeScale * explosionEffect.GetExplosionTime ()); // +1sec pour s'assurer que c'est bon

		exploded = true;
	}

	private void Despawn () {
		gameObject.SetActive (false);
	}
}
