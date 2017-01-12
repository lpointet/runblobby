using UnityEngine;
using System.Collections;

public class Firebreath : MonoBehaviour {

	private Rigidbody2D myRb;
	private Transform myTransform;

	[SerializeField] private float moveSpeed;
	[SerializeField] private float fullSizeTime; // Temps qu'il met à avoir sa taille maximale
	[SerializeField] private float fullSizeScale; // Taille maximale
	private float currentLifeTime;

	private int damageToPlayer = 1;
	[SerializeField] private int normalDamage;
	[SerializeField] private int hardDamage;
	[SerializeField] private int hellDamage;

	void Awake () {
		myRb = GetComponent<Rigidbody2D> ();
		myTransform = transform;

		// Chargement différent selon la difficulté et le mode
		if (LevelManager.levelManager.IsStory ()) {
			switch (LevelManager.levelManager.GetCurrentDifficulty ()) {
			// Normal
			case 0:
				damageToPlayer = normalDamage;
				break;
				// Hard
			case 1:
				damageToPlayer = hardDamage;
				break;
				// Hell
			case 2:
				damageToPlayer = hellDamage;
				break;
			}
		}
	}

	void OnEnable () {
		currentLifeTime = 0;

		// Vitesse en fonction de l'angle
		float rotaZ = myTransform.eulerAngles.z * Mathf.Deg2Rad;
		myRb.velocity = new Vector2 (-Mathf.Cos (rotaZ), -Mathf.Sin (rotaZ)) * moveSpeed;

		myTransform.localScale = Vector3.one * 0.5f;
	}

	void Update () {
		// Agrandir de plus en plus
		myTransform.localScale = Vector3.Lerp (Vector3.one, Vector3.one * fullSizeScale, currentLifeTime / fullSizeTime);
		currentLifeTime += TimeManager.deltaTime;
	}

	void OnTriggerEnter2D (Collider2D other) {
		if (other.CompareTag ("Player")) {
			LevelManager.player.Hurt (damageToPlayer);

			Despawn ();
		}
	}

	void OnBecameInvisible () {
		Despawn ();
	}

	private void Despawn () {
		gameObject.SetActive (false);
	}
}
