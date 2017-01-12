using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// Le sol est tellement chaud que le joueur se brûle les pieds
// Lorsque le joueur a les pieds trop chauds, il saute automatiquement, ce qui lui refroidit légèrement les pieds

[RequireComponent(typeof(AudioSource))]
public class BurningGround : MonoBehaviour {

	public static BurningGround current;

	private ParticleSystem burningParticle;
	private ParticleSystem.MainModule burningMain;
	private ParticleSystem.EmissionModule burningEmission;
	private float maxParticleRate = 20f;
	private float maxParticleSize = 0.25f;

	private AudioSource myAudio;
	//private Rigidbody2D playerRb;

	// Données concernant la charge de pollen
	private float currentCharge = 0f;	// Charge actuelle
	private float maxCharge = 10f;		// Charge à partir de laquelle le joueur saute
	private float chargeSpeed = 1f;		// Gain de charge au sol pendant 1 seconde
	private float dischargeSpeed = 2f;	// Perte de charge sans toucher le sol pendant 1 seconde
	private float jumpForce = 6f;	// Force du saut

	[Header("Normal Parameters")]
	public float normalMaxCharge;
	public float normalChargeSpeed;
	public float normalDischargeSpeed;
	public float normalJumpForce;

	[Header("Hard Parameters")]
	public float hardMaxCharge;
	public float hardChargeSpeed;
	public float hardDischargeSpeed;
	public float hardJumpForce;

	[Header("Hell Parameters")]
	public float hellMaxCharge;
	public float hellChargeSpeed;
	public float hellDischargeSpeed;
	public float hellJumpForce;

	void Awake () {
		if (current == null)
			current = this;
		
		burningParticle = GetComponent<ParticleSystem> ();
		myAudio = GetComponent<AudioSource> ();

		//playerRb = LevelManager.player.GetComponent<Rigidbody2D> ();
	}

	void Start () {
		// Chargement différent selon la difficulté
		switch (LevelManager.levelManager.GetCurrentDifficulty ()) {
		// Normal
		case 0:
			/*maxCharge = normalMaxCharge;
			chargeSpeed = normalChargeSpeed;
			dischargeSpeed = normalDischargeSpeed;
			jumpForce = normalJumpForce;

			break;*/ // TODO remove comment
		// Hard
		case 1:
			maxCharge = hardMaxCharge;
			chargeSpeed = hardChargeSpeed;
			dischargeSpeed = hardDischargeSpeed;
			jumpForce = hardJumpForce;

			break;
		// Hell
		case 2:
			maxCharge = hellMaxCharge;
			chargeSpeed = hellChargeSpeed;
			dischargeSpeed = hellDischargeSpeed;
			jumpForce = hellJumpForce;

			break;
		}

		LoadBurningGround ();
	}

	private void LoadBurningGround() {
		// Positionnement au pied du joueur
		transform.position = LevelManager.player.transform.position + Vector3.down * 7/16f;

		burningMain = burningParticle.main;

		// Initialisation à 0 particule
		burningEmission = burningParticle.emission;
		burningEmission.rateOverTime = new ParticleSystem.MinMaxCurve (0);
	}

	void Update() {
		if (TimeManager.paused || LevelManager.player.IsDead () || LevelManager.IsEndingScene())
			return;

		// Positionnement au pied du joueur
		transform.position = LevelManager.player.transform.position + Vector3.down * 7/16f;

		if (LevelManager.player.IsGrounded ()) {
			currentCharge += chargeSpeed * TimeManager.deltaTime;

			if (currentCharge >= maxCharge) {
				currentCharge = maxCharge;

				// On émet un burst de particules au moment du saut forcé
				ParticleSystem.EmitParams overrideEmit = new ParticleSystem.EmitParams ();
				overrideEmit.startLifetime = 1f;
				overrideEmit.velocity = 1f * Vector3.up;
				burningParticle.Emit (overrideEmit, 25);

				// Le joueur "saute"
				ForceJumpPlayer ();
			}
		} else {
			currentCharge -= dischargeSpeed * TimeManager.deltaTime;
			if (currentCharge < 0)
				currentCharge = 0;
		}

		// Quantité de particules
		burningEmission.rateOverTime = new ParticleSystem.MinMaxCurve (Mathf.Lerp (0, maxParticleRate, currentCharge / maxCharge));

		// Taille de particules
		burningMain.startSize = Mathf.Lerp (0.1f, maxParticleSize, currentCharge / maxCharge);
	}

	private void ForceJumpPlayer() {
		//playerRb.AddForce (Vector2.up * jumpForce);
		LevelManager.player.Jump (jumpForce);

		if (myAudio != null)
			myAudio.Play ();
		
		// On diminue la quantité de chaleur sous ses pieds de moitié
		StartCoroutine(DiminishHeat(0.5f));
	}

	// Permet de lancer la Coroutine depuis une autre fonction
	public void CoroutineDiminishHeat(float heatDischarge, float timeToRefresh = 0.5f) {
		StartCoroutine(DiminishHeat(heatDischarge, timeToRefresh));
	}

	private IEnumerator DiminishHeat(float heatDischarge, float timeToRefresh = 0.5f) {
		// heatDischarge représente la valeur de chaleur qu'on enlève en taux, donc entre 0 et 1
		heatDischarge = Mathf.Clamp01 (heatDischarge);

		float currentTime = 0;

		float startingCharge = currentCharge;
		float endingCharge = currentCharge - heatDischarge * currentCharge;

		while (currentTime < timeToRefresh) {
			currentCharge = Mathf.Lerp (startingCharge, endingCharge, currentTime / timeToRefresh);

			currentTime += TimeManager.deltaTime * timeToRefresh;
			yield return null;
		}
	}

	public void FullHeat() {
		currentCharge = maxCharge;
	}
}
