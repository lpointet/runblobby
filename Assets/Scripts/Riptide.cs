using System.Collections;
using UnityEngine;

public class Riptide : MonoBehaviour {

	[SerializeField] private float switchingSpeed; // Synchro avec la vitesse des caustiques
	[SerializeField] private float magnitude;

	void Start () {
		// Chargement différent selon la difficulté et le mode
		if (LevelManager.levelManager.IsStory ()) {
			switch (LevelManager.levelManager.GetCurrentDifficulty ()) {
			// Normal
			case 0:
			// Hard
			case 1:
				gameObject.SetActive (false);
				return;
			// Hell
			case 2:
				break;
			}
		} else
			return;
	}

	void Update () {
		if (LevelManager.player.IsDead () || TimeManager.paused)
			return;

		// Fait varier la vitesse du héros en sinusoïde
		LevelManager.levelManager.addedMoveSpeed = magnitude * Mathf.Sin (switchingSpeed * TimeManager.time);
	}
}
