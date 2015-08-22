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
		mySprite = GetComponent<SpriteRenderer> ();
		levelManager = FindObjectOfType<LevelManager> ();
		SetWeapon( transform.FindChild( "Weapon" ) );
	}
	
	protected override void Init() {
		base.Init();
		SetMoveSpeed( GetInitialMoveSpeed() );
		lerpingHP = GetHealthPoint ();
	}
	
	void FixedUpdate(){
		// Assure qu'on soit au sol lorsqu'on est en contact
		grounded = Physics2D.OverlapCircle (groundCheck.position, groundCheckRadius, layerGround);
		anim.SetBool ("grounded", grounded);
		anim.SetFloat ("verticalSpeed", myRb.velocity.y);
		// Ajuster la vitesse d'animation du héros en fonction de sa vitesse de déplacement
		anim.SetFloat("moveSpeed", GetMoveSpeed () / GetInitialMoveSpeed());
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
		//healthText.text = GetHealthPoint().ToString();
		//fillHealthBar.fillAmount = Mathf.Lerp (fillHealthBar.fillAmount, GetHealthPoint () / (float)GetHealthPointMax (), Time.deltaTime * 2);

		// Rouge = 210 ou -160 (on se laisse une marge de 5 pour approcher davantage de la couleur, vu qu'on l'atteint à la mort seulement)
		lerpingHP = Mathf.Lerp (lerpingHP, GetHealthPoint (), Time.deltaTime * 3);
		// sharedMaterial pour que les boules changent de couleur aussi
		mySprite.sharedMaterial.SetFloat ("_HueShift", _StaticFunction.MappingScale (lerpingHP, 0, GetHealthPointMax (), 210, 0));
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
