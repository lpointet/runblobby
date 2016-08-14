using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/* BOB LE FRERE DU HEROS
 * Il tire des balles vers le héros
 * Il vole de manière aléatoire
 * Il est petit
 */
public class Enemy0103 : Enemy {

	[Header("Bob Special")]
	[SerializeField] private Transform groundCheck;
	[SerializeField] private float groundCheckRadius;
	private bool grounded;

	private float ratioWeakBoss = 1 / 3f;
	private bool jumpToCatch = false; // Permet de savoir quand le boss a sauté pour rejoindre l'oiseau
	private float minHeight = 1f; // Hauteur minimale à laquelle il peut aller pendant son vol
	private float timeToSwitch = 0; // Délai avant le changement de sens
	private float switchSpeed; // Vitesse du prochain mouvement
	private int switchDirection = -1; // Direction du prochain mouvement (1 = haut, -1 = bas)

	private float previousHP; // Permet de faire un changement dégressif des HP
	private float delayLerpHP = 1f;
	private float timeLerpHP;
	private float lerpingHP;

	private PlayerSoundEffect mySpecialAudio;

	// Eléments de l'oiseau
	[SerializeField] private Transform myBirdTransform;
	private Animator myBirdAnim;
	private AudioSource myBirdAudio;
	private float birdBasePitch;

	private float birdStartPosition;
	private float offsetYToEnemy; // Décalage par rapport au héros (visuel)
	private bool catchedBird = false; // Permet de savoir quand l'oiseau a rattrapé le boss
	private bool weakBird = false; // Permet de savoir quand l'oiseau est faible = quand le boss est < ratioWeakBoss HP
	private float distanceToEnemy = 10f; // Distance de départ au boss
	private float timeToSpawn = 2f;
	// Fin éléments oiseau

	[Header("End of level")]
	[SerializeField] private RuntimeAnimatorController[] portrait;
	[SerializeField] private DialogEntry[] dialog;

	[SerializeField] private AudioClip boulderSound;

	protected override void Awake () {
		base.Awake();

		myBirdAnim = myBirdTransform.GetComponent<Animator> ();
		myBirdAudio =  myBirdTransform.GetComponent<AudioSource> ();
		mySpecialAudio = GetComponent<PlayerSoundEffect> ();
	}

	void Start() {
		jumpHeight = LevelManager.player.jumpHeight;

		catchedBird = false;
		weakBird = false;
		offsetYToEnemy = 5.4f / 16f;
		birdStartPosition = myTransform.position.x + Camera.main.orthographicSize * Camera.main.aspect;
		timeToSpawn = LevelManager.levelManager.enemySpawnDelay;
		birdBasePitch = myBirdAudio.pitch;

		myBirdAnim.SetBool("picked", false);
		myBirdAnim.SetBool("weak", false);
	}

	void FixedUpdate() {
		// Assure qu'on soit au sol lorsqu'on est en contact
		grounded = Physics2D.OverlapCircle (groundCheck.position, groundCheckRadius, layerGround);
		myAnim.SetBool ("grounded", grounded);
		myAnim.SetFloat ("verticalSpeed", myRb.velocity.y);
	}

