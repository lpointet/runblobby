﻿using UnityEngine;
using System.Collections;

public class StartLevel : MonoBehaviour {

	private SpriteRenderer myRenderer;

	public float delayBeforeStart = 1.5f;
	private float startingTime;

	private float startingTimeScale = 0;
	private float endingScale;	// Doit avoir la taille de l'écran à la fin
	private float currentScaling;
	private float modifCamScaling;

	void Start () {
		startingTime = Time.unscaledTime;

		myRenderer = GetComponent<SpriteRenderer> ();

		CameraManager cameraManager = Camera.main.GetComponent<CameraManager> ();

		endingScale = 96; // TODO trouver une formule pour ça...
		modifCamScaling = cameraManager.bgContainer.transform.localScale.x * 2.5f; // *2.5f Pour éviter d'être hors caméra TODO formule aussi

		// On place le joueur dans de bonnes conditions de chute en parachute
		LevelManager.GetPlayer ().GetComponent<Rigidbody2D> ().gravityScale = 0.1f;
		LevelManager.GetPlayer ().transform.position = new Vector2 (0, Camera.main.orthographicSize + cameraManager.yOffset + 1);

		// On accroche le parachute au joueur
		LevelManager.GetPlayer ().ActiveParachute (true);
	}

	void Update () {
		// On attend la fin du délai pour démarrer
		if (startingTime + delayBeforeStart < Time.unscaledTime) {
			LevelManager.GetPlayer ().ActiveParachute (false);
			LevelManager.levelManager.StartLevel ();
			gameObject.SetActive (false);
		} else {
			// On agrandit la taille du cercle au fil du temps
			startingTimeScale = Time.unscaledTime * Time.unscaledTime;
			currentScaling = _StaticFunction.MappingScale (startingTimeScale, 0, delayBeforeStart, 1, endingScale);
			transform.localScale = Vector2.one * currentScaling * modifCamScaling;

			// On fait suivre au cercle le joueur (mais pas sur z)
			transform.position = new Vector3 (LevelManager.GetPlayer ().transform.position.x, LevelManager.GetPlayer ().transform.position.y, transform.position.z);
		}
	}
}
