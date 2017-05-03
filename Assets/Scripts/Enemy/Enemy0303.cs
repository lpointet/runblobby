using UnityEngine;
using System.Collections;

/* CALVIN LA CHAUVE-SOURIS
 * Se déplace rapidement de haut en bas, avec peu de variations
 * Tire 5 ondes de choc qui partent d'elle pour couvrir tout l'écran en s'élargissant progressivement
 * Le héros doit tirer sur une onde pour la briser
 * Envoie des nuées de chauve-souris vers la position actuelle du héros, depuis le plafond, en courbe
 * Si elle entre dans la lumière, crie pour éteindre la moitié des flammes volantes et la torche du héros (et les sons ?)
 * Hard+ : la nuée suit le héros pendant 2 secondes
 */

public class Enemy0303 : Enemy {

	[Header("Calvin Movement")]
	[SerializeField] private float amplitude;
	[SerializeField] private float waveFrequence;
	private float timeMouvement;
	[SerializeField] private GameObject stunEffect;
	private bool stunned = false;
	private FlashLight flashlight; // Référence au script de la lumière
	private Material lightMaterial; // Référence au Material de la lumière (pour connaître la taille du trou)

	[Header("Atk 1 : Shock Wave")]
	[SerializeField] private float shockWaveAttackDelay;
	[SerializeField] private ShockWave shockWave;
	private int animationToWave = 3;	// Nombre de fois à invoquer l'animation avant de souffler
	private int currentWave = 3;

	[Header("Atk 2 : Bat Swarm")]
	[SerializeField] private GameObject batSwarm;
	private Transform batTransform;
	private ParticleSystem batParticle;
	private ParticleSystem.MainModule batMain;
	private ParticleSystem.EmissionModule batEmission;
	private ParticleSystem.ShapeModule batShape;
	[SerializeField] private float batSwarmAttackDelay;
	private Transform playerTransform;
	private float followingSpeed;
	[SerializeField] private float normalFollowingSpeed;
	[SerializeField] private float hardFollowingSpeed;
	[SerializeField] private float hellFollowingSpeed;

	protected override void Awake () {
		base.Awake();

		startPosition [1] = Camera.main.orthographicSize - CameraManager.cameraManager.yOffset / 2.0f; // On le place au milieu de l'écran
		popPosition [1] = startPosition [1];

		playerTransform = LevelManager.player.transform;
		batTransform = batSwarm.transform;
		batParticle = batSwarm.GetComponent<ParticleSystem> ();
		batMain = batParticle.main;
		batEmission = batParticle.emission;
		batShape = batParticle.shape;
		batSwarm.SetActive (false);

		if (LevelManager.levelManager.GetCurrentDifficulty () > 0) {
			flashlight = GameObject.FindObjectOfType<FlashLight> ();
			lightMaterial = flashlight.GetComponent<Renderer> ().sharedMaterial;
		}
	}

	protected override void NormalLoad () {
		followingSpeed = normalFollowingSpeed;
	}

	protected override void HardLoad () {
		followingSpeed = hardFollowingSpeed;
	}

	protected override void HellLoad () {
		followingSpeed = hellFollowingSpeed;
	}

	protected override void Update () {
		if (IsDead () || LevelManager.player.IsDead () || TimeManager.paused)
			return;

		base.Update();

		// Une fois que le "chargement" du boss est terminé, il commence ses oscillations
		if (!popEnemy) {
			myTransform.position = new Vector2 (startPosition[0], startPosition[1] + amplitude * Mathf.Sin (waveFrequence * timeMouvement));

			// En plus des oscillations, il peut se fait étourdir s'il entre dans la lumière du joueur et réduire violemment la lumière
			if (!stunned 
				&& LevelManager.levelManager.GetCurrentDifficulty () > 0 
				&& Mathf.Abs (playerTransform.position.y - myTransform.position.y) < 0.1f 
				&& lightMaterial.GetFloat ("_Cutoff") > 0.9f) {
				StartCoroutine (Stunned (1.0f));
				flashlight.Darkening (0.5f);
			}

			timeMouvement += TimeManager.deltaTime;
		}
	}

	protected override void ChooseAttack (int numberAttack) {
		switch (numberAttack) {
		case 1:
			timeToFire += shockWaveAttackDelay;
			// Animation de la chauve-souris qui gonfle
			myAnim.SetBool("waving", true);
			// L'appel de la fonction d'attaque "ShockWave" se fait dans l'Animator
			//ShockWave ();
			break;
		case 2:
			timeToFire += batSwarmAttackDelay;
			StartCoroutine (BatSwarm ());
			break;
		}
	}