	protected override void Update () {
		// Pour qu'il ne bouge pas si le joueur meurt
		if (LevelManager.player.IsDead ())
			myRb.velocity = Vector2.zero;

		if (IsDead () || LevelManager.player.IsDead () || TimeManager.paused || LevelManager.IsEndingScene())
			return;

		base.Update();

		/* OISEAU */
		// L'oiseau approche du boss tant qu'il n'est pas sur lui, puis on le laisse le suivre comme un child
		if (!catchedBird) {
			timeToSpawn -= TimeManager.deltaTime;
			if (distanceToEnemy > 0) { // Tant que l'oiseau n'est pas sur le joueur, on le rapproche
				distanceToEnemy = _StaticFunction.MappingScale (timeToSpawn, 2, 0, birdStartPosition, 0);
				myBirdTransform.position = new Vector2 (myTransform.position.x - distanceToEnemy, myTransform.position.y + offsetYToEnemy + Mathf.Pow(distanceToEnemy / 7f, 2));
				if (distanceToEnemy < 0) { // Pour être sûr qu'il soit en 0 (sur le joueur) à la fin, et pas plus loin
					myBirdTransform.position = new Vector2 (myTransform.position.x + 1 / 16f, myTransform.position.y + offsetYToEnemy);
					catchedBird = true;
					myBirdAnim.SetBool("picked", true);
				}
			}
			if (!jumpToCatch && timeToSpawn < 0.1f / LevelManager.player.RatioSpeed()) { // Le boss saute juste avant que l'oiseau n'arrive
				minHeight = Mathf.CeilToInt(myTransform.position.y); // On retient sa position avant le saut comme le minimum pendant le vol, arrondi au supérieur

				myRb.velocity = new Vector2(0, jumpHeight);
				mySpecialAudio.JumpSound ();

				// On le fait décoller
				myAnim.SetTrigger ("parachute");
				myRb.gravityScale = 0f;

				jumpToCatch = true;
			}
		} else if (!weakBird) { // Une fois que l'oiseau est sur lui, on agit différemment tant qu'il n'est pas affaibli
			// Adaptation de l'animation et du son de l'oiseau à la vitesse verticale du boss
			myAnim.SetFloat ("verticalSpeed", myRb.velocity.y);
			if (myRb.velocity.y > 0) {
				myBirdAudio.pitch = birdBasePitch;
			} else {
				myBirdAudio.pitch = birdBasePitch * 1.2f;
			}
		} else
			myBirdAudio.pitch = birdBasePitch * 1.2f;

		// Quand le boss n'a plus beaucoup de vie, on affaibli l'oiseau
		if (healthPoint / (float)healthPointMax <= ratioWeakBoss) {
			weakBird = true;
			myBirdAnim.SetBool("weak", true);
		}
		/* FIN OISEAU */

		// Une fois que l'oiseau a attrapé le boss, il commence ses oscillations
		if (catchedBird) {
			// Délai avant de changer de sens + vitesse du nouveau mouvement
			if (timeToSwitch <= 0) {
				//timeToSwitch = Random.Range (1f, 2.5f); Pour les autres difficultés ?
				//switchSpeed = Random.Range (0.5f, 1f);
				timeToSwitch = Random.Range (1.5f, 2.5f);
				switchSpeed = Random.Range (0.5f, 0.75f);
				switchDirection *= -1; // On change de sens à chaque fois
			}
			else
				timeToSwitch -= TimeManager.deltaTime;

			// On ajuste sa vitesse
			myRb.velocity = new Vector2 (0, switchDirection * switchSpeed);

			// On limite sa hauteur mini et maxi
			if (myTransform.position.y > maxHeight)
				myTransform.position = new Vector2 (myTransform.position.x, maxHeight);
			else if (myTransform.position.y < minHeight)
				myTransform.position = new Vector2 (myTransform.position.x, minHeight);
		}
	}

	void OnGUI() {
		// Rose = 25 (on se laisse une marge de 5 pour approcher davantage de la couleur, vu qu'on l'atteint à la mort seulement)
		if (lerpingHP != healthPoint) {
			timeLerpHP += TimeManager.deltaTime / delayLerpHP;
			lerpingHP = Mathf.Lerp (previousHP, healthPoint, timeLerpHP);
			// sharedMaterial pour que les boules changent de couleur aussi
			if (!IsDead ())
				mySprite.sharedMaterial.SetFloat ("_HueShift", _StaticFunction.MappingScale (lerpingHP, 0, healthPointMax, 25, 0));

		} else {
			previousHP = healthPoint;
		}
	}

	public override void Hurt(float damage, int penetration = 0, bool ignoreDefense = false, Character attacker = null) { // TODO reporter les modifications de Character.cs
		if (IsInvincible () || IsDead())
			return;

		// Si les "anciens" HP sont égaux aux "nouveaux" HP, on met à jour, sinon on garde l'encore plus vieille valeur
		if (previousHP == healthPoint)
			previousHP = healthPoint;

		timeLerpHP = 0; // On prépare la nouvelle variation de couleur

		healthPoint -= Mathf.RoundToInt (damage);

		if (healthPoint <= 0 && !IsDead () && !LevelManager.player.IsDead()) {
			//LevelManager.Kill (this);
			Despawn();
			LevelManager.levelManager.StartEndingScene ();
		}

		// Effet visuel de blessure
		if (!IsDead ()) {
			StartCoroutine (HurtEffect (invulnerabilityTime));
			SetInvincible (invulnerabilityTime);
		}

		if (!IsInvincible())
			mySpecialAudio.HurtSound ();
	}
		
