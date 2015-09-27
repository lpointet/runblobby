using UnityEngine;

public class StartEnemyBlock : MonoBehaviour {

	private Collider2D circleStartingPhase;

	void Awake() {
		circleStartingPhase = GetComponent<CircleCollider2D> ();
	}

	void Update() {
		circleStartingPhase.offset = new Vector2 (circleStartingPhase.offset.x, LevelManager.getPlayer ().transform.position.y + 1); // Décalage de 1 pour qu'il soit au dessus du sol
	}

	void OnTriggerEnter2D(Collider2D other) {
		if (other.name == "Heros") {
			LevelManager.levelManager.SetEnemyToSpawn( true ); // Indique au levelManager d'invoquer l'ennemi
		}
	}
}
