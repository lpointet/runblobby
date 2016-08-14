using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class PlayerController : Character {

	private Rigidbody2D myRb;
	private Animator myAnim;
	private PlayerSoundEffect myAudio;
	private Weapon weapon;

	// Calculé à partir des formules de portée et de hauteur max en partant des conditions initiales
	// Permet de conserver une hauteur et une distance constante des sauts pour toutes les vitesses horizontales
	private const float maxJumpHeight = 3;
	private const float jumpDistance = 6;
	private const float constJump = 4 * maxJumpHeight / jumpDistance;
	private const float constGravity = 0.5f / maxJumpHeight / 10f;

	/**
	 * Player Stats
	 */
	[Header("Stats joueur")]
	[SerializeField] private int _speedRatio;
	[SerializeField] private float _initialMoveSpeed;
	[SerializeField] private int _maxDoubleJump;
	/* End of Stats */
	
	// GUI
	private float previousHP; // Permet de faire un changement dégressif des HP
	private float delayLerpHP = 1f;
	private float timeLerpHP;
	private float lerpingHP;

	private bool activateTouch = true; // Permet de bloquer ou non l'interaction de l'écran pour le joueur

	private bool grounded;
	[SerializeField] private Transform groundCheck;
	[SerializeField] private float groundCheckRadius;
	[SerializeField] private LayerMask layerGround;

	//* Pickup
    private List<Collider2D> pickups = new List<Collider2D>();
    private LastWishPickup lastWish = null;
    // Attract Coins
    private int nbCoins = 0;                                // Nombre de pièces à ramasser
    private Collider2D[] coins = new Collider2D[40];        // Liste des pièces existantes
    private Vector3 direction; 	 							// Vecteur entre le joueur et une pièce
	// Fly
	private bool zeroGravFlying = false; // Selon le type de pickup vol débloqué
	private int _canBreakByContact = 0; // Le joueur peut casser _canBreakByContact objets en volant dessus
	// Tornado
	private int _canBreakByClick = 0; // Le joueur peut casser _canBreakByClick objets en cliquant dessus
	public bool activeAttract = false; // Si TRUE, un autocoin est en cours
	// Shield
	private int _canClickOnHim = 0; // Le joueur peut cliquer _canClickOnHim fois sur lui-même
	private int _permanentShield = 0; // Le nombre de points de vie supplémentaires générés par le bouclier amélioré
	public PermanentShield rotatingShield;
	// Bonus Leaf
	private int _valueLeafBoost = 0; // Puissance globale à ajouter aux tirs
	private int _numberLeafBoost = 0; // Nombre de feuilles ramassées pendant le pickup Bonus - Nombre de tirs restants avec un bonus de dégâts
	private float _coefLeafBoost = 0.1f; // Coefficient multiplicateur de la puissance par tir
	// LastWish
	private bool _canCollectAngel = false; // Permet au joueur de cliquer sur les anges pendant la "mort" du LastWish

	// Saut
	private bool _bounced = false;
	private int currentJump = 0;
	private int initialMaxDoubleJump;

	[Header("Vol")]
	[SerializeField] private GameObject parachute;
	[SerializeField] private float flySpeedCoeff = 1.5f; // Multiplication de vitesse durant le vol
	private float flySpeedBonusLastWish = 0; // Ajout au coef de multiplication si LastWish
	[SerializeField] private float flySpeedCoeffBonusLastWish = 2.5f; // Ajout d'un coef de multiplication si LastWish actif en ultime
	private float initialGravityScale;
	private bool isFlying = false; // Pendant le vol
	private bool wasFlying = false; // A la fin d'un vol avant l'atterrissage
	private bool wantToFly = false; // Quand le joueur touche l'écran pour sauter
	private Vector2 flyingTouchPosition;
	private int flyingTouchId = -1;

	private float yPosAirDeath;
	private float yVariableAirDeath = 0f;

    /**
	 * Getters & Setters
	 */
	public int speedRatio {
		get { return _speedRatio; }
		set { _speedRatio = value; }
	}
	public float initialMoveSpeed {
		get { return _initialMoveSpeed; }
		set { _initialMoveSpeed = value; }
	}
	public int maxDoubleJump {
		get { return _maxDoubleJump; }
		set { _maxDoubleJump = value; }
	}
	public bool bounced {
		get { return _bounced; }
		set { _bounced = value; }
	}

	public bool canCollectAngel {
		get { return _canCollectAngel; }
		set { _canCollectAngel = value; }
	}
	public int canBreakByContact {
		get { return _canBreakByContact; }
		set { _canBreakByContact = value; }
	}
	public int canBreakByClick {
		get { return _canBreakByClick; }
		set { _canBreakByClick = value; }
	}
	public int canClickOnHim {
		get { return _canClickOnHim; }
		set { _canClickOnHim = value; }
	}
	public int permanentShield {
		get { return _permanentShield; }
		set { _permanentShield = value; }
	}
	public int valueLeafBoost {
		get { return _valueLeafBoost; }
		set { _valueLeafBoost = value; }
	}
	public int numberLeafBoost {
		get { return _numberLeafBoost; }
		set { _numberLeafBoost = value; }
	}
	public float coefLeafBoost {
		get { return _coefLeafBoost; }
		set { _coefLeafBoost = value; }
	}

	public float RatioSpeed () {
		return moveSpeed / (float)initialMoveSpeed;
	}

	public void EquipWeapon( Weapon value ) {
		weapon = value;
		// TODO ajouter les valeurs de dégâts de l'arme (eg: critic = _critic + weaponCrit)
		// TODO passer la fonction dans Character.cs, avec les autres pièces d'équipements
	}

	public Weapon GetWeapon () {
		return weapon;
	}

	public void SetFireAbility( bool able ) {
		if( null != weapon ) {
			weapon.gameObject.SetActive( able );
			UIManager.uiManager.ToggleAmmoGUI (able);
		}
	}

	public bool GetFireAbility() {
		bool active = false;

		if( null != weapon ) {
			active = weapon.gameObject.activeSelf;
		}

		return active;
	}

    public bool HasLastWish() {
        return lastWish != null;
    }

	public LastWishPickup GetLastWish() {
		return lastWish;
	}

    public void SetLastWish( LastWishPickup value ) {
		if( value ) {
			lastWish = value;
		}
		else {
        	lastWish = null;
		}
    }

	public bool IsGrounded() {
		if (Mathf.Abs (myRb.velocity.y) > 0.05f)
			return false;
		else
			return grounded;
	}

	public bool IsFlying() {
		return isFlying;
	}

	public void SetZeroGravFlying(bool value) {
		zeroGravFlying = value;
	}

	public bool IsZeroGravFlying() {
		return zeroGravFlying;
	}

	public bool CanShieldAttract() {
		if (GameData.gameData.playerData.talent.shieldSkill > 0 && HasTypePickup (typeof(InviciblePickup)))
			return true;
		return false;
	}

	public void AddPickup( Collider2D pickup ) {
		pickups.Add( pickup );
	}

	public bool HasPickup( Collider2D pickup ) {
		return pickups.Contains( pickup );
	}

	public void RemovePickup( Collider2D pickup ) {
		pickups.Remove( pickup );
	}

	public Pickup HasTypePickup( System.Type type ) {
		Pickup[] pickups = GetComponentsInChildren<Pickup>();

		foreach( Pickup pickup in pickups ) {
			if( pickup.GetType() == type && pickup.transform.parent == myTransform ) {
				return pickup;
			}
		}

		return null;
	}

	public void SwitchTouch (bool active) {
		activateTouch = active;
	}
    /* End of Getters & Setters */

	protected override void Awake() {
        base.Awake();

		myRb = GetComponent<Rigidbody2D> ();
		myAnim = GetComponent<Animator> ();
		myAudio = GetComponent<PlayerSoundEffect> ();

		EquipWeapon (myTransform.FindChild ("Weapon").GetComponent<Weapon> ());
        initialMaxDoubleJump = maxDoubleJump;
    }
	
	protected override void Init() {
		// On ajoute les points des talents
		AddTalent();

		base.Init();

		moveSpeed = initialMoveSpeed * speedRatio / 100f;
		flySpeedCoeff += GameData.gameData.playerData.talent.flightDef * GameData.gameData.playerData.talent.flightDefPointValue;
		lerpingHP = healthPoint;
		mySprite.sharedMaterial.SetFloat ("_HueShift", 0);

		isFlying = false;
        wasFlying = false;
		if (GameData.gameData.playerData.talent.flightSkill > 0)
			SetZeroGravFlying (true);

		Mediator.current.Subscribe<TouchLeft> (PlayerActionLeft);
		Mediator.current.Subscribe<TouchRight> (PlayerActionRight);
		Mediator.current.Subscribe<EndTouch> (PlayerEndAction);
    }

	protected virtual void AddTalent() {
		Talent talent = GameData.gameData.playerData.talent;

		// Character.cs
		healthPointMax += Mathf.RoundToInt(talent.healthPoint * talent.healthPointPointValue);
		defense += Mathf.RoundToInt(talent.defense * talent.defensePointValue);
		dodge += Mathf.RoundToInt(talent.dodge * talent.dodgePointValue);
		sendBack += Mathf.RoundToInt(talent.sendBack * talent.sendBackPointValue);
		reflection += Mathf.RoundToInt(talent.reflection * talent.reflectionPointValue);
		invulnerabilityTime += talent.invulnerabilityTime * talent.invulnerabilityTime;
		vampirisme += Mathf.RoundToInt(talent.vampirisme * talent.vampirismePointValue);
		regeneration += Mathf.RoundToInt(talent.regeneration * talent.regenerationPointValue);
		attack += Mathf.RoundToInt(talent.attack * talent.attackPointValue);
		attackDelay += Mathf.RoundToInt(talent.attackDelay * talent.attackDelayPointValue);
		attackSpeed += Mathf.RoundToInt(talent.attackSpeed * talent.attackSpeedPointValue);
		criticalHit += Mathf.RoundToInt(talent.criticalHit * talent.criticalHitPointValue);
		criticalPower += Mathf.RoundToInt(talent.criticalPower * talent.criticalPowerPointValue);
		sharp += Mathf.RoundToInt(talent.sharp * talent.sharpPointValue);
		machineGun += Mathf.RoundToInt(talent.machineGun * talent.machineGunPointValue);
		shotDouble += Mathf.RoundToInt(talent.shotDouble * talent.shotDoublePointValue);
		shotWidth += Mathf.RoundToInt(talent.shotWidth * talent.shotWidthPointValue);
		shotRemote += Mathf.RoundToInt(talent.shotRemote * talent.shotRemotePointValue);

		// PlayerController.cs
		speedRatio += Mathf.RoundToInt(talent.speedBonus * talent.speedBonusPointValue);
		initialMaxDoubleJump += Mathf.RoundToInt(talent.tornadoSkill * talent.tornadoSkillPointValue);
		maxDoubleJump = initialMaxDoubleJump;
	}
	
	void FixedUpdate() {
		// Assure qu'on soit au sol lorsqu'on est en contact
		grounded = Physics2D.OverlapCircle (groundCheck.position, groundCheckRadius, layerGround);
		myAnim.SetBool ("grounded", grounded);
		myAnim.SetFloat ("verticalSpeed", myRb.velocity.y);
		// Ajuster la vitesse d'animation du héros en fonction de sa vitesse de déplacement
		myAnim.SetFloat("moveSpeed", RatioSpeed());
	}
	
	protected override void Update () {
		// On ne commence pas avant le début... hé oué !
		if (!LevelManager.levelManager.IsLevelStarted ()) {
			wasFlying = true; // Permet de "ranger" le parachute comme si on volait à la fin du "StartLevel"
			return;
		}

		base.Update();

		// Vol sur place du fantôme pendant la mort en l'air
		if (IsDead () && !HasLastWish() && !IsGrounded () && myTransform.position.y > LevelManager.levelManager.GetHeightStartBlock() - 3) {
			yVariableAirDeath += TimeManager.deltaTime;
			myTransform.position = new Vector2 (myTransform.position.x, yPosAirDeath + 0.2f * Mathf.Sin (yVariableAirDeath));
			return;
		}

        // Empêcher que des choses se passent durant la pause ou la mort
		if (TimeManager.paused || IsDead ())
            return;

		// Rapprocher le joueur douuuucement si on est pas en x = 0
		if (Mathf.Abs(myTransform.position.x) > 0.05f)
			myTransform.Translate (Mathf.Sign(myTransform.position.x) * Vector3.left * 0.005f);

		// Assure qu'on puisse faire plusieurs sauts à partir du moment où on est au sol
		if (IsGrounded ())
			currentJump = 0;

		// Ajustement du saut et gravité en fonction de la vitesse
		if (!IsFlying() && !wasFlying) {
			jumpHeight = moveSpeed * constJump;
			myRb.gravityScale = jumpHeight * jumpHeight * constGravity;
		}

		// Action de voler
		if (IsFlying() && wantToFly) {
			float verticalCoef = 1.5f;
			flyingTouchPosition = flyingTouchId == TouchManager.current.leftTouchId ? TouchManager.current.leftTouchPosition : TouchManager.current.rightTouchPosition;

			// Permet de suivre le "doigt" du joueur quand il vole en zéro gravité
			if (IsZeroGravFlying ()) {
				float cameraCursorY = Camera.main.ScreenToWorldPoint (flyingTouchPosition).y;

				// On ne bouge que si le curseur est suffisament loin du joueur (pour éviter des zigzags)
				if (Mathf.Abs (cameraCursorY - myTransform.position.y) > 0.1f)
					myRb.velocity = new Vector2 (0, Mathf.Sign ((cameraCursorY - myTransform.position.y)) * verticalCoef);
				else
					myRb.velocity = Vector2.zero;
			// En vol normal, tant qu'on appuie, le joueur "monte"
			} else {
				myRb.velocity = new Vector2 (0, verticalCoef);
			}
		// Si on n'appuie pas, on ne bouge pas
		} else if (IsFlying() && IsZeroGravFlying ()) {
			myRb.velocity = Vector2.zero;
		}
			
        // Appelé à la fin d'un vol si en l'air et jusqu'à l'atterrisage
		if(!IsGrounded () && wasFlying)
		{
            RaycastHit2D hit;
            CloudBlock cloudBlock;

			hit = Physics2D.Raycast(myTransform.position, new Vector2(2, -4), 2, layerGround);

            if (hit.collider != null)
            {
                Collider2D[] colliderHits = new Collider2D[10];
                int nbCollider;
                // Si on touche quelque chose, on allume les 10 cases autour si ce sont des nuages
                nbCollider = Physics2D.OverlapAreaNonAlloc(new Vector2(hit.point.x - 0.5f, hit.point.y - 0.4f), new Vector2(hit.point.x + 8.5f, hit.point.y + 0.4f), colliderHits, layerGround);
                for (int j = 0; j < nbCollider; j++)
                {
                    cloudBlock = colliderHits[j].GetComponent<CloudBlock>();
					if (cloudBlock != null)
						cloudBlock.ActiverNuage (true);
                }

				ActiveParachute(false);

                // On réinitialise pour ne plus afficher les éventuels nuages
				wasFlying = false;
            }
        }
	}
	
	void OnGUI() {
		// Rouge = 230 ou -140 (on se laisse une marge de 5 pour approcher davantage de la couleur, vu qu'on l'atteint à la mort seulement)
		if (lerpingHP != healthPoint) {
			timeLerpHP += TimeManager.deltaTime / delayLerpHP;
			lerpingHP = Mathf.Lerp (previousHP, healthPoint, timeLerpHP);
			// sharedMaterial pour que les boules changent de couleur aussi
			if (!IsDead ())
				mySprite.sharedMaterial.SetFloat ("_HueShift", _StaticFunction.MappingScale (lerpingHP, 0, healthPointMax, 230, 0));

		} else {
			previousHP = healthPoint;
		}
	}

	/* Lorsqu'on appuie à GAUCHE de l'écran, le joueur peut :
	 * - si un ennemi est là		SAUTER
	 * - si aucun ennemi n'est là	SAUTER | VOLER
	 */
	private void PlayerActionLeft (TouchLeft touch) {
		if (!activateTouch)
			return;
		
		if (!IsFlying ())
			JumpController ();
		else {
			if (wantToFly)
				return;
			
			wantToFly = true;

			flyingTouchId = touch.leftId;
			flyingTouchPosition = touch.leftTouchPosition;
		}
	}

	/* Lorsqu'on appuie à DROITE de l'écran, le joueur peut :
	 * - si un ennemi est là		TIRER
	 * - si aucun ennemi n'est là	SAUTER | VOLER
	 */
	private void PlayerActionRight (TouchRight touch) {
		if (!activateTouch)
			return;

		// Si un ennemi est présent, on ne peut sauter qu'à gauche de l'écran
		if (LevelManager.levelManager.GetEnemyEnCours () != null)
			return;
		
		if (!IsFlying ())
			JumpController ();
		else {
			if (wantToFly)
				return;

			wantToFly = true;

			flyingTouchId = touch.rightId;
			flyingTouchPosition = touch.rightTouchPosition;
		}
	}

	/* Quand le contact avec un doigt est rompu */
	private void PlayerEndAction (EndTouch touch) {
		// Si c'est le doigt qui servait à voler, on arrête le vol
		if (touch.fingerId == flyingTouchId) {
			wantToFly = false;
		}
	}

	private void JumpController() {
		// On ne saute pas durant la pause, la mort, ou la scène de fin
		if (TimeManager.paused || IsDead () || LevelManager.IsEndingScene ())
			return;

		if (IsGrounded () || bounced) {
			Jump ();
			bounced = false;
		} else if (!IsGrounded () && currentJump < maxDoubleJump) {
			Jump ();
			currentJump++;
		}
	}

	public void Jump() {
		// Affichage d'un effet de "nuage" à l'endroit du saut s'il est effectué en l'air
		if (!IsGrounded () && !IsFlying ()) {
			GameObject dust = PoolingManager.current.Spawn("AerialDust");

			if (dust != null) {
				dust.transform.position = myTransform.position;
				dust.transform.rotation = Quaternion.identity;

				dust.gameObject.SetActive (true);
			}
		}

		myRb.velocity = new Vector2(0, jumpHeight);
		myAudio.JumpSound ();
    }
		
	public override void OnKill() {
		// On ne peut plus tirer...
		SetFireAbility( false );

		isFlying = false;
		myAnim.SetBool ("flying", isFlying);

		// Si on est en dessous du bas de l'écran (3 blocs hauteur de base)
		if (myTransform.position.y < -3f + LevelManager.levelManager.GetHeightStartBlock()) {
			myAnim.SetTrigger ("dead_fall");
			myAudio.FallDeathSound ();
		}
		// Si on est au dessus de l'écran (donc mort suite LastWish)
		else if (myTransform.position.y > Camera.main.orthographicSize) {
			// On ne fait rien pour l'instant (placeholder)
			myRb.isKinematic = true;
			yPosAirDeath = myTransform.position.y;
		}
		else {
			if (!IsGrounded ()) { // Faire flotter le fantôme si on est en l'air
				myRb.isKinematic = true;
				yPosAirDeath = myTransform.position.y;
				myAudio.AirDeathSound ();
			} else
				myAudio.DeathSound ();
			
			myAnim.SetTrigger ("dead");
		}

		// STAT : on ajoute une mort à chaque... mort !
		GameData.gameData.playerData.numberOfDeath++;

		_StaticFunction.Save ();

		StartCoroutine (WaitForDeadAnim (myAnim));
	}

	private IEnumerator WaitForDeadAnim(Animator animation) {
		// On attend que l'animation de mort (quelle qu'elle soit) se termine
		do {
			yield return null;
		} while (animation.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f);

		myRb.Sleep();
		UIManager.uiManager.ToggleEndMenu (true);
	}

	public void OnVictory() {
		// On ne peut plus tirer...
		SetFireAbility( false );
		moveSpeed = 0;

		_StaticFunction.Save ();
	}

	public void ActiveParachute(bool active) {
		parachute.SetActive (active);
		myAnim.SetTrigger ("parachute");
		currentJump = 10000; // On empêche le joueur de sauter
	}

    public void Fly() {
		// On note la gravité actuelle s'il ne vole pas déjà
		if (!IsFlying ())
			initialGravityScale = myRb.gravityScale;

        // Abaisser la gravité et la hauteur du saut
		if (IsZeroGravFlying ())
			myRb.gravityScale = 0;
		else
			myRb.gravityScale = 0.1f;
		
		jumpHeight = 0;

		// Augmenter la vitesse si pas déjà en vol
		if (!IsFlying ()) {
			// Prise en compte du talent du Lastwish s'il est possédé
			if (HasLastWish ()) {
				flySpeedBonusLastWish = flySpeedCoeff * GameData.gameData.playerData.talent.lastWishDef * GameData.gameData.playerData.talent.lastWishDefPointValue / 100f;

				// Ajout d'un bonus si le Lastwish est en ultimate et déclenché
				if (GameData.gameData.playerData.talent.lastWishSkill > 0 && GetLastWish ().IsLaunched ()) {
					flySpeedBonusLastWish += flySpeedCoeffBonusLastWish;
				}
			}

			StartCoroutine (ChangeSpeed (moveSpeed * (flySpeedCoeff + flySpeedBonusLastWish)));
		}

		isFlying = true;
		myAnim.SetBool( "flying", isFlying ); // Permet d'annuler le parachute une fois au sol
		myAnim.SetTrigger ("parachute"); // Animation de "parachute" pendant le vol

		canBreakByContact = Mathf.RoundToInt (GameData.gameData.playerData.talent.flightAtk * GameData.gameData.playerData.talent.flightAtkPointValue);
    }

	public void Land() {
		isFlying = false;

        // Remettre les paramètres initiaux
		maxDoubleJump = initialMaxDoubleJump;
		StartCoroutine (ChangeSpeed (moveSpeed / (flySpeedCoeff + flySpeedBonusLastWish)));

		// On fait atterrir le joueur avec le parachute
		if (!IsGrounded () && !HasTypePickup (typeof(FlyPickup))) {
			// On signale au joueur qu'il était en train de voler, pour faire apparaître des nuages s'il tombe dans un trou
			wasFlying = true;

			ActiveParachute (true);
			// Rétablir une gravité "cohérente" avec un parachute
			myRb.gravityScale = initialGravityScale / 3.5f;
		}

		myAnim.SetBool( "flying", isFlying );

		canBreakByContact = 0;
    }

	private IEnumerator ChangeSpeed(float newSpeed, float delay = 1) {
		float oldSpeed = moveSpeed;
		float acceleration = 0;

		while (moveSpeed != newSpeed) {
			acceleration += TimeManager.deltaTime / delay;
			moveSpeed = Mathf.Lerp (oldSpeed, newSpeed, acceleration);

			if (moveSpeed >= newSpeed)
				moveSpeed = newSpeed;

			yield return null;
		}
	}

	// Fonction ne servant qu'à lancer la coroutine ici, car les pickups disparaissent
	public void AttractCoins( float radius, LayerMask layerCoins, float attractLength ) {
		StartCoroutine (CoroutineAttractCoins (radius, layerCoins, attractLength));
	}

	public IEnumerator CoroutineAttractCoins( float radius, LayerMask layerCoins, float attractLength ) {
		// On n'attire les pièces que si :
		// - le pickup attract est actif (activeAttract = true)
		// - on demande à l'activer une seule fois (dans le cas du shield special par exemple)
		// - il reste des pièces déjà attirées (qu'on traite après les deux premiers cas)
		do {
			if (!TimeManager.paused && !IsDead ()) {
				// On met à jour la liste des pièces
				nbCoins = Physics2D.OverlapCircleNonAlloc (myTransform.position, radius, coins, layerCoins);

				for (int i = 0; i < nbCoins; i++) {
					// On ignore les pièces de la liste si elles sont hors champ
					if (coins [i].transform.position.x > myTransform.position.x + CameraManager.cameraEndPosition ||
						coins [i].transform.position.x < myTransform.position.x + CameraManager.cameraStartPosition) {
						continue;
					}

					// Vérifier que le joueur n'a pas déjà pris cette pièce
					if (LevelManager.player.HasPickup (coins [i])) {
						continue;
					}

					// Le vecteur direction nous donne la droite entre la pièce et le bonus, donc le joueur
					direction = coins [i].transform.position - myTransform.position;

					// Faire venir la pièce vers le joueur
					// Vitesse inversement proportionelle à la distance, minimum 0.5
					// Fonction de la vitesse de déplacement (pour prévenir des loupés en cas de haut vitesse)
					coins [i].transform.Translate (RatioSpeed () * Mathf.Min (0.5f, 1 / direction.magnitude) * -direction.normalized);
				}
			}
			attractLength -= TimeManager.deltaTime;
			yield return null;
		} while (activeAttract || attractLength > 0);

		// Quand il n'y a plus de tornade, on récupère les pièces restantes et on reparcourt cette liste jusqu'à la vider
		List<Collider2D> coinsLeft = new List<Collider2D> (nbCoins);

		for (int i = 0; i < nbCoins; i++) {
			if (coins [i] != null) {
				coinsLeft.Add (coins [i]);
			}
		}

		while (coinsLeft.Count > 0) {
			if (!TimeManager.paused && !IsDead ()) {
				for (int i = 0; i < coinsLeft.Count; i++) {
					// On supprime les pièces de la liste si elles sont hors champ
					if (coinsLeft [i].transform.position.x > myTransform.position.x + CameraManager.cameraEndPosition ||
					   coinsLeft [i].transform.position.x < myTransform.position.x + CameraManager.cameraStartPosition) {
						coinsLeft.Remove (coinsLeft [i]);
						continue;
					}

					// On supprime les pièces prises par le joueur
					if (LevelManager.player.HasPickup (coinsLeft [i])) {
						coinsLeft.Remove (coinsLeft [i]);
						continue;
					}

					// Le vecteur direction nous donne la droite entre la pièce et le bonus, donc le joueur
					direction = coinsLeft [i].transform.position - myTransform.position;

					// Faire venir la pièce vers le joueur
					// Vitesse inversement proportionelle à la distance, minimum 0.5
					// Fonction de la vitesse de déplacement (pour prévenir des loupés en cas de haut vitesse)
					coinsLeft [i].transform.Translate (RatioSpeed () * Mathf.Min (0.5f, 1 / direction.magnitude) * -direction.normalized);
				}
			}
			yield return null;
		}
	}

	public override void Hurt(float damage, int penetration = 0, bool ignoreDefense = false, Character attacker = null) {
		// Si les "anciens" HP sont égaux aux "nouveaux" HP, on met à jour, sinon on garde l'encore plus vieille valeur
		if (previousHP == healthPoint)
			previousHP = healthPoint;
		
		timeLerpHP = 0; // On prépare la nouvelle variation de couleur

		if (!IsInvincible() && !IsDead ())
			myAudio.HurtSound ();

		// S'il a un bouclier permanent, on n'appelle pas immédiatement la fonction de dégâts classique
		if (!IsInvincible() && !IsDead () && permanentShield > 0) {
			
			float deltaDamage = Mathf.Min (damage, permanentShield); // Calcul des dégâts absorbés par le bouclier (au mieux, la valeur complète du bouclier)
			rotatingShield.HitShield (Mathf.RoundToInt (deltaDamage)); // On enlève les réserves au bouclier du joueur

			StartCoroutine (UIManager.uiManager.CombatText (myTransform, deltaDamage.ToString ("0 absorb"), LogType.special));

			// Si le bouclier est toujours là, on arrête tout
			if (permanentShield > 0)
				return;
			// Sinon, on change la valeur des dégâts avant d'appeler la fonction standard
			else {
				damage -= deltaDamage;
				if (damage <= 0) // Sauf si on ne fait plus de dégâts...
					return;
			}
		}

		base.Hurt (damage, penetration, ignoreDefense, attacker);
	}
}
