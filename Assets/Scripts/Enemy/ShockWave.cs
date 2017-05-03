using UnityEngine;
using System.Collections;

public class ShockWave : MonoBehaviour {

	private Rigidbody2D myRb;
	private Transform myTransform;
	private ParticleSystem myParticle;
	private ParticleSystem.MainModule particleMain;

	[SerializeField] private float moveSpeed;
	[SerializeField] private float fullSizeTime; // Temps qu'il met à avoir sa taille maximale
	[SerializeField] private float fullSizeScale; // Taille maximale
	private float currentLifeTime;

	[Header("HP")]
	[SerializeField] private int normalHP;
	[SerializeField] private int hardHP;
	[SerializeField] private int hellHP;
	private int healthPoint; // HP de l'onde de choc
	private int initialHP;

	[Header("Damage")]
	[SerializeField] private int normalDamage;
	[SerializeField] private int hardDamage;
	[SerializeField] private int hellDamage;
	private int damageToPlayer = 1;

	void Awake () {
		myRb = GetComponent<Rigidbody2D> ();
		myTransform = transform;
		myParticle = GetComponent<ParticleSystem> ();
		particleMain = myParticle.main;

		// Chargement différent selon la difficulté et le mode
		if (LevelManager.levelManager.IsStory ()) {
			switch (LevelManager.levelManager.GetCurrentDifficulty ()) {
			// Normal
			case 0:
				damageToPlayer = normalDamage;
				healthPoint = normalHP;
				break;
			// Hard
			case 1:
				damageToPlayer = hardDamage;
				healthPoint = hardHP;
				break;
			// Hell
			case 2:
				damageToPlayer = hellDamage;
				healthPoint = hellHP;
				break;
			}
		}

		initialHP = healthPoint;
	}

	void OnEnable () {
		healthPoint = initialHP;
		currentLifeTime = 0;

		// Vitesse en fonction de l'angle
		float rotaZ = myTransform.eulerAngles.z * Mathf.Deg2Rad;
		myRb.velocity = new Vector2 (-Mathf.Cos (rotaZ), -Mathf.Sin (rotaZ)) * moveSpeed;

		// Angle des particules
		particleMain.startRotation = new ParticleSystem.MinMaxCurve (-rotaZ);

		myTransform.localScale = Vector3.one * 0.5f;
	}

	void Update () {
		if (LevelManager.player.IsDead () || TimeManager.paused)
			return;
		
		if (LevelManager.levelManager.GetEnemyEnCours () == null)
			Despawn ();
		
		// Agrandir de plus en plus
		myTransform.localScale = Vector3.Lerp (Vector3.one, Vector3.one * fullSizeScale, currentLifeTime / fullSizeTime);
		currentLifeTime += TimeManager.deltaTime;
	}

	void OnTriggerEnter2D (Collider2D other) {
		if (other.CompareTag ("Bullet")) {
			other.gameObject.SetActive (false);

			Hit ();

			if (healthPoint <= 0) // Si l'onde de choc n'a plus de HP, elle disparait
				Despawn ();
		}
	}

	void OnBecameInvisible () {
		Despawn ();
	}

	private void Hit () {
		healthPoint--; // On enlève un HP par tir (fixe)

		myTransform.Translate (Vector2.right); // On le fait reculer un peu
	}

	private void Despawn () {
		gameObject.SetActive (false);
	}
}
