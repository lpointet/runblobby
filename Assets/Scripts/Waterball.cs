using UnityEngine;
using System.Collections;

public class Waterball : MonoBehaviour {

	[SerializeField] private Firewall fireWall;
	private ParticleSystem waterParticle;
	private ParticleSystem.MainModule waterParticleMain;
	private ParticleSystem.EmissionModule waterParticleEmission;
	private ParticleSystem.MinMaxCurve initialParticleVelocityX;
	private float initialParticleDampen;
	private ParticleSystem.MinMaxCurve initialParticleSize;

	private ParticleSystem.MinMaxGradient initialColor;
	// Utilisé dans le cadre du second boss du second niveau
	[SerializeField] private Color poisonousColor;
	[SerializeField] private Enemy poisonousEnemy;
	private float firewallBackDistance;

	private CircleCollider2D myCollider;
	private AudioSource myAudio;
	private SpriteRenderer mySprite;
	private Animator myAnim;
	private Transform myTransform;

	private Vector2 initialScale;

	[SerializeField] private AudioClip audioVapor;
	[SerializeField] private AudioClip audioPop;

	private bool alreadyHit = false;

	private float lifeTime;
	[SerializeField] private float hardLifeTime;
	[SerializeField] private float hellLifeTime;
	[SerializeField] private float arcadeLifeTime;
	private float timeToLive;

	void Awake () {
		waterParticle = GetComponentInChildren<ParticleSystem> ();
		myAudio = GetComponent<AudioSource> ();
		myCollider = GetComponent<CircleCollider2D> ();
		mySprite = GetComponent<SpriteRenderer> ();
		myAnim = GetComponent<Animator> ();
		myTransform = transform;

		waterParticleMain = waterParticle.main;
		waterParticleEmission = waterParticle.emission;
		initialParticleVelocityX = waterParticle.velocityOverLifetime.x;
		initialParticleSize = waterParticleMain.startSize;
		initialColor = waterParticleMain.startColor;
	}

	void Start () {
		Mediator.current.Subscribe<TouchWaterLevel2> (WaterHit);

		// Chargement différent selon la difficulté et le mode
		if (LevelManager.levelManager.IsStory ()) {
			switch (LevelManager.levelManager.GetCurrentDifficulty ()) {
				// Normal
			case 0:
				//break; // TODO enlever le commentaire du break
				// Hard
			case 1:
				lifeTime = hardLifeTime;
				break;
				// Hell
			case 2:
				lifeTime = hellLifeTime;
				break;
			}
		} else // Arcade
			lifeTime = arcadeLifeTime;

		timeToLive = lifeTime;

		// Ajustement de l'échelle par rapport à celle du mur de flamme
		initialScale = 0.5f * new Vector2 (1 / (Camera.main.orthographicSize * Camera.main.aspect), 1 / Camera.main.orthographicSize);
		myTransform.localScale = initialScale;
	}

	void OnEnable () {
		timeToLive = lifeTime;

		alreadyHit = false;
		myCollider.enabled = true;
		mySprite.sprite = null; // Pour éviter que l'image réapparaisse pendant l'animation "Nothing"
		waterParticleMain.startColor = initialColor;
		mySprite.color = Color.white;

		// Taille de particules
		waterParticleMain.startSize = initialParticleSize;

		// Nombre de particule
		waterParticleEmission.rateOverTime = 0;

		// Direction des particules
		ParticleSystem.VelocityOverLifetimeModule particleVelocity;
		particleVelocity = waterParticle.velocityOverLifetime;
		particleVelocity.x = initialParticleVelocityX;

		StartCoroutine( CreateWaterBall ());

		// On joue le son du pop
		if (myAudio != null) {
			myAudio.Stop ();
			myAudio.pitch = 1.0f + Random.Range (-0.25f, 0.35f);
			myAudio.clip = audioPop;
			myAudio.Play ();
		}
	}

