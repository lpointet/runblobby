using UnityEngine;
using System.Collections;

public class BulletManager : MonoBehaviour {
	
	public float initialMoveSpeed;
	private float moveSpeed;
	public int initialBulletPower;
	private int bulletPower;
	public int initialBulletPenetration;
	private int bulletPenetration;

	// Module autodguidage
	public bool isRemote = false;
	public int initialNumberRebound = 0;
	private int numberRebound = 0;
	private Character target;
	private Transform targetTransform;
	private float distanceToTarget;
	private float distanceToRebound;
	private float distanceTraveled;
	private bool distanceCalculated = false;

	private Rigidbody2D myRb;
	private Weapon myWeapon;
	private SpriteRenderer mySprite;

	private AudioSource myAudio;
	public AudioClip bulletImpactSound;
	private float initSoundVolume;
	public float impactSoundVolume = 0.5f;

	private Transform myTransform;
	private Vector3 initialScale;
	public TrailRenderer myTrail;
	public GameObject myExplosion;

	private LayerMask layerCollision;
	private LayerMask layerCollisionEnemy; 	// Tous les objets qui peuvent être touchés par le héros
	private LayerMask layerEnemy;			// Uniquement l'ennemi
	private LayerMask layerCollisionPlayer;	// Tous les objets qui peuvent être touchés par les ennemis
	private LayerMask layerPlayer;			// Uniquement le joueur

	private bool alreadyHit;
	private bool alreadyReflect;

	public void SetFiringWeapon (Weapon weapon) {
		myWeapon = weapon;
	}

	void Awake() {
		myRb = GetComponent<Rigidbody2D> ();
		myTransform = transform;
		myAudio = GetComponent<AudioSource> ();
		mySprite = GetComponent<SpriteRenderer> ();

		initSoundVolume = myAudio.volume;

		layerCollisionEnemy = LayerMask.GetMask ("Enemy", "EnemyDrop", "Ground");
		layerEnemy = LayerMask.NameToLayer("Enemy");
		layerCollisionPlayer = LayerMask.GetMask ("Player", "Ground");
		layerPlayer = LayerMask.NameToLayer ("Player");
	}
	
	void OnEnable () {
		float coefScale = 1;

		if (myWeapon != null) {
			// On ajoute la puissance de l'arme à la balle
			bulletPower = initialBulletPower + myWeapon.currentWeaponPower;
			bulletPenetration = initialBulletPenetration + myWeapon.currentWeaponPenetration;

			moveSpeed = initialMoveSpeed * myWeapon.currentWeaponSpeed / 100f;

			// Ajustement de la couche de collisions selon l'appartenance de l'arme
			// Détection de l'autoguidage et récupération de la cible
			// Couleur de la balle
			if (myWeapon.weaponOwner.GetType () == typeof(PlayerController)) {
				mySprite.material.SetFloat ("_HueShift", _StaticFunction.MappingScale (LevelManager.player.healthPoint, 0, LevelManager.player.healthPointMax, 230, 0));

				layerCollision = layerCollisionEnemy;
				isRemote = GameData.gameData.playerData.talent.shotRemote > 0 || myWeapon.remoteBullet;
				numberRebound = GameData.gameData.playerData.talent.shotRemote * (int)GameData.gameData.playerData.talent.shotRemotePointValue + myWeapon.numberBulletRebound;
				if (isRemote) {
					target = LevelManager.levelManager.GetEnemyEnCours ();
					targetTransform = target.transform;
				}
			} else {
				mySprite.material.SetFloat ("_HueShift", _StaticFunction.MappingScale (myWeapon.weaponOwner.healthPoint, 0, myWeapon.weaponOwner.healthPointMax, 25, 0));

				layerCollision = layerCollisionPlayer;
				isRemote = myWeapon.remoteBullet;
				numberRebound = initialNumberRebound + myWeapon.numberBulletRebound;
				if (isRemote) {
					target = LevelManager.player;
					targetTransform = target.transform;
				}
			}

			// On ajuste la taille du missile en fonction des talents
			coefScale = Mathf.Max (1f, myWeapon.weaponOwner.shotWidth / 100f);
		}
			
		myRb.velocity = myTransform.right * moveSpeed;

		StartCoroutine (GrowingBullet (coefScale));

		// On réactive la trainée
		myTrail.gameObject.SetActive (true);
		myTrail.startWidth = coefScale * coefScale * 0.05f;

		mySprite.enabled = true;

		alreadyHit = false;
		alreadyReflect = false;
		myAudio.volume = initSoundVolume;

		distanceTraveled = 0;
		distanceCalculated = false;

		if (myExplosion != null)
			myExplosion.SetActive (false);
	}

