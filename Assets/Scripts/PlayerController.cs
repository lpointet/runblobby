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
	private float lerpingHP;
	
	private Rigidbody2D myRb;
	private Animator myAnim;
	private PlayerSoundEffect myAudio;
	private Transform weapon;
	private SpriteRenderer mySprite;
	
	private bool grounded;
    [HideInInspector] public bool bounced = false;
    [HideInInspector] public bool wasFlying = false;
	public Transform groundCheck;
	public float groundCheckRadius;
	public LayerMask layerGround;
	
	private int currentJump = 0;
	private int initialMaxDoubleJump;

	// Concerne le saut
    private float initialGravityScale;
    private float initialJumpHeight;
    
	// Calculé à partir des formules de portée et de hauteur max en partant des conditions initiales
	// Permet de conserver une hauteur et une distance constante des sauts pour toutes les vitesses horizontales
	private const float maxJumpHeight = 3;
	private const float jumpDistance = 4;
	private const float constJump = 4 * maxJumpHeight / jumpDistance;
	private const float constGravity = 0.5f / maxJumpHeight / 10f;

    private List<Collider2D> pickups = new List<Collider2D>();
    private LastWishPickup lastWish = null;

    // Attract Coins
    private int nbCoins = 0;                                // Nombre de pièces à ramasser
    private Collider2D[] coins = new Collider2D[20];        // Liste des pièces existantes
    private Vector3 direction; 	 							// Vecteur entre le joueur et une pièce

	// Concerne le vol
	private float flySpeedCoeff = 2f;
	private float speedBeforeFly;
	private float speedInFly;
	private float acceleration = 0f; // Temps de transition
	private bool isFlying = false;
	private bool zeroGravFlying = false;
	private float yPosAirDeath;
	private float yVariableAirDeath = 0f;

	private float followDelay = 1f;
	private float dampVelocity = 0f;

	// TODO ratioMoveSpeed = GetMoveSpeed () / GetInitialMoveSpeed() à remplacer dans tous les scripts qui appellent ça ?

    /**
	 * Getters & Setters
	 */
    public float GetInitialMoveSpeed() {
		return initialMoveSpeed;
	}
	
	public void SetInitialMoveSpeed( float value ) {
		initialMoveSpeed = value;
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
    /* End of Getters & Setters */

    protected override void Awake() {
        base.Awake();

		myRb = GetComponent<Rigidbody2D> ();
		myAnim = GetComponent<Animator> ();
		myAudio = GetComponent<PlayerSoundEffect> ();
		mySprite = GetComponent<SpriteRenderer> ();

		SetWeapon( myTransform.FindChild( "Weapon" ) );
//        initialGravityScale = myRb.gravityScale;
//        initialJumpHeight = GetJumpHeight();
        initialMaxDoubleJump = GetMaxDoubleJump();
    }
	
	protected override void Init() {
		base.Init();

		SetMoveSpeed( GetInitialMoveSpeed() );
		lerpingHP = GetHealthPoint ();
		isFlying = false;
        wasFlying = false;
		SetZeroGravFlying (false); // TODO doit provenir de l'arbre des talents (v2)
    }
	
	void FixedUpdate(){
		// Assure qu'on soit au sol lorsqu'on est en contact
		grounded = Physics2D.OverlapCircle (groundCheck.position, groundCheckRadius, layerGround);
		myAnim.SetBool ("grounded", grounded);
		myAnim.SetFloat ("verticalSpeed", myRb.velocity.y);
		// Ajuster la vitesse d'animation du héros en fonction de sa vitesse de déplacement
		myAnim.SetFloat("moveSpeed", GetMoveSpeed () / GetInitialMoveSpeed());
	}
	
	protected override void Update () {
		base.Update();

		// Ajustement du saut et gravité en fonction de la vitesse
		if (!isFlying) {
			SetJumpHeight (GetMoveSpeed () * constJump);
			myRb.gravityScale = GetJumpHeight () * GetJumpHeight () * constGravity;
		}

		// Vol sur place du fantôme pendant la mort en l'air
		if (IsDead () && !HasLastWish() && !IsGrounded () && myTransform.position.y > 3.5f) {
			yVariableAirDeath += Time.deltaTime;
			myTransform.position = new Vector2 (myTransform.position.x, yPosAirDeath + 0.2f * Mathf.Sin (yVariableAirDeath));
		}

        // Empêcher que des choses se passent durant la pause
		if (Time.timeScale == 0 || IsDead ())
            return;

		// Rapprocher le joueur douuuucement si on est pas en x = 0
		if (myTransform.position.x < 0)
			myTransform.Translate (Vector3.right * 0.005f);

		// Permet de suivre le "doigt" du joueur quand il vole en zéro gravité
		if (IsFlying() && IsZeroGravFlying ()) {
			if (Input.GetMouseButton (0)) {
				float cameraCursorY = Camera.main.ScreenToWorldPoint (Input.mousePosition).y;

				// On ne bouge que si le curseur est suffisament loin du joueur (pour éviter des zigzags)
				if (Mathf.Abs (cameraCursorY - myTransform.position.y) > 0.1f)
					myRb.velocity = new Vector2 (0, Mathf.Sign ((cameraCursorY - myTransform.position.y)) * 5);
				else
					myRb.velocity = Vector2.zero;
			} else { // Si on n'appuie pas, on ne bouge pas
				myRb.velocity = Vector2.zero;
			}
		}

		// Assure qu'on puisse faire plusieurs sauts à partir du moment où on est au sol
		if (IsGrounded ())
			currentJump = 0;
		
		// Gestion des sauts
		if (Input.GetButtonDown ("Jump") && (IsGrounded () || bounced)) {
			Jump ();
            bounced = false;
		}
		if (Input.GetButtonDown ("Jump") && !IsGrounded () && currentJump < maxDoubleJump) {
			Jump ();
			currentJump++;
		}

        // Appelé à la fin d'un vol si en l'air et jusqu'à l'atterrisage
		if(!IsGrounded () && wasFlying)
        {
            RaycastHit2D hit;
            CloudBlock cloudBlock;

			hit = Physics2D.Raycast(myTransform.position, new Vector2(2, -4), 1, layerGround);

            if (hit.collider != null)
            {
                Collider2D[] colliderHits = new Collider2D[5];
                int nbCollider;
                // Si on touche quelque chose, on allume les 5 cases autour si ce sont des nuages
                nbCollider = Physics2D.OverlapAreaNonAlloc(new Vector2(hit.point.x - 0.5f, hit.point.y - 0.4f), new Vector2(hit.point.x + 3.5f, hit.point.y + 0.4f), colliderHits, layerGround);
                for (int j = 0; j < nbCollider; j++)
                {
                    cloudBlock = colliderHits[j].GetComponent<CloudBlock>();
					if (cloudBlock != null)
						cloudBlock.ActiverNuage (true);
                }

                // On réinitialise pour ne plus afficher les éventuels nuages
                wasFlying = false;
            }
        }

		// Accélération et décélération au début et à la fin du vol
		if (speedBeforeFly != speedInFly) {
			acceleration += Time.deltaTime;
			if (isFlying) // Au démarrage
				SetMoveSpeed (Mathf.Lerp (speedBeforeFly, speedInFly, acceleration)); // En une seconde
			else {
				SetMoveSpeed (Mathf.Lerp (speedInFly, speedBeforeFly, acceleration * 2)); // En 0.5 seconde
				// Quand on a atteint la bonne vitesse, on évite de rappeler cette fonction
				if (GetMoveSpeed () == speedBeforeFly)
					speedInFly = speedBeforeFly;
			}
		}
	}
	
	void OnGUI() {
		// Rouge = 210 ou -160 (on se laisse une marge de 5 pour approcher davantage de la couleur, vu qu'on l'atteint à la mort seulement)
		lerpingHP = Mathf.Lerp (lerpingHP, GetHealthPoint (), Time.deltaTime * 3);
        // sharedMaterial pour que les boules changent de couleur aussi
        if (!IsDead()) mySprite.sharedMaterial.SetFloat ("_HueShift", _StaticFunction.MappingScale (lerpingHP, 0, GetHealthPointMax (), 210, 0));
	}
	
	public void Jump() {
		myRb.velocity = new Vector2(0, GetJumpHeight());
		myAudio.JumpSound ();

		// Affichage d'un effet de "nuage" à l'endroit du saut s'il est effectué en l'air
		if (!IsGrounded () && !IsFlying ()) {
			GameObject dust = PoolingManager.current.Spawn("AerialDust");

			if (dust != null) {
				dust.transform.position = myTransform.position;
				dust.transform.rotation = Quaternion.identity;

				dust.gameObject.SetActive (true);
			}
		}
    }

	// TODO supprimer l'apparition du fantôme quand on meurt avec le LastWish
	public override void OnKill() {
		// On ne peut plus tirer...
		SetFireAbility( false );

		if (myTransform.position.y < -3.5f) { // Si on est en dessous du bas de l'écran
			myAnim.SetTrigger ("dead_fall");
			myAudio.FallDeathSound ();
		}
		else {
			if (!IsGrounded ()) { // Faire flotter le fantôme si on est en l'air
				myRb.gravityScale = 0f;
				isFlying = false;
				myAnim.SetBool ("flying", isFlying);
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
		//Application.LoadLevelAdditive (1);
		UIManager.uiManager.ToggleEndMenu (true);
	}

	public void OnVictory() {
		// On ne peut plus tirer...
		SetFireAbility( false );
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

    public void Fly() {
		isFlying = true;

        // Abaisser la gravité et la hauteur du saut
		if (IsZeroGravFlying ()) {
			myRb.gravityScale = 0;
			SetJumpHeight (0);
		} else {
			myRb.gravityScale = 0.2f;
			SetJumpHeight (2);
		}

        // Faire décoller le joueur
        Jump();

        // Faire en sorte que le nombre de sauts soit illimité (= 1000, n'abusons pas !)
        SetMaxDoubleJump( 1000 );

		// Augmenter la vitesse
		acceleration = 0;
		speedBeforeFly = GetMoveSpeed ();
		speedInFly = GetMoveSpeed() * flySpeedCoeff;

		//SetMoveSpeed( GetMoveSpeed() * flySpeedCoeff );

		myAnim.SetBool( "flying", isFlying );
    }

    public void Land() {
		isFlying = false;

        // Remettre les paramètres initiaux
//        myRb.gravityScale = initialGravityScale;
//        SetJumpHeight( initialJumpHeight );
        SetMaxDoubleJump( initialMaxDoubleJump );

        // On signale au joueur qu'il était en train de voler, pour faire apparaître des nuages s'il tombe dans un trou
        wasFlying = true;

        // On "force" le joueur à sauter avant l'atterrissage, signant en même temps la fin du vol
        Jump();

		// Diminuer la vitesse
		acceleration = 0;

		myAnim.SetBool( "flying", isFlying );
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
}
