using UnityEngine;
using System.Collections;

public class Enemy : Character {

	/**
	 * Enemy Stats
	 */
	[SerializeField] private float distanceToKill;
	[SerializeField] private int damageToGive;
	[SerializeField] private int pointScore;
	/* End of Stats */

	public bool movingEnemy;
	public bool moveRight;

	private Rigidbody2D myRb;
	private Transform myTransform;

	private bool hittingWall;
	public Transform wallCheck;
	public float wallCheckRadius;
	public LayerMask whatIsWall;

	/**
	 * Getters & Setters
	 */
	public float GetDistanceToKill() {
		return distanceToKill;
	}

	public int GetDamageToGive() {
		return damageToGive;
	}

	public int GetPointScore() {
		return pointScore;
	}

	public void SetDistanceToKill( float value ) {
		distanceToKill = value;
	}

	public void SetDamageToGive( int value ) {
		damageToGive = value;
	}

	public void SetPointScore( int value ) {
		pointScore = value;
	}
	/* End of Getters & Setters */

	void OnEnable() {
		Init();
	}
	
	void Awake () {
		myRb = GetComponent<Rigidbody2D> ();
		myTransform = transform;
	}

	void Update () {
		// On n'appelle ça que si l'ennemy bouge
		if (movingEnemy) {
			// Vérifie que l'ennemi touche un mur ou non
			hittingWall = Physics2D.OverlapCircle (wallCheck.position, wallCheckRadius, whatIsWall);
			// Changement de direction si on touche un mur
			if (hittingWall)
				moveRight = !moveRight;
			// Position et échelle selon la direction
			if (moveRight) {
				myTransform.localScale = new Vector3 (-1f, 1f, 1f);
				myRb.velocity = new Vector2 (GetMoveSpeed(), myRb.velocity.y);
			} else {
				myTransform.localScale = new Vector3 (1f, 1f, 1f);
				myRb.velocity = new Vector2 (-GetMoveSpeed(), myRb.velocity.y);
			}
		}
	}

	void OnTriggerEnter2D(Collider2D other){
		if (other.name == "Heros")
			LevelManager.getPlayer ().Hurt(GetDamageToGive());
	}

	public override void OnKill() {
		ScoreManager.AddPoint (GetPointScore());
		Despawn();
	}

	protected void Despawn() {
		gameObject.SetActive (false);
	}
}
