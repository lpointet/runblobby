using UnityEngine;
using System.Collections;

public class BulletManager : MonoBehaviour {
	
	public int moveSpeed;
	public int bulletPower;

	private float endOfScreen;

	private Rigidbody2D myRb;
	private Weapon myWeapon;
	private SpriteRenderer myRenderer; // A supprimer : il faut trouver un meilleur moyen de désactiver les balles quand elles font des conneries
	private Collider2D myCollider; // A supprimer : il faut trouver un meilleur moyen de désactiver les balles quand elles font des conneries

	private Transform myTransform;
	public Transform hitParticle;

	public LayerMask layerCollision;

	void Awake() {
		myRb = GetComponent<Rigidbody2D> ();
		myRenderer = GetComponent<SpriteRenderer> ();
		myCollider = GetComponent<Collider2D> ();
		myTransform = transform;

		myWeapon = FindObjectOfType<Weapon> ();
	}

	void Start() {
		bulletPower += myWeapon.weaponPower;
	}
	
	void OnEnable () {
		endOfScreen = Camera.main.transform.position.x + Camera.main.orthographicSize * Camera.main.aspect;// La fin de l'écran

		myRb.velocity = myTransform.right * moveSpeed;
		myRenderer.enabled = true;
		myCollider.enabled = true;
	}
	
	void Update () {
		if (transform.position.x > endOfScreen)
			Despawn(); // on désactive s'il sort de l'écran à droite pour éviter qu'il touche des objets
	}

	void OnTriggerEnter2D(Collider2D other) {
		if ((1 << other.gameObject.layer & layerCollision) != 0) {
			Despawn();
			Transform particle = Instantiate(hitParticle, transform.position, Quaternion.FromToRotation(Vector3.down, transform.right)) as Transform; // Effet vers la balle... :(
			particle.parent = other.transform; // On rattache l'effet au point d'impact pour qu'il suive le mouvement

			// Si on rencontre un ennemi
			if(other.gameObject.layer == LayerMask.NameToLayer("Enemy")) {
				other.GetComponent<Enemy>().HurtEnemy(bulletPower);
			}
		}
	}

	private void Despawn() {
		myRenderer.enabled = false;
		myCollider.enabled = false;
	}
}
