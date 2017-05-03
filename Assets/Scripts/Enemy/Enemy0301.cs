using System.Collections;
using UnityEngine;

/* DANIELE THE COOKIE ARANEAE
 * Descend du plafond, soit sur le héros, soit pour lancer une attaque
 * Sur le héros, elle l'attire vers le plafond où il subira des dégâts : en sautant, on peut se libérer
 * Le seul moyen de l'éviter est de tomber dans une toile d'araignée qui le ralentit
 * Elle attaque en jetant des crachats de venin (en cloche) qui restent au sol s'ils tombent trop tôt
 * Elle lache parfois des toiles d'araignée du plafond, devant lui, ralentissant le héros s'il est pris dedans
 * Hard+ : Elle inflige des dégâts dès qu'elle touche le héros
 * Hard+ : Elle descend aussi sur les flammes pour les voler
 */
public class Enemy0301 : Enemy {

	private Rigidbody2D playerRb;
	private Animator playerAnim;
	private Transform playerTransform;

	private FixedJoint2D jawJoint; // Ancre au niveau de sa mâchoire

	[Header("Daniele Special")]
	[SerializeField] private GameObject thread;
	private Transform threadTransform;
	private float lengthThread; // Permet de connaître la longueur du fil
	[SerializeField] private float verticalSpeed; // Vitesse de base de remontée et de descente
	[SerializeField] private float ratioUpOnDown; // Multiplicateur pour la vitesse descendante

	private float minDescendingPosition;
	private float maxDescendingPosition;
	private float upPosition;

	[Header("Atk 1 : Grab Hero")]
	[SerializeField] private float grabHeroAttackDelay;
	private bool tryingToGrab; // Permet de savoir si on essaye d'attraper ou non (permet d'éviter de l'attraper au mauvais moment)
	public bool heroGrabbed { get; private set; }
	[SerializeField] private float ascentSpeed; // Vitesse de remontée AVEC le héros
	[SerializeField] private int jumpForRelease; // Nombre de sauts à effectuer pour être relaché
	private int currentJump; // Nombre de sauts effectué durant le GrabHero
	[SerializeField] private int grabDamage;
	private bool shakingHero = false; // Permet de savoir si le héros est en train d'être secoué ou non

	[Header("Atk 2 : Spit Poison")]
	[SerializeField] private PoisonBall poisonBall;
	[SerializeField] private float spitPoisonAttackDelay;
	private int numberOfBall;
	[SerializeField] private int normalNumberOfBall;
	[SerializeField] private int hardNumberOfBall;
	[SerializeField] private int hellNumberOfBall;

	[Header("Atk 3 : Drop Web")]
	[SerializeField] private Bave spiderWeb;
	[SerializeField] private float dropWebAttackDelay;

	[Header("Atk 4 : Grab Firefly")]
	private Firefly firefly;
	private Transform fireflyTransform;
	[SerializeField] private float grabFireflyAttackDelay;

	// Permet de savoir si le boss est en haut ou non
	private bool IsUp () {
		if (myTransform.position.y >= upPosition)
			return true;
		else
			return false;
	}

	protected override void Awake () {
		base.Awake();

		jawJoint = GetComponent<FixedJoint2D> ();
		playerRb = LevelManager.player.GetComponent<Rigidbody2D> ();
		playerAnim = LevelManager.player.GetComponent<Animator> ();
		playerTransform = LevelManager.player.transform;
		threadTransform = thread.transform;

		tryingToGrab = false;
		heroGrabbed = false;
		thread.transform.localScale = new Vector3 (0.1f, 0.0f, 1.0f);
	}

	void Start () {
		Mediator.current.Subscribe<TouchLevel3Boss1> (JumpingHero);

		minDescendingPosition = Camera.main.orthographicSize * Camera.main.aspect - CameraManager.cameraManager.xOffset;
		maxDescendingPosition = CameraManager.cameraRightPosition / 1.25f;
		upPosition = CameraManager.cameraUpPosition + 1.0f;

		myAnim.SetBool ("goingUp", true);
	}

	protected override void NormalLoad () {
		numberOfBall = normalNumberOfBall;
	}

	protected override void HardLoad () {
		damageToGive = 1;
		numberOfBall = hardNumberOfBall;
		firefly = GameObject.Find ("Firefly").GetComponent<Firefly> ();
		fireflyTransform = firefly.transform;
	}

	protected override void HellLoad () {
		HardLoad ();
		numberOfBall = hellNumberOfBall;
	}

