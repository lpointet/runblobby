using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class PlayerController : Character {
	
	/**
	 * Player Stats
	 */
	[SerializeField] private float initialMoveSpeed;
	[SerializeField] private int maxDoubleJump;
	/* End of Stats */
	
	// GUI
	private float previousHP; // Permet de faire un changement dégressif des HP
	private float delayLerpHP = 1f;
	private float timeLerpHP;
	private float lerpingHP;
	
	private Rigidbody2D myRb;
	private Animator myAnim;
	private PlayerSoundEffect myAudio;
	private Transform weapon;
	public GameObject parachute;

    [HideInInspector] public bool bounced = false;
    public bool wasFlying = false;
	private bool grounded;
	public Transform groundCheck;
	public float groundCheckRadius;
	public LayerMask layerGround;
	
	private int currentJump = 0;
	private int initialMaxDoubleJump;

	// Concerne le saut
    private float initialGravityScale;
    private float initialJumpHeight; // TODO à supprimer ?
    
	// Calculé à partir des formules de portée et de hauteur max en partant des conditions initiales
	// Permet de conserver une hauteur et une distance constante des sauts pour toutes les vitesses horizontales
	private const float maxJumpHeight = 3;
	private const float jumpDistance = 6;
	private const float constJump = 4 * maxJumpHeight / jumpDistance;
	private const float constGravity = 0.5f / maxJumpHeight / 10f;

    private List<Collider2D> pickups = new List<Collider2D>();
    private LastWishPickup lastWish = null;

    // Attract Coins
    private int nbCoins = 0;                                // Nombre de pièces à ramasser
    private Collider2D[] coins = new Collider2D[40];        // Liste des pièces existantes
    private Vector3 direction; 	 							// Vecteur entre le joueur et une pièce

	// Concerne le vol
	private float flySpeedCoeff = 2f;
	private bool isFlying = false;
	private bool wantToFly = false;
	private Vector2 flyingTouchPosition;
	private int flyingTouchId = -1;
	private bool zeroGravFlying = false;
	private float yPosAirDeath;
	private float yVariableAirDeath = 0f;

    /**
	 * Getters & Setters
	 */
    public float GetInitialMoveSpeed() {
		return initialMoveSpeed;
	}
	
	public void SetInitialMoveSpeed( float value ) {
		initialMoveSpeed = value;
	}

	public float GetRatioSpeed () {
		return GetMoveSpeed () / (float)GetInitialMoveSpeed ();
	}
	
	public Transform GetWeapon() {
		return weapon;
	}
	
	public void SetWeapon( Transform value ) {
		weapon = value;
	}

	public int GetMaxDoubleJump() {
		return maxDoubleJump;
	}

	public void SetMaxDoubleJump( int value ) {
		maxDoubleJump = value;
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

	private bool WantToFly() {
		return wantToFly;
	}

	public void SetZeroGravFlying(bool value) {
		zeroGravFlying = value;
	}

	public bool IsZeroGravFlying() {
		return zeroGravFlying;
	}

	public void SetFireAbility( bool able ) {
		if( null != weapon ) {
			weapon.gameObject.SetActive( able );
		}
	}

	public bool GetFireAbility() {
		bool active = false;

		if( null != weapon ) {
			active = weapon.gameObject.activeSelf;
		}

		return active;
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
			if( pickup.GetType() == type ) {
				return pickup;
			}
		}

		return null;
	}
    /* End of Getters & Setters */

    protected override void Awake() {
        base.Awake();

		myRb = GetComponent<Rigidbody2D> ();
		myAnim = GetComponent<Animator> ();
		myAudio = GetComponent<PlayerSoundEffect> ();

		SetWeapon( myTransform.FindChild( "Weapon" ) );
//        initialGravityScale = myRb.gravityScale;
//        initialJumpHeight = GetJumpHeight();
        initialMaxDoubleJump = GetMaxDoubleJump();
    }
	
	protected override void Init() {
		base.Init();

		SetMoveSpeed( GetInitialMoveSpeed() );
		lerpingHP = GetHealthPoint ();
		mySprite.sharedMaterial.SetFloat ("_HueShift", 0);
		mySprite.sharedMaterial.SetFloat ("_Alpha", 1);
		mySprite.sharedMaterial.SetFloat ("_Val", 1);

		isFlying = false;
        wasFlying = false;
		SetZeroGravFlying (true); // TODO doit provenir de l'arbre des talents (v2)

		Mediator.current.Subscribe<TouchLeft> (PlayerActionLeft);
		Mediator.current.Subscribe<TouchRight> (PlayerActionRight);
		Mediator.current.Subscribe<EndTouch> (PlayerEndAction);
    }
	
	void FixedUpdate() {
		// Assure qu'on soit au sol lorsqu'on est en contact
		grounded = Physics2D.OverlapCircle (groundCheck.position, groundCheckRadius, layerGround);
		myAnim.SetBool ("grounded", grounded);
		myAnim.SetFloat ("verticalSpeed", myRb.velocity.y);
		// Ajuster la vitesse d'animation du héros en fonction de sa vitesse de déplacement
		myAnim.SetFloat("moveSpeed", GetRatioSpeed());
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
			SetJumpHeight (GetMoveSpeed () * constJump);
			myRb.gravityScale = GetJumpHeight () * GetJumpHeight () * constGravity;
		}

		// Action de voler
		if (IsFlying() && WantToFly()) {
			float verticalCoef = 1.5f;
			// TODO Il faut mettre à jour l'information de la Touch, sinon on ne met pas à jour la position...
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
		if (lerpingHP != GetHealthPoint ()) {
			timeLerpHP += TimeManager.deltaTime / delayLerpHP;
			lerpingHP = Mathf.Lerp (previousHP, GetHealthPoint (), timeLerpHP);
			// sharedMaterial pour que les boules changent de couleur aussi
			if (!IsDead ())
				mySprite.sharedMaterial.SetFloat ("_HueShift", _StaticFunction.MappingScale (lerpingHP, 0, GetHealthPointMax (), 230, 0));

		} else {
			previousHP = GetHealthPoint ();
		}
	}

	/* Lorsqu'on appuie à GAUCHE de l'écran, le joueur peut :
	 * - si un ennemi est là		SAUTER
	 * - si aucun ennemi n'est là	SAUTER | VOLER
	 */
	private void PlayerActionLeft (TouchLeft touch) {
		if (!IsFlying ())
			JumpController ();
		else {
			if (WantToFly ())
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
		// Si un ennemi est présent, on ne peut sauter qu'à gauche de l'écran
		if (LevelManager.levelManager.GetEnemyEnCours () != null)
			return;
		
		if (!IsFlying ())
			JumpController ();
		else {
			if (WantToFly ())
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

		myRb.velocity = new Vector2(0, GetJumpHeight());
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
		SetMoveSpeed (0);
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
		if (IsZeroGravFlying ()) {
			myRb.gravityScale = 0;
			SetJumpHeight (0);
		} else {
			myRb.gravityScale = 0.1f;
			SetJumpHeight (0);
		}

		// Augmenter la vitesse si pas déjà en vol
		if (!IsFlying ())
			StartCoroutine (ChangeSpeed (GetMoveSpeed () * flySpeedCoeff));

		//SetMoveSpeed( GetMoveSpeed() * flySpeedCoeff );
		isFlying = true;
		myAnim.SetBool( "flying", isFlying ); // Permet d'annuler le parachute une fois au sol
		myAnim.SetTrigger ("parachute"); // Animation de "parachute" pendant le vol

		// Faire décoller le joueur
		//Jump();

		// Faire en sorte que le nombre de sauts soit illimité (= 1000, n'abusons pas !)
		//SetMaxDoubleJump( 1000 );
    }

	public void Land() {
		isFlying = false;

        // Remettre les paramètres initiaux
//        myRb.gravityScale = initialGravityScale;
//        SetJumpHeight( initialJumpHeight );
        SetMaxDoubleJump( initialMaxDoubleJump );
		StartCoroutine (ChangeSpeed (GetMoveSpeed () / flySpeedCoeff));

        // On "force" le joueur à sauter avant l'atterrissage, signant en même temps la fin du vol
        //Jump();

		// On fait atterrir le joueur avec le parachute
		if (!IsGrounded ()) {
			// On signale au joueur qu'il était en train de voler, pour faire apparaître des nuages s'il tombe dans un trou
			wasFlying = true;

			ActiveParachute (true);
			// Rétablir une gravité "cohérente" avec un parachute
			myRb.gravityScale = initialGravityScale / 3.5f;
		}

		myAnim.SetBool( "flying", isFlying );
    }

	private IEnumerator ChangeSpeed(float newSpeed, float delay = 1) {
		float oldSpeed = GetMoveSpeed ();
		float acceleration = 0;

		while (GetMoveSpeed () != newSpeed) {
			acceleration += TimeManager.deltaTime / delay;
			SetMoveSpeed (Mathf.Lerp (oldSpeed, newSpeed, acceleration));

			if (GetMoveSpeed () >= newSpeed)
				SetMoveSpeed (newSpeed);

			yield return null;
		}
	}

    public void AttractCoins( float radius, LayerMask layerCoins ) {
        nbCoins = Physics2D.OverlapCircleNonAlloc( myTransform.position, radius, coins, layerCoins );

        for( int i = 0; i < nbCoins; i++ ) {
            if( coins[i].transform.position.x > myTransform.position.x + CameraManager.cameraManager.camRightEnd ) {
                continue;
            }

            // Vérifier que le joueur n'a pas déjà pris cette pièce
            if( LevelManager.GetPlayer().HasPickup( coins[i] ) ) {
                continue;
            }

            // Le vecteur direction nous donne la droite entre la pièce et le bonus, donc le joueur
            direction = coins[i].transform.position - myTransform.position;

            // Faire venir la pièce vers le joueur
            // Vitesse inversement proportionelle à la distance, minimum 0.5
            coins[i].transform.Translate( Mathf.Min( 0.5f, 1 / direction.magnitude ) * -direction.normalized );
        }
    }

	public override void Hurt(int damage) {
		// Si les "anciens" HP sont égaux aux "nouveaux" HP, on met à jour, sinon on garde l'encore plus vieille valeur
		if (previousHP == GetHealthPoint ())
			previousHP = GetHealthPoint ();

		timeLerpHP = 0; // On prépare la nouvelle variation de couleur

		if (!IsInvincible() && !IsDead ())
			myAudio.HurtSound ();

		base.Hurt (damage);
	}

	/*protected override IEnumerator HurtEffect() {
		float tempAlpha = mySprite.sharedMaterial.GetFloat ("_Alpha");
		float flashDelay = 0.1f;
		int flashNumber = 0;
		int flashNumberMax = 4;
		bool increment = false;

		while (flashNumber < flashNumberMax) {
			if (increment)
				tempAlpha += TimeManager.deltaTime / flashDelay;
			else 
				tempAlpha -= TimeManager.deltaTime / flashDelay;

			if (tempAlpha > 1) {
				increment = false;
				flashNumber++;
			}
			else if (tempAlpha < 0.25f)
				increment = true;

			mySprite.sharedMaterial.SetFloat ("_Alpha", tempAlpha);
			yield return null;
		}
		// Retour à la "normale"
		mySprite.sharedMaterial.SetFloat ("_Alpha", 1);
	}*/

	protected override IEnumerator HurtEffect() {
		float backToNormalDelay = 0.5f;
		float timeToNormal = backToNormalDelay;
		float maxValue = 100f;
		float minValue = 1f;
		float tempValue = minValue;

		float delaySwitch = 0.1f;
		float timeToSwitch = 0;

		SetInvincible (backToNormalDelay);

		mySprite.sharedMaterial.SetFloat ("_Val", tempValue);

		while (timeToNormal > 0) {
			timeToNormal -= TimeManager.deltaTime;

			if (TimeManager.time > timeToSwitch) {
				timeToSwitch = TimeManager.time + delaySwitch;
				tempValue = (tempValue == minValue) ? maxValue : minValue;
				mySprite.sharedMaterial.SetFloat ("_Val", tempValue);
			}

			yield return null;
		}

		// Retour à la "normale"
		mySprite.sharedMaterial.SetFloat ("_Val", minValue);
	}
}
