using System.Collections;
using UnityEngine;

public class PoisonBall : MonoBehaviour {

	[SerializeField] private ExplosionRadius explosionEffect;

	private LayerMask groundLayer;

	public float creationTime { get; private set; } // Temps d'invocation de la boule
	[SerializeField] private float normalCreationTime;
	[SerializeField] private float hardCreationTime;
	[SerializeField] private float hellCreationTime;
	private bool explosed = false;

	void Awake () {
		groundLayer = LayerMask.NameToLayer ("Ground");

		// Chargement différent selon la difficulté et le mode
		// Placé autre part que dans le Start () pour que "creationTime" soit immédiatement disponible à la première invocation
		if (LevelManager.levelManager.IsStory ()) {
			switch (LevelManager.levelManager.GetCurrentDifficulty ()) {
			// Normal
			case 0:
				creationTime = normalCreationTime;
				break;
				// Hard
			case 1:
				creationTime = hardCreationTime;
				break;
				// Hell
			case 2:
				creationTime = hellCreationTime;
				break;
			}
		}
	}

	void Update () {
		// Destruction s'il l'ennemi n'est plus là
		if (LevelManager.levelManager.GetEnemyEnCours () == null) {
			gameObject.SetActive (false);
		}
	}

	void OnEnable () {
		explosed = false;
	}

	void OnTriggerEnter2D (Collider2D other) {
		if (explosed) // On n'explose qu'une seule fois
			return;
		
		if (other.gameObject.layer == groundLayer) {
			explosed = true;
			explosionEffect.StartExplosion ();
			transform.parent = other.transform;
			GetComponent<Rigidbody2D> ().velocity = Vector2.zero;
		}
	}
}