	protected override void Update () {
		if (IsDead () || LevelManager.player.IsDead () || TimeManager.paused)
			return;

		base.Update();

		// Modification du material du fil
		//thread.GetComponent<MeshRenderer> ().material.mainTextureOffset = Vector2.up * Mathf.Sin (TimeManager.time);

		// Calcul de la longueur du fil
		lengthThread = CameraManager.cameraUpPosition - myTransform.position.y;
		// Gestion de la position du fil
		threadTransform.localPosition = new Vector2 (0, lengthThread / 2f);
		// Gestion de la longueur du fil (2 pixels de large)
		threadTransform.localScale = new Vector3 (0.1f, lengthThread, 1.0f);
	}

	protected override void ChooseAttack (int numberAttack) {
		// On force à récupérer la luciole si la luciole est dans le premier quart de l'écran (en Hard+)
		// TODO ça risque de tourner en boucle, à tester, prévoir un bool
		if (LevelManager.levelManager.GetCurrentDifficulty () > 0) {
			if (fireflyTransform.position.y > 0.5f
			   && fireflyTransform.position.x > 1.6f * Camera.main.orthographicSize * Camera.main.aspect - CameraManager.cameraManager.xOffset)
				numberAttack = 4;
			else if (numberAttack == 4) // Si on avait choisit 4 mais que la luciole n'est pas à la bonne place, on choisit autre chose
			numberAttack = Random.Range (1, 4);
		}
		
		switch (numberAttack) {
		case 1:
			timeToFire += grabHeroAttackDelay * 1.5f;
			StartCoroutine (GrabHero (grabHeroAttackDelay));
			break;
		case 2:
			timeToFire += spitPoisonAttackDelay;
			StartCoroutine (SpitPoison ());
			break;
		case 3:
			timeToFire += dropWebAttackDelay;
			DropWeb ();
			break;
		case 4:
			timeToFire += grabFireflyAttackDelay;
			StartCoroutine (GrabFirefly (grabFireflyAttackDelay));
			break;
		}
	}

	private IEnumerator GoingUp (float speed) {
		myAnim.SetBool ("goingUp", true);
		myAnim.SetBool ("stopped", false);

		while (myTransform.position.y < upPosition) {
			myTransform.Translate (Vector2.up * TimeManager.deltaTime * speed);

			timeToFire += TimeManager.deltaTime; // Pour mettre cette phase "hors" du temps de l'attaque
			yield return null;
		}
	}

	private IEnumerator GoingDown (float speed, float yEnding, float xMin, float xMax) {
		myAnim.SetBool ("goingUp", false);
		myAnim.SetBool ("stopped", false);

		float xPosition = Random.Range (xMin, xMax);
		myTransform.position = new Vector2 (xPosition, upPosition);

		while (myTransform.position.y > yEnding) {
			myTransform.Translate (Vector2.down * TimeManager.deltaTime * speed);

			timeToFire += TimeManager.deltaTime; // Pour mettre cette phase "hors" du temps de l'attaque
			yield return null;
		}

		myAnim.SetBool ("stopped", true);
		timeToFire += 0.5f;
		yield return new WaitForSecondsRealtime (0.5f);
		myAnim.SetBool ("stopped", false);
	}

