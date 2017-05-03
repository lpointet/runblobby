using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FloatingBubble : MonoBehaviour {

	private Transform myTransform;

	private float damageToGive;
	[SerializeField] private float normalDamageToGive;
	[SerializeField] private float hardDamageToGive;
	[SerializeField] private float hellDamageToGive;

	private float moveSpeed;
	[SerializeField] private float normalMoveSpeed;
	[SerializeField] private float hardMoveSpeed;
	[SerializeField] private float hellMoveSpeed;

	private float startingOffset; // Valeur qui permet d'assurer que le premier sinus qu'on va tracer sera toujours ascendant à partir de 45° = \_
	private float waveFrequency = 2.5f;
	private float waveAmplitude = 0.05f;

	private float currentTime;

	void Awake () {
		myTransform = transform;
		startingOffset = Mathf.Sin (Mathf.PI / 4.0f);
	}

	void OnEnable () {
		currentTime = 0;
	}

	void Start() {
		// Chargement différent selon la difficulté et le mode
		if (LevelManager.levelManager.IsStory ()) {
			switch (LevelManager.levelManager.GetCurrentDifficulty ()) {
			// Normal
			case 0:
				moveSpeed = normalMoveSpeed;
				damageToGive = normalDamageToGive;
				break;
			// Hard
			case 1:
				moveSpeed = hardMoveSpeed;
				damageToGive = hardDamageToGive;
				break;
			// Hell
			case 2:
				moveSpeed = hellMoveSpeed;
				damageToGive = hellDamageToGive;
				break;
			}
		}
	}

	void Update () {
		if (LevelManager.player.IsDead () || TimeManager.paused || !gameObject.activeInHierarchy)
			return;
		
		myTransform.Translate (Vector3.left * moveSpeed * TimeManager.deltaTime + Vector3.up * waveAmplitude * Mathf.Sin (waveFrequency * currentTime + startingOffset));

		// Désactivation "neutre" quand on sort de l'écran
		if (myTransform.position.x < CameraManager.cameraStartPosition)
			gameObject.SetActive(false);

		currentTime += TimeManager.deltaTime;
	}

	void OnTriggerEnter2D (Collider2D other) {
		if (LevelManager.levelManager.GetEnemyEnCours () == null)
			return;

		if (other.name == "Heros") {
			LevelManager.player.Hurt (damageToGive, LevelManager.levelManager.GetEnemyEnCours ().sharp);
			Despawn ();
		}
	}

	public void Despawn () {
		CameraManager.cameraManager.ShakeScreen (1, 0.25f);
		gameObject.SetActive (false);
	}
}