	// A la mort, il s'enfuit mais ne "meurt" pas
	protected override void Despawn () {
		// L'oiseau s'envole
		StartCoroutine( TakeOff(myBirdTransform) );

		// Bob reprend une gravité cohérente
		myRb.gravityScale = 0.35f;

		// On fait partir les pickups du héros
		LevelManager.CleanPickup( LevelManager.player.GetLastWish() );
		// On rend le héros invincible pour éviter les missiles qui pourraient venir vers lui à ce moment
		LevelManager.player.SetInvincible (30f);
		// On coupe le son du héros
		LevelManager.player.GetComponent<PlayerSoundEffect>().enabled = false;
		// On empêche le joueur de sauter
		LevelManager.player.maxDoubleJump = 0;

		// Coroutine jusqu'à ce qu'il touche le sol
		StartCoroutine(WaitForLanding());
	}

	private IEnumerator TakeOff(Transform flyTransform) {
		float maxHeight = Camera.main.orthographicSize + CameraManager.cameraManager.yOffset;
		float flyDistance = 0f;

		while (flyTransform.position.y < maxHeight) {
			flyDistance += TimeManager.deltaTime;
			flyTransform.position = new Vector2 (flyTransform.position.x + flyDistance / 2f, flyTransform.position.y + flyDistance);
			yield return null;
		}

		myBirdAudio.Stop();
		flyTransform.gameObject.SetActive( false );
	}

	private IEnumerator WaitForLanding() {
		while (!grounded) {
			yield return null;
		}

		// Une fois qu'il a touché le sol, le dialogue peut débuter
		StartCoroutine (UIManager.uiManager.RunDialog (portrait, dialog, (isOver => {
			if (isOver) {
				DestructionOfTheWorld ();
			}
		})));
	}

	private void DestructionOfTheWorld () {
		// On fait partir l'ennemi avec un rire méphistophélique
		// TODO son rire
		StartCoroutine(WalkingBoss(3f, 3f));

		// Suppression de toutes les feuilles qui restent
		foreach (CoinPickup coin in GameObject.FindObjectsOfType<CoinPickup> ()) {
			coin.gameObject.SetActive (false);
		}

		// Désactiver la mort par chute
		CameraManager.cameraManager.fallingKiller.gameObject.SetActive(false);

		float fallingTime = 2f;
		float delayBetweenBlock = 0.15f;
		// Les blocs tombent l'un après l'autre en suivant leur index
		for (int i = 0; i < LevelManager.levelManager.GetListBlock ().Count; i++) {
			StartCoroutine (FallingBlock(LevelManager.levelManager.GetListBlock ()[i].transform, fallingTime, delayBetweenBlock * i));
		}
		// On lance le compte-à-rebours avant la victoire en fonction du nombre de blocs et du temps de chute
		Invoke("Victory", LevelManager.levelManager.GetListBlock ().Count * delayBetweenBlock * Time.timeScale + fallingTime);

		// Son des rochers
		AudioSource myAudio = GetComponent<AudioSource> ();
		myAudio.clip = boulderSound;
		myAudio.volume = 1;
		myAudio.loop = true;
		myAudio.Play ();
	}

	private IEnumerator WalkingBoss(float walkingTime, float leavingSpeed) {
		// L'ennemi se déplace vers la sortie
		while (walkingTime > 0) {
			myTransform.Translate (Vector3.right * TimeManager.deltaTime * leavingSpeed);
			walkingTime -= TimeManager.deltaTime;
			yield return null;
		}
	}

	private IEnumerator FallingBlock(Transform currentTransform, float transitionTime = 0, float delay = 0) {
		yield return new WaitForSeconds (delay * Time.timeScale);

		float timeToComplete = 0;
		float currentTranslation = 5f;
		float shakeSpeed = 75f;
		float shakeAmount = 0.1f;

		while (timeToComplete < transitionTime) {
			currentTransform.Translate (new Vector2(shakeAmount * Mathf.Sin(TimeManager.time * shakeSpeed), -1 * TimeManager.deltaTime * currentTranslation));

			timeToComplete += Time.deltaTime / Time.timeScale;
			yield return null;
		}
	}

	private void Victory() {
		LevelManager.levelManager.StopEndingScene ();
		Die ();
	}
}
