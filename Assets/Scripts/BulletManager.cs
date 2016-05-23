using UnityEngine;
using System.Collections;

public class BulletManager : MonoBehaviour {
	
	public int moveSpeed;
	public int bulletPower;
	/*public enum bulletDirection { left, right }
	public bulletDirection direction = bulletDirection.right;*/

	private float endOfScreen;
	private float startOfScreen;

	private Rigidbody2D myRb;
	private Weapon myWeapon;
	private SpriteRenderer mySprite;

	public ParticleUnscaled myParticle;
	private AudioSource myAudio;
	public AudioClip bulletImpactSound;
	private float initSoundVolume;
	public float impactSoundVolume = 0.5f;

	private Transform myTransform;

	public LayerMask layerCollision;
	private LayerMask layerEnemy;
	private LayerMask layerPlayer;
	private bool alreadyHit;

	void Awake() {
		myRb = GetComponent<Rigidbody2D> ();
		myTransform = transform;
		myAudio = GetComponent<AudioSource> ();
		mySprite = GetComponent<SpriteRenderer> ();

		myWeapon = FindObjectOfType<Weapon> ();
		initSoundVolume = myAudio.volume;
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
		mySprite.enabled = true;

		/*if( direction == bulletDirection.left ) {
			myRb.velocity *= -1;
		}*/

		alreadyHit = false;
		myAudio.volume = initSoundVolume;
	}
	
	void Update () {
		if (myTransform.position.x > endOfScreen || myTransform.position.x < startOfScreen)
			Despawn(); // on désactive s'il sort de l'écran pour éviter qu'il touche des objets
	}

	void OnTriggerEnter2D(Collider2D other) {
		if (alreadyHit)
			return;
		
		if ((1 << other.gameObject.layer & layerCollision) != 0) {
			alreadyHit = true;

			// Son de l'impact
			myAudio.volume = impactSoundVolume;
			myAudio.PlayOneShot(bulletImpactSound);
			mySprite.enabled = false; // On cache la balle
			Invoke ("Despawn", 0.3f * Time.timeScale); // Durée des particles

			// Effet de particule à l'impact
			if (myParticle != null) {
				myParticle.transform.position = myTransform.position;
				myParticle.transform.rotation = Quaternion.Euler (new Vector3 (360 - myTransform.rotation.eulerAngles.z, 90, 0));
				myParticle.GetComponent<ParticleSystemRenderer> ().sharedMaterial.SetFloat ("_HueShift", _StaticFunction.MappingScale (LevelManager.GetPlayer ().GetHealthPoint (), 0, LevelManager.GetPlayer ().GetHealthPointMax (), 210, 0));		
				myParticle.gameObject.SetActive (true);
			}

			// Si on rencontre un ennemi
			if(other.gameObject.layer == layerEnemy) {
				if (other.GetComponent<Enemy>())
					other.GetComponent<Enemy>().Hurt(bulletPower);
			}

			// Si on rencontre un joueur
			if(other.gameObject.layer == layerPlayer) {
				LevelManager.GetPlayer().Hurt(bulletPower);
			}
		}
	}

	private void Despawn() {
		//gameObject.SetActive (false);
		_StaticFunction.SetActiveRecursively (gameObject, false);
	}
}
