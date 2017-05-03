using System.Collections;
using UnityEngine;

public class Firefly : MonoBehaviour {
	
	private Transform myTransform;
	private Vector3 currentPosition;
	private AudioSource myAudio;
	private ParticleSystem myParticle;

	[SerializeField] private FlashLight flashlight; // Référence au script de la lumière
	private Material lightMaterial; // Référence au Material de la lumière (pour changer la position du trou)
	private float holeRadius; // Rayon "large" du trou de lumière pour déterminer quand le faire disparaitre et apparaitre (en unité)
	public int brightLifetime { get; private set; } // Durée de vie d'une luciole avant de s'affaiblir (temps de déplacement d'une luciole sur l'écran)

	private bool activeFirefly = false; // Permet de contrôler la présence ou non d'une luciole

	[SerializeField] private float initialMoveSpeed = 1.0f;
	[SerializeField] private float initialFrequency = 1.0f;
	[SerializeField] private float initialMagnitude = 1.0f;
	private float moveSpeed = 1.0f;
	private float frequency = 1.0f;
	private float magnitude = 1.0f;

	[SerializeField] private float randomDelay; // Pour permettre un mouvement plus aléatoire de la luciole
	private float timeBeforeRandom;
	private float smoothTime = 2.0f;
	private Vector3 aimingPosition;
	private Vector3 currentVelocity;

	public float GetMoveSpeed () {
		return moveSpeed;
	}

	void Awake () {
		myTransform = transform;
		myAudio = GetComponent<AudioSource> ();
		myParticle = GetComponent<ParticleSystem> ();
		lightMaterial = flashlight.GetComponent<Renderer> ().sharedMaterial;

		lightMaterial.SetVector ("_Center", new Vector2(25, 25)); // Loin de la caméra
	}

	void Start () {
		// Chargement différent selon la difficulté et le mode
		if (LevelManager.levelManager.IsStory ()) {
			switch (LevelManager.levelManager.GetCurrentDifficulty ()) {
			// Normal
			case 0:
				gameObject.SetActive (false);
				break;
			// Hard
			case 1:
				Init ();
				break;
			// Hell
			case 2:
				Init ();
				break;
			}
		}

		holeRadius = 2.0f * Camera.main.orthographicSize * Camera.main.aspect * lightMaterial.GetFloat ("_Radius");
	}

	private void Init () {
		Invoke ("EnableFirefly", Time.timeScale * Random.Range (1.0f, 3.0f));

		// Calcul de sa durée de vie en fonction de l'écran et de sa vitesse moyenne
		brightLifetime = Mathf.CeilToInt (Camera.main.orthographicSize * Camera.main.aspect * 2 / initialMoveSpeed);
	}

	public void EnableFirefly () {
		gameObject.SetActive (true);
		activeFirefly = true;
		myAudio.pitch = 1.0f + Random.Range (-0.25f, 0.35f);
		myAudio.Play ();

		// Position à droite de l'écran (+holeRadius pour le trou de lumière), pas trop bas (4.0f), et jusqu'à 7.0f
		myTransform.position = new Vector3 (CameraManager.cameraRightPosition + holeRadius, CameraManager.cameraDownPosition + Random.Range (4.0f, 7.0f), -1.0f);
		currentPosition = myTransform.position;

		RandomizeMovement ();
	}

	void Update () {
		if (!activeFirefly || LevelManager.player.IsDead () || TimeManager.paused)
			return;
		
		currentPosition += Vector3.left * moveSpeed * TimeManager.deltaTime;
		aimingPosition = currentPosition + Vector3.up * magnitude * Mathf.Sin (TimeManager.time * frequency);

		myTransform.position = Vector3.SmoothDamp (myTransform.position, aimingPosition, ref currentVelocity, smoothTime);

		// Position + taille du trou de lumière
		lightMaterial.SetVector ("_Center", (Vector2) Camera.main.WorldToViewportPoint (myTransform.position) + lightMaterial.mainTextureOffset);
		lightMaterial.SetFloat ("_Radius", 0.15f + 0.01f * Mathf.Sin (TimeManager.time * 7.5f));

		if (TimeManager.time > timeBeforeRandom)
			RandomizeMovement ();
	}

	private void RandomizeMovement () {
		moveSpeed = initialMoveSpeed * Random.Range (0.75f, 1.25f);
		frequency = initialFrequency * Random.Range (0.8f, 1.2f);
		magnitude = initialMagnitude * Random.Range (0.5f, 1.5f);

		timeBeforeRandom = TimeManager.time + randomDelay * Random.Range (0.5f, 1.5f);
	}

	private void OnBecameInvisible () {
		if (!activeFirefly)
			return;
		// On attend que le "trou" de lumière sorte aussi de l'écran
		if (myTransform.position.x > CameraManager.cameraLeftPosition - holeRadius)
			return;
		
		Despawn ();
	}

	private void OnTriggerEnter2D (Collider2D other) {
		if (other.name == "Heros") {
			flashlight.GetNewFirefly ();

			// Effet d'explosion de particules
			ParticleSystem.EmitParams overrideEmit = new ParticleSystem.EmitParams ();
			overrideEmit.velocity = 2.0f * Vector3.up;
			myParticle.Emit (overrideEmit, 15);

			Invoke ("Despawn", 1.0f);
		}
	}

	public void Despawn () {
		Invoke ("EnableFirefly", Time.timeScale * Random.Range (2.0f, 5.0f));

		// Position du trou de lumière loin de tout
		lightMaterial.SetVector ("_Center", new Vector2(25, 25));

		myAudio.Stop ();
		activeFirefly = false;
		gameObject.SetActive (false);

	}
}
