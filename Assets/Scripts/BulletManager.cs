using UnityEngine;
using System.Collections;

public class BulletManager : MonoBehaviour {
	
	public int moveSpeed;
	public int bulletPower;

	private float endOfScreen;

	private Rigidbody2D myRb;
	private Weapon myWeapon;

	private Transform myTransform;
	public Transform hitParticle;

	public LayerMask layerCollision;

	void Awake() {
		myRb = GetComponent<Rigidbody2D> ();
		myTransform = transform;

		myWeapon = FindObjectOfType<Weapon> ();
	}

	void Start() {
		bulletPower += myWeapon.weaponPower; // On ajoute la puissance de l'arme à la balle
	}
	
	void OnEnable () {
		endOfScreen = Camera.main.transform.position.x + Camera.main.orthographicSize * Camera.main.aspect;// La fin de l'écran

		myRb.velocity = myTransform.right * moveSpeed;
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
				other.GetComponent<Enemy>().Hurt(bulletPower);
			}
		}
	}

	private void Despawn() {
		gameObject.SetActive (false);
	}
}