	// ATTAQUE 1 : GRAB HERO
	private IEnumerator GrabHero (float delayBeforeAttack) {
		currentJump = 0;
		float playerXPosition = playerTransform.position.x; // Pour garder la position au début de l'attaque
		// On s'assure que le boss est en haut
		if (!IsUp ())
			yield return StartCoroutine (GoingUp (verticalSpeed));

		// On le fait apparaitre un peu, que le joueur puisse se préparer
		yield return new WaitForSecondsRealtime (delayBeforeAttack / 4f);
		yield return StartCoroutine (GoingDown (verticalSpeed * ratioUpOnDown, CameraManager.cameraUpPosition - 1.0f, playerXPosition, playerXPosition));

		// On jette une toile d'araignée pour que le joueur puisse ralentir
		DropWeb ();

		yield return new WaitForSecondsRealtime (delayBeforeAttack);
		yield return StartCoroutine (GoingUp (verticalSpeed));
		yield return new WaitForSecondsRealtime (delayBeforeAttack / 4f);

		tryingToGrab = true;

		// On le fait descendre sur le joueur (dérivé de GoingDown)
		myAnim.SetBool ("goingUp", false);
		myAnim.SetBool ("stopped", false);

		myTransform.position = new Vector2 (playerXPosition, upPosition);

		while (myTransform.position.y > playerTransform.position.y && !heroGrabbed) {
			myTransform.Translate (Vector2.down * TimeManager.deltaTime * verticalSpeed * ratioUpOnDown);

			timeToFire += TimeManager.deltaTime; // Pour mettre cette phase "hors" du temps de l'attaque
			yield return null;
		}

		// Si on touche le héros
		if (heroGrabbed) {
			// On joint le joueur en même temps que le boss
			jawJoint.connectedBody = playerRb;

			if (LevelManager.player.IsDead ())
				yield break;
			
			playerAnim.SetTrigger ("parachute");
			playerAnim.SetBool ("flying", true);

			// On le fait remonter plus lentement (dérivé de GoingUp)
			while (myTransform.position.y < upPosition) {
				myAnim.SetBool ("goingUp", true);
				// Si le héros est attrapé, il monte plus doucement
				if (heroGrabbed) {
					myTransform.Translate (Vector2.up * TimeManager.deltaTime * ascentSpeed);
					// Si le héros arrive à sa limite, il se prend des dégâts et est relâché
					if (playerTransform.position.y >= LevelManager.player.maxHeight - 1.0f)
						ReleaseHero (true);
					// Si le héros saute assez, il tombe sans prendre de dégâts
					if (currentJump >= jumpForRelease)
						ReleaseHero (false);
				} else {
					myTransform.Translate (Vector2.up * TimeManager.deltaTime * verticalSpeed);
				}

				timeToFire += TimeManager.deltaTime; // Pour mettre cette phase "hors" du temps de l'attaque
				yield return null;
			}
		} else {
			myAnim.SetBool ("stopped", true);
			timeToFire += 0.5f;
			yield return new WaitForSecondsRealtime (0.5f);
			myAnim.SetBool ("stopped", false);
			// On le fait remonter normalement s'il n'a pas le héros
			yield return StartCoroutine (GoingUp (verticalSpeed));
		}
	}

	private void JumpingHero (TouchLevel3Boss1 touch) {
		// On ne peut pas se resecouer si on se secoue déjà !
		if (!shakingHero) {
			currentJump++;

			// Secouer le héros pour montrer qu'il se passe quelque chose
			if (currentJump < jumpForRelease) {
				shakingHero = true;
				LevelManager.player.ShakePlayer (0.25f);
				Invoke ("StopShakingHero", 0.25f);
			}
			else
				LevelManager.player.StopShake ();
		}
	}

	private void StopShakingHero () {
		shakingHero = false;
	}

	private void ReleaseHero (bool withDamage) {
		LevelManager.player.ActiveParachute (true);

		jawJoint.connectedBody = null;

		LevelManager.player.wasFlying = true;
		playerAnim.SetBool ("flying", false);


		tryingToGrab = false;
		heroGrabbed = false;

		if (withDamage)
			LevelManager.player.Hurt(grabDamage, sharp);
	}

	// ATTAQUE 2 : SPIT POISON
	private IEnumerator SpitPoison () {
		// On s'assure que le boss est en haut
		if (!IsUp ())
			yield return StartCoroutine (GoingUp (verticalSpeed));

		// On la fait descendre devant le joueur
		yield return StartCoroutine (GoingDown (verticalSpeed * ratioUpOnDown, startPosition.y * Random.Range (0.75f, 1.1f), minDescendingPosition, maxDescendingPosition));

		myAnim.SetBool ("spitting", true);

		// Création de chaque boule
		for (int i = 0; i < numberOfBall; i++) {
			GameObject obj = PoolingManager.current.Spawn (poisonBall.name);

			if (obj != null) {
				obj.transform.position = new Vector2 (myTransform.position.x, myTransform.position.y - 0.75f);
				obj.transform.rotation = Quaternion.identity;
				obj.SetActive (true);

				float ballCreationTime = obj.GetComponent<PoisonBall> ().creationTime;
				// On laisse le temps à la boule de se former
				timeToFire += ballCreationTime;
				yield return new WaitForSecondsRealtime (ballCreationTime);

				// Calcul de la direction de la balle (aux pieds du héros en (0, 0)) avec 2 tirs de semonces de plus en plus proches
				Vector2 direction = Vector3.right * (LevelManager.player.moveSpeed + 2 * (numberOfBall - i - 1)) - obj.transform.position;
				obj.GetComponent<Rigidbody2D> ().velocity = direction.normalized * 5.0f;
			}
		}

		myAnim.SetBool ("spitting", false);
	}

