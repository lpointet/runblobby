using UnityEngine;

public class StartEnemyBlock : MonoBehaviour {

	private Collider2D circleStartingPhase;

	void Awake() {
		circleStartingPhase = GetComponent<CircleCollider2D> ();
	}

	void Update() {
		circleStartingPhase.offset = new Vector2 (circleStartingPhase.offset.x, LevelManager.player.transform.position.y + LevelManager.levelManager.GetHeightStartBlock());
	}

	void OnTriggerEnter2D(Collider2D other) {
		if (other.name == "Heros") {
			LevelManager.levelManager.SetEnemyToSpawn( true ); // Indique au levelManager d'invoquer l'ennemi
			circleStartingPhase.enabled = false; // Empêche l'apparition de multiples ennemis sur les configurations trop lentes
		}
	}
}