	// STUNNED
	private IEnumerator Stunned (float stunTime) {
		float currentTime = 0;

		timeToFire += stunTime; // On allonge la durée avant de retirer d'autant

		stunned = true;
		// Activation de l'effet de "stun" visuel
		GameObject stun = PoolingManager.current.Spawn (stunEffect.name);

		if (stun != null) {
			stun.transform.position = myTransform.position + Vector3.left * 0.125f;
			stun.transform.rotation = Quaternion.identity;
			stun.transform.SetParent (myTransform, true);

			stun.gameObject.SetActive (true);
		}

		while (currentTime < stunTime) {
			timeMouvement -= TimeManager.deltaTime;
			currentTime += TimeManager.deltaTime;
			yield return null;
		}

		stunned = false;
		if (stun != null) {
			stun.transform.parent = PoolingManager.pooledObjectParent;
			stun.gameObject.SetActive (false);
		}
	}

	// ATTAQUE 1 : SHOCK WAVE
	private void ShockWave () {
		// On laisse "animationToWave" fois l'animation se jouer avant de se lancer
		if (currentWave > 0) {
			currentWave--;
			return;
		} else
			currentWave = animationToWave;
		
		float shockWaveAngle;
		float higherPointAngle;
		float lowerPointAngle;
		int shockWaveNumberShot;

		higherPointAngle = Mathf.Rad2Deg * Mathf.Atan2 (CameraManager.cameraUpPosition - myTransform.position.y, myTransform.position.x); // On considère que le héros est toujours en 0
		lowerPointAngle = Mathf.Rad2Deg * Mathf.Atan2 (myTransform.position.y - CameraManager.cameraDownPosition, myTransform.position.x);
		shockWaveAngle = lowerPointAngle + higherPointAngle;

		shockWaveNumberShot = Mathf.CeilToInt (shockWaveAngle / 12.0f); // Une onde tous les 12 degrés

		for (int i = 0; i < shockWaveNumberShot; i++) {
			SingleShockWave (Quaternion.Euler (0, 0, i * shockWaveAngle / (float)(shockWaveNumberShot - 1) - higherPointAngle));
		}

		myAnim.SetBool("waving", false);
	}

	private void SingleShockWave (Quaternion waveRotation) {
		GameObject obj = PoolingManager.current.Spawn (shockWave.name);

		if (obj != null) {
			obj.transform.position = myTransform.position + Vector3.left * 1.25f;
			obj.transform.rotation = waveRotation;

			obj.SetActive (true);
		}
	}

	// ATTAQUE 2 : BAT SWARM
	private IEnumerator BatSwarm () {
		float currentTime = 0;
		float yPosition = playerTransform.position.y;

		// Animation
		myAnim.SetBool ("swarming", true);
		yield return new WaitForSecondsRealtime (1.0f); // On laisse le temps à l'animation de se présenter pour que le joueur puisse se "cacher"
		timeToFire += 1.0f;

		// Changement du vampirisme du boss
		int initialVampirisme = vampirisme;
		vampirisme = 100;

		// Apparition des chauves-souris
		batSwarm.SetActive (true);
		batSwarm.transform.SetParent (null); // Pour qu'il ne suive pas le mouvement du boss
		batSwarm.transform.position = new Vector2 (CameraManager.cameraRightPosition, yPosition);
		// Taille (un quart de l'écran en hauteur)
		batShape.radius = Camera.main.orthographicSize * 0.25f;
		// Vitesse (2 sec pour passer l'écran)
		batMain.startSpeed = new ParticleSystem.MinMaxCurve (Camera.main.orthographicSize * Camera.main.aspect / batMain.duration * 2.0f);
		// Emission
		batEmission.enabled = true;

		while (currentTime < batSwarmAttackDelay) {
			// Suivi du joueur
			// On ne bouge que si le swarm est suffisament loin du joueur (pour éviter des zigzags)
			if (Mathf.Abs (playerTransform.position.y - batTransform.position.y) > 0.1f)
				batTransform.Translate (Vector2.right * TimeManager.deltaTime * followingSpeed * Mathf.Sign (playerTransform.position.y - batTransform.position.y));

			// Arrêt de la production de chauves-souris à 2 sec avant la fin
			if (currentTime > batSwarmAttackDelay - 2.0f)
				batEmission.enabled = false;

			currentTime += TimeManager.deltaTime;
			yield return null;
		}

		myAnim.SetBool ("swarming", false);

		vampirisme = initialVampirisme;

		batSwarm.transform.SetParent (myTransform);
		batSwarm.SetActive (false);
	}

	// A la mort, la chauve-souris se fait percer par un trou de lumière
	protected override void Despawn () {
		if (batSwarm.activeInHierarchy) {
			batSwarm.transform.SetParent (myTransform);
			batSwarm.SetActive (false);
		}
		// On arrête tout
		StopAllCoroutines ();

		Die ();

		myAnim.SetTrigger ("dead");
	}
}