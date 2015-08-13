using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerController : Character {

	/**
	 * Player Stats
	 */
	[SerializeField] private float initialMoveSpeed;
	/* End of Stats */

	// GUI
	public Text healthText;
	public GameObject healthBar;
	public Image fillHealthBar;

	private Rigidbody2D myRb;
	private Animator anim;	
	private LevelManager levelManager;
	private Transform weapon;

	private bool grounded;
	public Transform groundCheck;
	public float groundCheckRadius;
	public LayerMask layerGround;

	private bool doubleJumped;

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
	/* End of Getters & Setters */
	
	void Awake () {
		myRb = GetComponent<Rigidbody2D> ();
		anim = GetComponent<Animator> ();
		levelManager = FindObjectOfType<LevelManager> ();
		SetWeapon( transform.FindChild( "Weapon" ) );
	}

	protected override void Init() {
		base.Init();
		SetMoveSpeed( GetInitialMoveSpeed() );
		// GUI
		healthBar.SetActive (false);
	}

	void FixedUpdate(){
		// Assure qu'on soit au sol lorsqu'on est en contact
		grounded = Physics2D.OverlapCircle (groundCheck.position, groundCheckRadius, layerGround);
		anim.SetBool ("grounded", grounded);
		anim.SetFloat ("verticalSpeed", myRb.velocity.y);
	}

	void Update () {
		if (grounded) // Assure qu'on puisse doubleJump à partir du moment où on est au sol
			doubleJumped = false;

		//rb.velocity = new Vector2 (moveSpeed * Input.GetAxisRaw ("Horizontal"), rb.velocity.y);
		//rb.velocity = new Vector2 (moveSpeed, rb.velocity.y);

		// Gestion des sauts
		if (Input.GetButtonDown ("Jump") && grounded) {
			Jump ();

		}
		if (Input.GetButtonDown ("Jump") && !grounded && !doubleJumped) {
			doubleJumped = true;
			Jump ();
		}

		if (Input.GetKeyDown (KeyCode.A)) {
			myRb.gravityScale = -myRb.gravityScale;
		}
	}

	void OnGUI() {
		healthText.text = GetHealthPoint().ToString();

		if (!levelManager.IsBlockPhase ()) {
			// On affiche la barre de vie qu'en phase ennemie, vu que tout le reste nous tue instantanément
			healthBar.SetActive (true);
			fillHealthBar.fillAmount = GetHealthPoint () / (float)GetHealthPointMax ();
		} else {
			healthBar.SetActive (false);
		}
	}

	private void Jump() {
		myRb.velocity = new Vector2(myRb.velocity.x, GetJumpHeight());
	}

	public override void OnKill() {
		levelManager.RespawnPlayer();
	}

	public void SetFireAbility( bool able ) {
		if( null != weapon ) {
			weapon.gameObject.SetActive( able );
		}
	}
}
