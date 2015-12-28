using UnityEngine;
using UnityEngine.UI;

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

	private bool hittingWall;
	public Transform wallCheck;
	public float wallCheckRadius;
	public LayerMask whatIsWall;

	public float frequence = 0.01f;
	public GameObject[] coins;
	public LayerMask layerGround;
	private float distanceParcourue = 0;
	private float pointLastDropCheck = 0;
	public float intervalleDrop = 0.5f;

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
	
	protected override void Awake () {
        base.Awake();

		myRb = GetComponent<Rigidbody2D> ();
		frequence = Mathf.Clamp01 (frequence);
	}

	protected override void Update () {
		base.Update();

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

		// Laché de feuilles aléatoires
		if (!LevelManager.GetPlayer ().IsDead ())
			MoneyDrop ();
	}

	void OnTriggerEnter2D(Collider2D other){
		// Si l'ennemi est déjà mort, il ne peut plus rien faire...
		if( IsDead() ) {
			return;
		}

		if (other.name == "Heros")
			LevelManager.GetPlayer ().Hurt(GetDamageToGive());
	}

	public override void OnKill() {
		ScoreManager.AddPoint (GetPointScore(), ScoreManager.Types.Experience);
		Despawn();
	}

	protected void Despawn() {
		gameObject.SetActive (false);
	}

	/* Fonction qui permet de déposer des feuilles à la suite du boss
	 * Dépend de la difficulté, de la fréquence souhaitée
	 * Possibilité de régler la valeur moyenne des feuilles qui tombent */
	protected void MoneyDrop() {
		distanceParcourue += LevelManager.levelManager.GetLocalDistance ();

		if (distanceParcourue > pointLastDropCheck + intervalleDrop) { // On ne propose de poser une feuille que tous les intervalleDrop parcourus
			pointLastDropCheck = distanceParcourue;

			if (Random.Range (0f, 1f) <= frequence) { // On pose une pièce en respectant la fréquence
				RaycastHit2D hit;
				hit = Physics2D.Raycast (myTransform.position, Vector2.down, 20, layerGround); // On essaye de trouver le sol, sinon on ne fait rien
				if (hit.collider != null) {
					GameObject coin = PoolingManager.current.Spawn ("Leaf1");

					coin.gameObject.SetActive (true);

					coin.transform.position = new Vector2 (myTransform.position.x, hit.transform.position.y + 0.25f);
					coin.transform.parent = hit.transform;
				}
			}
		}
	}
}