	// ATTAQUE 3 : DROP WEB
	private void DropWeb () {
		GameObject obj = PoolingManager.current.Spawn (spiderWeb.name);

		if (obj != null) {
			float xDropPosition = Random.Range (minDescendingPosition, maxDescendingPosition);
			obj.transform.position = new Vector2 (xDropPosition, upPosition);
			obj.transform.rotation = Quaternion.identity;

			obj.SetActive (true);
			// On fait tomber la toile
			obj.GetComponent<Rigidbody2D> ().velocity = Vector2.down * 2.5f;

			StartCoroutine (CheckGroundForWeb (obj.transform));
		}
	}

	private IEnumerator CheckGroundForWeb (Transform web) {
		RaycastHit2D hit;

		// On essaye de trouver le sol juste en-dessous pour rattacher la toile
		do {
			hit = Physics2D.Raycast (web.position, Vector2.down, 0.5f, layerGround);
			yield return null;
		} while (hit.collider == null);

		web.parent = hit.collider.transform;
		web.GetComponent<Rigidbody2D> ().velocity = Vector2.zero;
	}

	// ATTAQUE 4 (HARD) : GRAB FIREFLY
	private IEnumerator GrabFirefly (float delayBeforeAttack) {
		// On s'assure que le boss est en haut
		if (!IsUp ())
			yield return StartCoroutine (GoingUp (verticalSpeed));

		// On force la position de départ à une distance fixe de la luciole
		myTransform.position = new Vector2 (firefly.transform.position.x - 5.0f, upPosition);

		// On le fait descendre sur la luciole (dérivé de GoingDown)
		myAnim.SetBool ("goingUp", false);
		myAnim.SetBool ("stopped", false);

		// Calcul de la vitesse de descente en fonction de la distance de la luciole, de sa vitesse et de la hauteur restante à parcourir
		float heightToGo;
		float timeToGo;
		float descendingSpeed;

		while (myTransform.position.y > 0.0f && firefly.isActiveAndEnabled) {
			heightToGo = myTransform.position.y - fireflyTransform.position.y;
			timeToGo = (fireflyTransform.position.x - myTransform.position.x) / firefly.GetMoveSpeed ();
			descendingSpeed = heightToGo / timeToGo;

			myTransform.Translate (Vector2.down * TimeManager.deltaTime * descendingSpeed);

			timeToFire += TimeManager.deltaTime; // Pour mettre cette phase "hors" du temps de l'attaque
			yield return null;
		}

		myAnim.SetBool ("stopped", true);
		timeToFire += 0.5f;
		yield return new WaitForSecondsRealtime (0.5f);
		myAnim.SetBool ("stopped", false);
		// On le fait remonter normalement
		yield return StartCoroutine (GoingUp (verticalSpeed));
	}

	protected override void OnTriggerEnter2D (Collider2D other) {
		// Si l'ennemi est déjà mort, il ne peut plus rien faire...
		if( IsDead() )
			return;

		if (other.name == "Heros" && tryingToGrab) { // Ne peut intervenir que lors de l'attaque "Grab Hero"
			if (damageToGive > 0)
				LevelManager.player.Hurt (damageToGive, sharp);
			heroGrabbed = true;

			// Supprime le vol si le joueur en avait un
			if (LevelManager.player.IsFlying ())
				LevelManager.player.GetComponentInChildren <FlyPickup> ().Disable ();
		}

		if (firefly != null && other.name == firefly.name) {
			firefly.Despawn ();
		}
	}

	// A la mort, l'araignée se fait "croquer" et tombe sur le sol
	protected override void Despawn () {
		// On arrête tout
		StopAllCoroutines ();

		StartCoroutine (ReachingGround ());

		Die ();

		myAnim.SetTrigger ("dead");

		thread.SetActive (false);

		jawJoint.enabled = false;
		myRb.isKinematic = false;
		myRb.gravityScale = 0.35f;
		myRb.AddForce (Vector2.up * 100);
	}

	private IEnumerator ReachingGround () {
		RaycastHit2D hit;

		// On essaye de trouver le sol juste en-dessous
		do {
			hit = Physics2D.Raycast (myTransform.position, Vector2.down, 0.5f, layerGround);
			myRb.transform.Rotate (Vector3.back, TimeManager.deltaTime * 15.0f);
			yield return null;
		} while (hit.collider == null);

		myTransform.parent = hit.transform;
		myRb.isKinematic = true;
		myRb.velocity = Vector2.zero;
		foreach (CircleCollider2D collider in GetComponentsInChildren<CircleCollider2D> ())
			collider.enabled = false;
	}
}
