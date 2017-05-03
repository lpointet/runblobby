using System.Collections;
using UnityEngine;

public class PoolCroupie : Pool {

	[SerializeField] private FlashLight flashLight;
	[SerializeField] private float timeBetweenDarkening = 0.1f;
	private float timeToDark;
	[SerializeField] private float cutoffDecay = 0.25f;

	void Start () {
		// Chargement différent selon la difficulté et le mode
		if (LevelManager.levelManager.IsStory ()) {
			switch (LevelManager.levelManager.GetCurrentDifficulty ()) {
			// Normal
			case 0:
				this.enabled = false;
				return;
				// Hard
			case 1:

				break;
				// Hell
			case 2:

				break;
			}
		}

		if (flashLight == null)
			flashLight = GameObject.Find ("FlashLight").GetComponent<FlashLight> ();
		
		timeToDark = TimeManager.time;
	}

	protected override void StayEffect () {
		if (TimeManager.time > timeToDark) {
			flashLight.Darkening (cutoffDecay);
			timeToDark = TimeManager.time + timeBetweenDarkening;
		}
	}

	/*void OnTriggerStay2D (Collider2D other) {
		if (other.name == "Heros") {
			if (TimeManager.time > timeToDark) {
				flashLight.Darkening (cutoffDecay);
				timeToDark = TimeManager.time + timeBetweenDarkening;
			}
		}
	}*/
}
