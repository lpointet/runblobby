using UnityEngine;
using System.Collections;

public class BulletManager : MonoBehaviour {
	
	public int moveSpeed;
	public int bulletPower;
	public enum bulletDirection { left, right }
	public bulletDirection direction = bulletDirection.right;

	private float endOfScreen;
	private float startOfScreen;

	private Rigidbody2D myRb;
	private Weapon myWeapon;

	private Transform myTransform;
	public Transform hitParticle;

	public LayerMask layerCollision;
	public float despawnTimer;

	void Awake() {
		myRb = GetComponent<Rigidbody2D> ();
		myTransform = transform;

		myWeapon = FindObjectOfType<Weapon> ();
	}

	void Start() {
		bulletPower += myWeapon.weaponPower; // On ajoute la puissance de l'arme à la balle
	}
	
	void OnEnable () {
		float camEdge = Camera.main.orthographicSize * Camera.main.aspect;
		endOfScreen = Camera.main.transform.position.x + camEdge;// La fin de l'écran
		startOfScreen = Camera.main.transform.position.x - camEdge;// Le début de l'écran

		myRb.velocity = myTransform.right * moveSpeed;

		if( direction == bulletDirection.left ) {
			myRb.velocity*= -1;
		}

		StartCoroutine(DespawnAfterDelay(this.gameObject));
	}
	
	void Update () {
		if (transform.position.x > endOfScreen || transform.position.x < startOfScreen)
			Despawn(); // on désactive s'il sort de l'écran pour éviter qu'il touche des objets
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

			// Si on rencontre un joueur
			if(other.gameObject.layer == LayerMask.NameToLayer("Player")) {
				LevelManager.GetPlayer().Hurt(bulletPower);
			}
		}
	}

	private void Despawn() {
		gameObject.SetActive (false);
	}

	
	private IEnumerator DespawnAfterDelay(GameObject obj)
	{
		yield return new WaitForSeconds (despawnTimer);
		obj.SetActive(false);
	}
}
