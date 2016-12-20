using UnityEngine;

public class Pool : MonoBehaviour {

	void Start () {
		// Chargement différent selon la difficulté et le mode
		if (LevelManager.levelManager.IsStory ()) {
			switch (LevelManager.levelManager.GetCurrentDifficulty ()) {
			// Normal
			case 0:
				NormalLoad ();
				break;
				// Hard
			case 1:
				HardLoad ();
				break;
				// Hell
			case 2:
				HellLoad ();
				break;
			}
		} else // Arcade
			ArcadeLoad ();
	}

	protected virtual void NormalLoad () {
		// Action à réaliser en mode normal
	}

	protected virtual void HardLoad () {
		// Action à réaliser en mode hard
	}

	protected virtual void HellLoad () {
		// Action à réaliser en mode hell
	}

	protected virtual void ArcadeLoad () {
		// Action à réaliser en mode arcade
	}

	private void OnTriggerEnter2D (Collider2D other) {
		// Si ce n'est pas le joueur, on ne fait rien
		if (!other.CompareTag ("Player"))
			return;
		
		// Si le joueur n'est pas en train de voler
		// Si le joueur est déjà dans une flaque, on ne fait rien
		if (LevelManager.player.IsFlying () || LevelManager.player.pooled)
			return;
		
		// On déclare que le joueur est dans une flaque
		LevelManager.player.pooled = true;

		// On déclenche l'effet d'entrée
		EnterEffect ();
	}

	private void OnTriggerStay2D (Collider2D other) {
		// Si ce n'est pas le joueur, on ne fait rien
		if (!other.CompareTag ("Player"))
			return;

		// Si le joueur est en train de voler, on ne fait rien
		if (LevelManager.player.IsFlying ())
			return;

		// On déclare que le joueur est dans une flaque (au cas où il est déjà dedans mais que son vol s'arrête, par exemple)
		LevelManager.player.pooled = true;

		// On délenche l'effet de contact
		StayEffect ();
	}

	private void OnTriggerExit2D (Collider2D other) {
		// Si ce n'est pas le joueur, on ne fait rien
		if (!other.CompareTag ("Player"))
			return;

		// On déclare que le joueur n'est plus dans une flaque
		LevelManager.player.pooled = false;

		// On déclenche l'effet de sortie
		ExitEffect ();
	}

	protected virtual void EnterEffect () {
		// Effet à déclencher lors de l'entrée du joueur dans la zone
	}

	protected virtual void StayEffect () {
		// Effet à déclencher tant que le joueur est dans la zone
	}

	protected virtual void ExitEffect () {
		// Effet à déclencher lors de la sortie du joueur de la zone
	}
}
