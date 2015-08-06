﻿using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerController : Character {

	/**
	 * Player Stats
	 */
	[SerializeField] private float initialMoveSpeed;
	/* End of Stats */

	public Text healthText;

	private Rigidbody2D myRb;
	private Animator anim;	
	private LevelManager levelManager;

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
	/* End of Getters & Setters */
	
	void Awake () {
		myRb = GetComponent<Rigidbody2D> ();
		anim = GetComponent<Animator> ();
		levelManager = FindObjectOfType<LevelManager> ();
	}

	protected override void Init() {
		base.Init();
		SetMoveSpeed( GetInitialMoveSpeed() );
		//moveSpeed /= 100f;
	}

	void FixedUpdate(){
		// Assure qu'on soit au sol lorsqu'on est en contact
		grounded = Physics2D.OverlapCircle (groundCheck.position, groundCheckRadius, layerGround);
		anim.SetBool ("grounded", grounded);
		anim.SetFloat ("verticalSpeed", myRb.velocity.y);
	}

	void Update () {
		// Mise à jour de la GUI
		healthText.text = GetHealthPoint().ToString();

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

		// On tue le joueur s'il tombe trop bas
	}

	private void Jump() {
		myRb.velocity = new Vector2(myRb.velocity.x, GetJumpHeight());
	}

	public override void OnKill() {
		levelManager.RespawnPlayer();
	}
}
