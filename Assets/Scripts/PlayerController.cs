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
	private Animator anim;
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

    private float initialGravityScale;
    private float initialJumpHeight;
    private int initialMaxDoubleJump;

    private List<Collider2D> pickups = new List<Collider2D>();
    private LastWishPickup lastWish = null;

    // Attract Coins
    private int nbCoins = 0;                                // Nombre de pièces à ramasser
    private Collider2D[] coins = new Collider2D[20];        // Liste des pièces existantes
    private Vector3 direction; 	 							// Vecteur entre le joueur et une pièce

	private float flySpeedCoeff = 2f;
	private float speedBeforeFly;
	private float speedInFly;
	private float acceleration = 0f;
	private bool isFlying = false;

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
    /* End of Getters & Setters */

    protected override void Awake() {
        base.Awake();

		myRb = GetComponent<Rigidbody2D> ();
		anim = GetComponent<Animator> ();
		myAudio = GetComponent<PlayerSoundEffect> ();
		mySprite = GetComponent<SpriteRenderer> ();

		SetWeapon( transform.FindChild( "Weapon" ) );
        initialGravityScale = myRb.gravityScale;
        initialJumpHeight = GetJumpHeight();
        initialMaxDoubleJump = GetMaxDoubleJump();
    }
	
	protected override void Init() {
		base.Init();
		SetMoveSpeed( GetInitialMoveSpeed() );
		lerpingHP = GetHealthPoint ();
		isFlying = false;
        wasFlying = false;
    }
	
	void FixedUpdate(){
		// Assure qu'on soit au sol lorsqu'on est en contact
		grounded = Physics2D.OverlapCircle (groundCheck.position, groundCheckRadius, layerGround);
		anim.SetBool ("grounded", grounded);
		anim.SetFloat ("verticalSpeed", myRb.velocity.y);
		// Ajuster la vitesse d'animation du héros en fonction de sa vitesse de déplacement
		anim.SetFloat("moveSpeed", GetMoveSpeed () / GetInitialMoveSpeed());
	}
	
	protected override void Update () {
		base.Update();
        // Empêcher que des choses se passent durant la pause
		if (Time.timeScale == 0 || IsDead ())
            return;

        if (grounded) // Assure qu'on puisse faire plusieurs sauts à partir du moment où on est au sol
			currentJump = 0;
		
		// Gestion des sauts
		if (Input.GetButtonDown ("Jump") && (grounded || bounced)) {
			Jump ();
            bounced = false;
		}
		if (Input.GetButtonDown ("Jump") && !grounded && currentJump < maxDoubleJump) {
			Jump ();
			currentJump++;
		}

        // Appelé à la fin d'un vol si en l'air et jusqu'à l'atterrisage
        if(!grounded && wasFlying)
        {
            RaycastHit2D hit;
            CloudBlock cloudBlock;

            hit = Physics2D.Raycast(transform.position, new Vector2(2, -4), 1, layerGround);

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
                        cloudBlock.thisNuageActif = true;
                }

                // On réinitialise pour ne plus afficher les éventuels nuages
                wasFlying = false;
            }
        }

		if (speedBeforeFly != speedInFly) {
			acceleration += Time.deltaTime;
			if (isFlying)
				SetMoveSpeed (Mathf.Lerp (speedBeforeFly, speedInFly, acceleration));
			else
				SetMoveSpeed (Mathf.Lerp (speedInFly, speedBeforeFly, acceleration * 2));
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
    }
	
	public override void OnKill() {
        if( HasLastWish() && !lastWish.IsLaunched() ) {
			lastWish.Launch();
            return;
		}
		// On ne peut plus tirer...
		SetFireAbility( false );

		if (transform.position.y < -3.5f) {// Si on est en dessous du bas de l'écran
			anim.SetTrigger ("dead_fall");
			myAudio.FallDeathSound ();
		}
		else {
			anim.SetTrigger ("dead");
			myAudio.DeathSound ();
		}

		StartCoroutine (WaitForDeadAnim (anim));
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

	public bool IsGrounded() {
		return grounded && !isFlying;
	}

    public void Fly() {
		isFlying = true;

        // Abaisser la gravité et la hauteur du saut
        myRb.gravityScale = 0.2f;
        SetJumpHeight( 2 );

        // Faire décoller le joueur
        Jump();

        // Faire en sorte que le nombre de sauts soit illimité (= 1000, n'abusons pas !)
        SetMaxDoubleJump( 1000 );

		// Augmenter la vitesse
		acceleration = 0;
		speedBeforeFly = GetMoveSpeed ();
		speedInFly = GetMoveSpeed() * flySpeedCoeff;

		//SetMoveSpeed( GetMoveSpeed() * flySpeedCoeff );

		anim.SetBool( "flying", isFlying );
    }

    public void Land() {
		isFlying = false;

        // Remettre les paramètres initiaux
        myRb.gravityScale = initialGravityScale;
        SetJumpHeight( initialJumpHeight );
        SetMaxDoubleJump( initialMaxDoubleJump );

        // On signale au joueur qu'il était en train de voler, pour faire apparaître des nuages s'il tombe dans un trou
        wasFlying = true;

        // On "force" le joueur à sauter avant l'atterrissage, signant en même temps la fin du vol
        Jump();

		// Diminuer la vitesse
		acceleration = 0;

		anim.SetBool( "flying", isFlying );
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