	void Update () {
		// Module de téléguidage
		if (isRemote && numberRebound > 0) {
			// Calcul de la distance entre le tir et la cible
			// On ne recalcule pas cette distance (pour éviter des surcharges)
			if (!distanceCalculated) {
				distanceToTarget = Vector2.Distance (transform.position, targetTransform.position);
				distanceToRebound = distanceToTarget / (numberRebound + 1);
				distanceTraveled = 0; // On part de 0, car il s'agit de la distance à parcourir pour CE rebond

				distanceCalculated = true;
			}

			// On met à jour la distance parcourue (dépend du temps "réel")
			distanceTraveled += moveSpeed * TimeManager.deltaTime * Time.timeScale;

			// Si la balle a parcouru la moitié de la distance, elle se redirige
			if (distanceTraveled > distanceToRebound) {
				//* Calcul de la nouvelle position à atteindre
				// Fonction de la position de la cible et de sa vitesse au moment du calcul (estimation de son arrivée)
				// Fonction de la vitesse de la balle et du temps pour parcourir la distance théorique restante sans changement de direction
				float timeToHit = Vector2.Distance (transform.position, targetTransform.position) / moveSpeed / Time.timeScale;
				Vector2 newTargetPosition = (Vector2)targetTransform.position + target.GetComponent<Rigidbody2D> ().velocity * timeToHit;

				//* Redirection de la balle
				Vector2 newVelocity = newTargetPosition - (Vector2)myTransform.position;

				float rotaZ = Vector2.Angle (newVelocity, myRb.velocity);

				// Permet de s'assurer qu'on fait une rotation dans le bon sens (produit de 2 vecteurs)
				Vector3 cross = Vector3.Cross (newVelocity, myRb.velocity);
				if (cross.z > 0)
					rotaZ = 360 - rotaZ;

				float angleTolerance = 10f;
				// Contrainte d'angle pour ne pas que la balle touche trop facilement sa cible (surtout avec plusieurs rebonds)
				if (cross.z > 0 && rotaZ < 360 - angleTolerance)
					rotaZ = 360 - angleTolerance;
				else if (cross.z <= 0 && rotaZ > angleTolerance)
					rotaZ = angleTolerance;

				// Fait tourner la balle
				myTransform.Rotate (new Vector3 (0, 0, rotaZ));
				myRb.velocity = myTransform.right * moveSpeed;

				numberRebound--; // On enlève un rebond à faire
				distanceCalculated = false; // Permet de recalculer une nouvelle valeur
			}
		}
	}

	void OnBecameInvisible () {
		//Despawn();
	}

	void OnTriggerEnter2D(Collider2D other) {
		if (alreadyHit)
			return;
		
		if ((1 << other.gameObject.layer & layerCollision) != 0) {

			// Si on rencontre un ennemi
			if(other.gameObject.layer == layerEnemy) {
				Enemy enemyHit = other.GetComponent<Enemy> ();
				if (enemyHit) {
					// Tentative de renvoi de l'attaque
					if (!alreadyReflect && Random.Range (0, 100) < enemyHit.sendBack) {
						Reflection (enemyHit.transform);

						// Changement d'obédience du missile
						layerCollision = layerCollisionPlayer;

						return;
					} else
						enemyHit.Hurt (bulletPower, bulletPenetration, false, myWeapon.weaponOwner);
				}
			}

			// Si on rencontre un joueur
			if(other.gameObject.layer == layerPlayer) {

				// Tentative de renvoi de l'attaque
				if (!alreadyReflect && Random.Range (0, 100) < LevelManager.player.sendBack) {
					Reflection (LevelManager.player.transform);

					// Changement d'obédience du missile
					layerCollision = layerCollisionEnemy;

					return;
				} else
					LevelManager.player.Hurt (bulletPower, bulletPenetration, false, myWeapon.weaponOwner);
			}

			alreadyHit = true;
			ImpactEffect ();
		}
	}

	private void Despawn() {
		//gameObject.SetActive (false);
		_StaticFunction.SetActiveRecursively (gameObject, false);
	}

	private void Reflection (Transform sender) {
		StartCoroutine (UIManager.uiManager.CombatText (sender, "reflect", LogType.special));
		Debug.Log ("Reflect attack");

		// Renvoi du missile à l'inverse de sa trajectoire initiale
		myRb.velocity *= -1;
		myTransform.localScale *= -1;

		alreadyReflect = true;
	}

	private void ImpactEffect() {
		myRb.velocity = Vector3.zero;

		// Son de l'impact
		myAudio.volume = impactSoundVolume;
		myAudio.PlayOneShot(bulletImpactSound);

		mySprite.enabled = false; // On cache la balle
		myTrail.gameObject.SetActive (false); // On désactive la trainée
		Invoke ("Despawn", 1.5f); // Durée de l'explosion * timeScale

		if (myExplosion != null) {
			myExplosion.SetActive (true);
		}
	}

	private IEnumerator GrowingBullet(float endingScale) {
		float growingTime = 0.25f;
		float currentGrow = 0;

		while (currentGrow < growingTime) {
			myTransform.localScale = (currentGrow / growingTime) * endingScale * Vector3.one;
			currentGrow += TimeManager.deltaTime;

			yield return null;
		}

		myTransform.localScale = endingScale * Vector3.one;
	}
}
