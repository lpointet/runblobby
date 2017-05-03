using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// Le pollen se charge au fur et à mesure que le joueur reste dans la zone de pollen
// Lorsque le joueur sort de la zone de pollen, la charge diminue doucement
// Une fois la charge maximale atteinte, le joueur "éternue" et recule de quelques pas, et la charge se vide

[RequireComponent(typeof(AudioSource))]
public class PollenEffect : MonoBehaviour {

	public static PollenEffect current;

	private ParticleSystem pollenParticle;
	private ParticleSystem.MainModule pollenParticleMain;
	private BoxCollider2D pollenCollider;
	private AudioSource myAudio;

	public ParticleSystem pollenBall;
	private ParticleSystem.MainModule pollenBallMain;
	private ParticleSystem.ShapeModule ballShape;
	private ParticleSystem.EmissionModule ballEmission;
	private ParticleSystem.MinMaxCurve ballRate;
	private Transform pollenBallTransform;

	public Color cleanColor;
	public Color sneezeColor;

	public float ratioHeight = 0.4f;

	private Rigidbody2D playerRb;

	// Données concernant la charge de pollen
	private bool inCharge = true;		// Permet de savoir s'il faut charger ou décharger
	private bool goingToSneeze = false;	// Permet de savoir si on a atteint la jauge maximale pour ne pas la baisser dans ce cas
	private float currentCharge = 0f;	// Charge actuelle
	private float maxCharge = 10f;		// Charge à partir de laquelle le joueur commence à éternuer
	private float ratioCharge;
	private float chargeSpeed = 1f;		// Gain de charge dans la zone pendant 1 seconde
	private float dischargeSpeed = 2f;	// Perte de charge hors zone pendant 1 seconde
	private float backForce = 5000f;	// Force de recul du joueur lors de l'éternument

	[Header("Hard Parameters")]
	public float hardMaxCharge;
	public float hardChargeSpeed;
	public float hardDischargeSpeed;
	public float hardBackForce;

	[Header("Hell Parameters")]
	public float hellMaxCharge;
	public float hellChargeSpeed;
	public float hellDischargeSpeed;
	public float hellBackForce;

	void Awake () {
		if (current == null)
			current = this;
		
		pollenParticle = GetComponentInChildren<ParticleSystem> ();
		pollenParticleMain = pollenParticle.main;

		pollenBallMain = pollenBall.main;
		pollenBallTransform = pollenBall.transform;

		pollenCollider = GetComponent<BoxCollider2D> ();

		myAudio = GetComponent<AudioSource> ();

		playerRb = LevelManager.player.GetComponent<Rigidbody2D> ();
	}

	void Start () {
		// Chargement différent selon la difficulté
		switch (LevelManager.levelManager.GetCurrentDifficulty ()) {
		// Normal
		case 0:
			gameObject.SetActive (false);

			break;
		// Hard
		case 1:
			gameObject.SetActive (true);
			// Paramètres
			maxCharge = hardMaxCharge;
			chargeSpeed = hardChargeSpeed;
			dischargeSpeed = hardDischargeSpeed;
			backForce = hardBackForce;

			LoadPollen ();

			break;
		// Hell
		case 2:
			gameObject.SetActive (true);
			// Paramètres
			maxCharge = hellMaxCharge;
			chargeSpeed = hellChargeSpeed;
			dischargeSpeed = hellDischargeSpeed;
			backForce = hellBackForce;

			LoadPollen ();

			break;
		}
	}

