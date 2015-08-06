using UnityEngine;
using System.Collections;

public class Enemy : MonoBehaviour {

	[System.Serializable]
	public class Stats
	{
		public int healthPoint;
		public int healthPointMax;
		public float moveSpeed;
		public float jumpHeight;
		public bool isDead;
		public float distanceToKill;

		public int damageToGive;
		public int pointScore;
	}
	public Stats stats = new Stats();

	public bool movingEnemy;
	public bool moveRight;

	private Rigidbody2D myRb;
	private Transform myTransform;

	private bool hittingWall;
	public Transform wallCheck;
	public float wallCheckRadius;
	public LayerMask whatIsWall;
	
	public bool bouncableEnemy;
	public float offsetCheckBounce;
	public float bouncePower;
	
	private Rigidbody2D herosRb;
	
	void Awake () {
		myRb = GetComponent<Rigidbody2D> ();
		myTransform = transform;
	}

	void Start() {
		init();
	}

	void OnEnable() {
		init();
	}

	private void init() {
		// Init health
		stats.healthPoint = stats.healthPointMax;
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
				myRb.velocity = new Vector2 (stats.moveSpeed, myRb.velocity.y);
			} else {
				myTransform.localScale = new Vector3 (1f, 1f, 1f);
				myRb.velocity = new Vector2 (-stats.moveSpeed, myRb.velocity.y);
			}
		}
	}

	void OnTriggerEnter2D(Collider2D other){
		if (other.name == "Heros") {
			// On ne rebondit que si l'ennemi est de bouncable
			if (bouncableEnemy) {
				herosRb = other.attachedRigidbody;
				
				if (other.transform.position.y - offsetCheckBounce > transform.position.y) {
					herosRb.velocity = new Vector2 (herosRb.velocity.x, bouncePower);
					ScoreManager.AddPoint (stats.pointScore);
				} else {
					LevelManager.getPlayer ().HurtPlayer (stats.damageToGive);
				}
				Despawn ();
			} else {
				LevelManager.getPlayer ().HurtPlayer (stats.damageToGive);
			}
		}
	}

	public void HurtEnemy(int damage) {
		stats.healthPoint -= damage;
		
		if (stats.healthPoint <= 0) {
			stats.isDead = true;
			ScoreManager.AddPoint (stats.pointScore);
			Despawn();
		}
	}

	private void Despawn() {
		gameObject.SetActive (false);
	}
}
