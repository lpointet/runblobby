using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/* GALIA LE GALLINACE
 * Elle esquive parfois les balles du joueur
 * Elle pond des oeufs tous les x% de vies perdues
 * TODO en mode hard, elle sautille vers le joueur de façon aléatoire quand elle attaque
 */
public class Enemy0102 : Enemy {

	[Header("Galia Special")]
	[SerializeField] private LayerMask playerMask;
	[SerializeField] private ParticleUnscaled smokeParticle;
	[SerializeField] private ParticleUnscaled featherParticle;
	[SerializeField] private float dodgeSkill;
	[SerializeField] private int pourcentOeuf = 10;
	[SerializeField] private string oeufName = "Oeuf";
	private int currentOeuf = 100;

	private float moveBackSpeed;
	[SerializeField] private float easyMoveBackSpeed = 10f;

	private List<Oeuf> listeOeufs = new List<Oeuf>();
	private bool angry = false;
	private float lerpingAngryTime = 0f;
	private float delayRunAngry = 0.7f;

	// Zone de détection des balles
	private Collider2D detectedBullet = null;
	private int currentBulletID = 0;
	private float entryTime; // Le moment d'entrée de la dernière balle dans la zone de détection

	// Détection du sol
	private bool grounded;
	[SerializeField] private float groundCheckRadius;

	void Start() {
		currentOeuf -= pourcentOeuf; // Initialise à la première valeur de laché d'oeuf
		//myAnim.SetFloat("ratioHP", 1f);

		moveBackSpeed = easyMoveBackSpeed;

		smokeParticle.gameObject.SetActive (false);
	}

	protected override void Update () {
		base.Update();

		if (IsDead () || LevelManager.player.IsDead () || TimeManager.paused)
			return;

		AngryChicken ();

		// Quand elle n'est pas en colère
		if (!angry) {
			// Elle peut éviter les balles si elle peut sauter (= si elle touche le sol)
			grounded = Physics2D.OverlapCircle (myTransform.position, groundCheckRadius, layerGround);
			if (grounded) {
				DodgeBullet ();
			}

			// Si elle n'est pas à sa place initiale, elle s'y avance régulièrement
			if (myTransform.position.x < startPosition [0]) {
				moveSpeed = moveBackSpeed;
			} else {
				moveSpeed = 0;
			}
			myRb.velocity = new Vector2 (moveSpeed, myRb.velocity.y);
		}
	}

	// Esquive des tirs du joueur
	private void DodgeBullet() {
		detectedBullet = Physics2D.OverlapArea (new Vector2 (myTransform.position.x - 1, myTransform.position.y + 0.5f), new Vector2 (myTransform.position.x - 2.5f, myTransform.position.y - 1), playerMask);

		// Si une balle entre dans la zone derrière la poule
		if (detectedBullet != null && detectedBullet.CompareTag("Bullet")) {
			// On retient la dernière balle prise en compte, pour éviter d'appeler en boucle la même fonction
			if (currentBulletID != detectedBullet.GetInstanceID ()) {
				currentBulletID = detectedBullet.GetInstanceID ();
				entryTime = TimeManager.time; // On retient le moment de l'entrée dans la zone, pour reset au bout de 2sec si jamais il ne se passe rien (voir plus bas)

				// On compare à la probabilité d'esquiver
				if (Random.Range(0f, 1f) < dodgeSkill) {
					// On ajuste la puissance du saut en fonction du lieu d'impact de la balle
					float powerJump = Mathf.Clamp(detectedBullet.transform.position.y, 1.5f, 2);
					// On fait sauter la poule
					myRb.velocity = new Vector2 (myRb.velocity.x, powerJump * jumpHeight);
				}
			}
		}

		if (TimeManager.time > entryTime + 2) {
			currentBulletID = 0;
		}
	}

	/*private void OnDrawGizmos () {
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(new Vector2 (myTransform.position.x - 1, myTransform.position.y + 0.5f),  new Vector2 (myTransform.position.x - 2.5f, myTransform.position.y - 1));
	}*/

	// Attaque du poulet vers le joueur s'il casse un oeuf
	private void AngryChicken () {
		// On contrôle qu'un oeuf est cassé si elle n'est pas déjà énervée
		if (!angry) {
			foreach (Oeuf item in listeOeufs) {
				if (item.IsBroken ()) {
					listeOeufs.Remove (item); // On l'enlève de la liste, vu qu'il est cassé

					angry = true; // On active le mode enragé
					myAnim.SetFloat ("angry", 1);

					break;
				}
			}
		}
		// Si elle vient de s'énervée, elle fonce vers le joueur
		if (angry) {
			// Si elle n'est pas encore un peu avant le joueur, elle s'avance vers lui
			if (myTransform.position.x > LevelManager.player.transform.position.x - 0.5f) {
				// On le fait accélérer
				if (lerpingAngryTime < 1) {
					lerpingAngryTime += TimeManager.deltaTime / delayRunAngry;
					moveSpeed = Mathf.Lerp (0, -moveBackSpeed, lerpingAngryTime);
				}
				myRb.velocity = new Vector2 (moveSpeed, myRb.velocity.y);
			} else {
				angry = false;
				myAnim.SetFloat ("angry", 0);
				lerpingAngryTime = 0;
			}
		}
	}

	public override void Hurt(float damage, int penetration = 0, bool ignoreDefense = false, Character attacker = null) {
		if (IsDead () || LevelManager.player.IsDead())
			return;
		
		base.Hurt (damage, penetration, ignoreDefense, attacker);

		// Impact de balles
		featherParticle.Play ();

		// Calculer le pourcentage de vie restant
		int pourcentVie = Mathf.FloorToInt(100 * healthPoint / (float)healthPointMax);
		// Si la différence vaut plus que la valeur oeufale courante (selon difficulté) et que le poulet n'est pas mort, on lâche un oeuf
		if (pourcentVie <= currentOeuf && !IsDead()) {
			EggDrop ();

			// On fait sauter la poule
			myRb.AddForce(Vector2.up * 250);
			// On ajuste la valeur du prochain oeuf en fonction
			do {
				currentOeuf -= pourcentOeuf;
			} while(currentOeuf > pourcentVie);
		}
	}

	// Laché d'oeufs
	private void EggDrop () {
		RaycastHit2D hit;
		hit = Physics2D.Raycast (myTransform.position, Vector2.down, 20, layerGround); // On essaye de trouver le sol, sinon on ne fait rien

		if (hit.collider != null) {
			GameObject obj = PoolingManager.current.Spawn (oeufName);

			if (obj != null) {
				obj.transform.position = new Vector2(myTransform.position.x - 0.5f, myTransform.position.y - 0.8f); // On fait pondre la poule derrière elle
				obj.transform.rotation = myTransform.rotation;
				obj.transform.parent = hit.transform;
				obj.SetActive (true);

				// On ajoute l'oeuf à la liste des oeufs
				listeOeufs.Add(obj.GetComponent<Oeuf>());
			}
		}
	}

	// A la mort, attacher l'ennemi au sol, et laisser un poulet rôti / un pilon de poulet
	protected override void Despawn () {
		Die ();

		myAnim.SetTrigger ("dead");

		smokeParticle.gameObject.SetActive (true);

		RaycastHit2D hit;
		hit = Physics2D.Raycast (myTransform.position, Vector2.down, 20, layerGround);

		if (hit.collider != null) {
			myTransform.parent = hit.transform;
		}
	}
}