	private void LoadPollen() {
		// Positionnement en bas de l'écran
		transform.position = new Vector3 (CameraManager.cameraManager.xOffset, CameraManager.cameraManager.yOffset - (1 - ratioHeight) * Camera.main.orthographicSize, transform.position.z);

		// Ajustement de la zone d'émission des particules et de la zone de collision
		float widthScreen = Camera.main.orthographicSize * Camera.main.aspect * 2;
		float heightScreen = ratioHeight * Camera.main.orthographicSize * 2;

		ParticleSystem.ShapeModule boxShape;
		boxShape = pollenParticle.shape;
		boxShape.box = new Vector3 (widthScreen, heightScreen, pollenParticle.shape.box.z);

		pollenCollider.size = new Vector2 (widthScreen, heightScreen);

		// Ajustement du nombre de particules en fonction de la taille
		ParticleSystem.MinMaxCurve numberParticle;
		float constantRatio = 0.15f;
		numberParticle = new ParticleSystem.MinMaxCurve (Mathf.FloorToInt (boxShape.box.x * boxShape.box.y * constantRatio));

		ParticleSystem.EmissionModule emission;
		emission = pollenParticle.emission;
		emission.rateOverTime = numberParticle;

		// Balle
		ballEmission = pollenBall.emission;
		ballShape = pollenBall.shape;
	}

	void Update() {
		if (TimeManager.paused || LevelManager.player.IsDead () || LevelManager.IsEndingScene ())
			return;

		// Positionnement au pied du joueur
		pollenBallTransform.position = LevelManager.player.transform.position;

		if (inCharge) {
			currentCharge += chargeSpeed * TimeManager.deltaTime;

			if (currentCharge >= maxCharge) {
				if (!goingToSneeze) {
					goingToSneeze = true;
					StartCoroutine (SneezeThePlayer ());
				}
			}
		} else if (!goingToSneeze) {
			currentCharge -= dischargeSpeed * TimeManager.deltaTime;
			if (currentCharge < 0)
				currentCharge = 0;
		}

		if (!goingToSneeze) {
			ratioCharge = currentCharge / maxCharge;
			ballRate = new ParticleSystem.MinMaxCurve (Mathf.FloorToInt (Mathf.Lerp (0, 25, ratioCharge)));
			ballEmission.rateOverTime = ballRate;
			pollenBallMain.startColor = Color.Lerp (cleanColor, sneezeColor, ratioCharge);
			ballShape.arc = Mathf.Lerp (0, 360, ratioCharge);
			pollenParticleMain.startSize = 0.05f * Mathf.Lerp (1.0f, 2.5f, ratioCharge);
		}
	}

	// Quand le joueur entre, on met à jour le bool pour charger la barre
	void OnTriggerEnter2D(Collider2D other) {
		if (other.name == "Heros" && !LevelManager.player.IsDead()) {
			inCharge = true;
		}
	}

	// Quand le joueur sort, on met à jour le bool pour décharger la barre
	void OnTriggerExit2D(Collider2D other) {
		if (other.name == "Heros" && !LevelManager.player.IsDead()) {
			inCharge = false;
		}
	}

	private IEnumerator SneezeThePlayer() {
		// On attend que le joueur soit au sol pour le faire reculer
		// S'il vole, on éternue jamais
		while (!LevelManager.player.IsGrounded () || LevelManager.player.IsFlying())
			yield return null;

		playerRb.AddForce (Vector2.left * backForce);

		myAudio.pitch = 1.0f + Random.Range (-0.25f, 0.35f);
		myAudio.Play ();

		// On enlève la charge de pollen quand il tousse
		StartCoroutine(RemovePollen(1));
	}

	// Permet de lancer la Coroutine depuis une autre fonction
	public void CoroutineRemovePollen(float pollenDischarge, float timeToClean = 0.5f) {
		StartCoroutine(RemovePollen(pollenDischarge, timeToClean));
	}

	private IEnumerator RemovePollen(float pollenDischarge, float timeToClean = 0.5f) {
		// pollenDischarge représente la valeur de pollen qu'on enlève en taux, donc entre 0 et 1
		pollenDischarge = Mathf.Clamp01 (pollenDischarge);
		
		float currentTime = 0;

		float startingCharge = currentCharge;
		float endingCharge = currentCharge - pollenDischarge * currentCharge;

		while (currentTime < timeToClean) {
			currentCharge = Mathf.Lerp (startingCharge, endingCharge, currentTime / timeToClean);

			currentTime += TimeManager.deltaTime * timeToClean;
			yield return null;
		}

		// On vient de lui enlever du pollen, il ne va pas éternuer
		goingToSneeze = false;
	}
}
