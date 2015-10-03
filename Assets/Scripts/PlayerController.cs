using System.Collections.Generic;
using UnityEngine;

public class PlayerController : Character {
	
	/**
	 * Player Stats
	 */
	[SerializeField] private float initialMoveSpeed;
	[SerializeField] private int maxDoubleJump;
	/* End of Stats */
	
	// GUI
	//public Text healthText;
	//public GameObject healthBar;
	//public Image fillHealthBar;
	private float lerpingHP;
	
	private Rigidbody2D myRb;
	private Animator anim;	
	private LevelManager levelManager;
	private Transform weapon;
	private SpriteRenderer mySprite;
	
	private bool grounded;
    [HideInInspector] public bool bounced = false;
    [HideInInspector] public bool wasFlying = false;
	public Transform groundCheck;
	public float groundCheckRadius;
	public LayerMask layerGround;
	
	private int currentJump = 0;

    private List<Collider2D> pickups = new List<Collider2D>();

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
	/* End of Getters & Setters */

	void Awake () {
		myRb = GetComponent<Rigidbody2D> ();
		anim = GetComponent<Animator> ();
		mySprite = GetComponent<SpriteRenderer> ();
		levelManager = FindObjectOfType<LevelManager> ();
		SetWeapon( transform.FindChild( "Weapon" ) );
	}
	
	protected override void Init() {
		base.Init();
		SetMoveSpeed( GetInitialMoveSpeed() );
		lerpingHP = GetHealthPoint ();
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
        if (Time.timeScale == 0)
            return;

        if (grounded) // Assure qu'on puisse faire plusieurs à partir du moment où on est au sol
			currentJump = 0;

        //myRb.velocity = new Vector2 (GetMoveSpeed() * Input.GetAxisRaw ("Horizontal"), myRb.velocity.y);
		//rb.velocity = new Vector2 (moveSpeed, rb.velocity.y);
		
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
                // Si on touche quelquec hose, on allume les 5 cases autour si ce sont des nuages
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
	}
	
	void OnGUI() {
		//healthText.text = GetHealthPoint().ToString();
		//fillHealthBar.fillAmount = Mathf.Lerp (fillHealthBar.fillAmount, GetHealthPoint () / (float)GetHealthPointMax (), Time.deltaTime * 2);

		// Rouge = 210 ou -160 (on se laisse une marge de 5 pour approcher davantage de la couleur, vu qu'on l'atteint à la mort seulement)
		lerpingHP = Mathf.Lerp (lerpingHP, GetHealthPoint (), Time.deltaTime * 3);
        // sharedMaterial pour que les boules changent de couleur aussi
        if (!IsDead()) mySprite.sharedMaterial.SetFloat ("_HueShift", _StaticFunction.MappingScale (lerpingHP, 0, GetHealthPointMax (), 210, 0));
	}
	
	public void Jump() {
		myRb.velocity = new Vector2(0, GetJumpHeight());
        //myRb.velocity = new Vector2(myRb.velocity.x, GetJumpHeight());
    }
	
	public override void OnKill() {
        anim.SetTrigger("dead");
		levelManager.RespawnPlayer();
	}
	
	public void SetFireAbility( bool able ) {
		if( null != weapon ) {
			weapon.gameObject.SetActive( able );
		}
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
}
