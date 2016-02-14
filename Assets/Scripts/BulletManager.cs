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
	private LayerMask layerEnemy;
	private LayerMask layerPlayer;

	void Awake() {
		myRb = GetComponent<Rigidbody2D> ();
		myTransform = transform;

		myWeapon = FindObjectOfType<Weapon> ();
	}

	void Start() {
		bulletPower += myWeapon.weaponPower; // On ajoute la puissance de l'arme à la balle
		layerEnemy = LayerMask.NameToLayer("Enemy");
		layerPlayer = LayerMask.NameToLayer ("Player");
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
		if (myTransform.position.x > endOfScreen || myTransform.position.x < startOfScreen)
			Despawn(); // on désactive s'il sort de l'écran pour éviter qu'il touche des objets
	}

	void OnTriggerEnter2D(Collider2D other) {
		if ((1 << other.gameObject.layer & layerCollision) != 0) {
			Despawn();

			// Effet de particule à l'impact
			GameObject particle = PoolingManager.current.Spawn("BulletImpact");

			if (particle != null) {
				particle.transform.position = myTransform.position;
				particle.transform.rotation = Quaternion.Euler(new Vector3(360 - myTransform.rotation.eulerAngles.z, 90, 0));
				particle.GetComponent<ParticleSystemRenderer>().sharedMaterial.SetFloat ("_HueShift", _StaticFunction.MappingScale (LevelManager.GetPlayer().GetHealthPoint(), 0, LevelManager.GetPlayer().GetHealthPointMax (), 210, 0));
				// TODO Améliorer la façon de changer la couleur des particules
				particle.gameObject.SetActive (true);
			}

			// Si on rencontre un ennemi
			if(other.gameObject.layer == layerEnemy) {
				other.GetComponent<Enemy>().Hurt(bulletPower);
			}

			// Si on rencontre un joueur
			if(other.gameObject.layer == layerPlayer) {
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