	void Update () {
		if (TimeManager.paused)
			return;

		if (lifeTime > 0)
			timeToLive -= TimeManager.deltaTime;

		// Animation de fin de bulle
		if (timeToLive < 0.3f && !alreadyHit) {
			alreadyHit = true;
			myAnim.SetTrigger ("end");
		}

		if (timeToLive < 0)
			Despawn ();

		// Taille de la bulle
		myTransform.localScale = Vector2.Lerp (initialScale, 0.5f * initialScale, (lifeTime - timeToLive) / lifeTime);

		// Déplacement de la bulle
		myTransform.Translate (Vector3.left * TimeManager.deltaTime);

		// Empoisonnement de l'eau pendant le second boss en mode Hard ou Hell
		if (LevelManager.levelManager.GetEnemyEnCours () != null
		    && LevelManager.levelManager.GetEnemyEnCours ().GetType () == poisonousEnemy.GetType ()
			&& LevelManager.levelManager.GetCurrentDifficulty () > 0) {
			// Couleur des particules et de la boule
			waterParticleMain.startColor = new ParticleSystem.MinMaxGradient (Color.Lerp (initialColor.color, poisonousColor, (lifeTime - timeToLive) / lifeTime));
			mySprite.color = Color.Lerp (Color.white, poisonousColor, (lifeTime - timeToLive) / lifeTime);
			// Au début le mur recule, puis il avance si on clique trop tard
			firewallBackDistance = Mathf.Lerp (1, -0.5f, (lifeTime - timeToLive) / lifeTime);
		} else
			firewallBackDistance = 1;
	}

	private IEnumerator CreateWaterBall () {
		ParticleSystem.EmitParams overrideEmit = new ParticleSystem.EmitParams ();
		overrideEmit.startLifetime = 1f;
		overrideEmit.velocity = Vector3.one;
		waterParticle.Emit (overrideEmit, 20);

		yield return new WaitForSecondsRealtime (0.5f);
	}

	private void WaterHit (TouchWaterLevel2 touchObject) {
		if (touchObject.objectId != this.gameObject.GetInstanceID () || alreadyHit)
			return;

		alreadyHit = true;

		// On fait reculer le mur de flammes
		if (fireWall != null) {
			fireWall.SlowFireWall (firewallBackDistance);
		}

		// On joue le son du depop
		if (myAudio != null) {
			myAudio.Stop ();
			myAudio.pitch = 1.0f + Random.Range (-0.25f, 0.35f);
			myAudio.clip = audioVapor;
			myAudio.Play ();
		}

		myCollider.enabled = false; // Pour ne pas bloquer le joueur en l'empêchant de cliquer alors qu'il n'y a plus rien
		myAnim.SetTrigger("end"); // Animation de fin de la bulle

		StartCoroutine (DissipateParticle ());
	}

	private IEnumerator DissipateParticle () {
		float timeToDissipate = 0.75f;
		float currentTime = 0;

		// On force un taux d'émission à 20 (arbitraire)
		waterParticleEmission.rateOverTime = 20f;

		// Direction des particules vers le mur de feu
		ParticleSystem.VelocityOverLifetimeModule particleVelocity;
		particleVelocity = waterParticle.velocityOverLifetime;
		float distanceFromFire = fireWall.transform.position.x + Camera.main.orthographicSize * Camera.main.aspect - myTransform.position.x + 1f;
		particleVelocity.x = new ParticleSystem.MinMaxCurve (distanceFromFire);

		float tempSize = waterParticleMain.startSize.constant;

		while (currentTime < timeToDissipate) {
			tempSize -= initialParticleSize.constant * (TimeManager.deltaTime / timeToDissipate); // Réduction de la taille des particules

			currentTime += TimeManager.deltaTime;
			yield return null;
		}

		gameObject.SetActive (false);
	}

	private void Despawn () {
		gameObject.SetActive (false);
	}
}