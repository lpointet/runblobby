using UnityEngine;

public class Weapon : MonoBehaviour {

	public int weaponPower = 0;
	public int currentWeaponPower { get; private set; }
	public int weaponPenetration = 0;
	public int currentWeaponPenetration { get; private set; }
	public int weaponCritical = 0;
	public int weaponCritPower = 0;
	public int weaponFireRate = 0;
	public int currentWeaponFireRate { get; private set; }
	public float delayMini = 0.5f;
	public int weaponSpeed = 0;
	public int currentWeaponSpeed { get; private set; }
	public int weaponDoubleShot = 0;
	public int currentWeaponDoubleShot { get; private set; }

	public int weaponAmmunition = 1;
	public int currentWeaponAmmunition { get; private set; }
	private float currentLoad = 1;

	public bool remoteBullet = false;
	public int numberBulletRebound = 0;

	public BulletManager bulletType;
	private string bulletName = "Bullet";

	public bool autoFire = false;
	private float timeToFire = 0;
	private Transform firePoint;

	private bool wantToShoot = false;
	private int shootingId;

	public Character weaponOwner { get; private set; }
	private bool isMachineGun = false;

	public float GetCurrentAmmo () {
		return currentLoad;
	}

	void Awake() {
		bulletName = bulletType.name;
		firePoint = transform.FindChild ("FirePoint");
		if (firePoint == null)
			Debug.LogError ("FirePoint manquant, corrige moi ça boulet !");
		
		weaponOwner = GetComponentInParent<Character> ();
	}

	void Start() {
		Mediator.current.Subscribe<TouchRight> (ShootController);
		Mediator.current.Subscribe<EndTouch> (ShootStop);

		AddOwnerParameters ();

		// On charge les munitions au maximum
		currentLoad = currentWeaponAmmunition;

		// On permet ou non le tir automatique
		isMachineGun = weaponOwner.machineGun > 0 ? true : false;
	}

	void OnEnable () {
		if (autoFire && weaponOwner != LevelManager.player)
			timeToFire = TimeManager.time + Random.Range (2f, 3f);
	}

	void Update () {
        // Empêcher que des choses se passent durant la pause
		if (TimeManager.paused || weaponOwner.IsDead() || LevelManager.player.IsDead() || LevelManager.IsEndingScene())
            return;

		if ( (autoFire || wantToShoot) && TimeManager.time > timeToFire) {
			Shoot();
			//timeToFire = TimeManager.time + 10f / currentWeaponFireRate;
			timeToFire = TimeManager.time + delayMini;
		}

		if (currentLoad < currentWeaponAmmunition) {
			currentLoad += TimeManager.deltaTime * currentWeaponFireRate / 10f;
			if (currentLoad > currentWeaponAmmunition)
				currentLoad = currentWeaponAmmunition;
		}
	}

	private void ShootController(TouchRight touch) {
		if (currentWeaponFireRate == 0 || !isMachineGun) {
			// On attend une frame pour que WeaponRotation.cs ait le temps de se mettre en place
			Invoke ("Shoot", TimeManager.deltaTime);
		} else {
			wantToShoot = true;
			shootingId = touch.rightId;
		}
	}

	private void ShootStop(EndTouch touch) {
		if (shootingId == touch.fingerId)
			wantToShoot = false;
	}

	private void Shoot() {
		// Si on a des munitions, on tire
		if (currentLoad >= 1) {
			// Recalcul de la puissance du tir au moment dudit tir
			AddOwnerParameters ();

			// Cas particulier du joueur : on ajoute la puissance accumulée par les feuilles
			if (weaponOwner == LevelManager.player) {
				if (LevelManager.player.numberLeafBoost > 0) {
					// On ajoute un dixième des feuilles ramassées en puissance
					currentWeaponPower += Mathf.RoundToInt (LevelManager.player.valueLeafBoost * LevelManager.player.coefLeafBoost);
					LevelManager.player.numberLeafBoost--;
				}
			}

			// Chance de double tir
			if (Random.Range (0, 100) < currentWeaponDoubleShot) {
				Invoke ("LaunchBullet", delayMini / 2f);
			}

			// Tir normal
			LaunchBullet ();

			// On retire une munition (l'éventuel double tir est "gratuit");
			currentLoad -= 1;
		}
	}

	private void LaunchBullet () {
		if (weaponOwner.IsDead ())
			return;
		
		GameObject obj = PoolingManager.current.Spawn( bulletName );

		if (obj != null) {
			obj.transform.position = firePoint.position;
			obj.transform.rotation = firePoint.rotation;
			obj.GetComponent<BulletManager> ().SetFiringWeapon (this);
			obj.SetActive (true);
		}
	}

	private void AddOwnerParameters () {
		if (weaponOwner != null) {
			currentWeaponPower = weaponPower + weaponOwner.attack;
			currentWeaponPenetration = weaponPenetration + weaponOwner.sharp;
			currentWeaponDoubleShot = weaponDoubleShot + weaponOwner.shotDouble;
			currentWeaponSpeed = weaponSpeed + weaponOwner.attackSpeed;

			currentWeaponFireRate = weaponFireRate + weaponOwner.attackDelay;
			currentWeaponAmmunition = weaponAmmunition + weaponOwner.machineGun;

			// Ajout des talents de LastWish
			if (GameData.gameData.playerData.talent.lastWishAtk > 0 && weaponOwner == LevelManager.player && LevelManager.player.HasLastWish ()) {Debug.Log(currentWeaponPower);
				currentWeaponPower += Mathf.RoundToInt (GameData.gameData.playerData.talent.lastWishAtk * GameData.gameData.playerData.talent.lastWishAtkPointValue);Debug.Log(currentWeaponPower);
			}
		} else {
			Debug.LogWarning ("No Character for this Weapon " + this.name);

			currentWeaponPower = weaponPower;
			currentWeaponPenetration = weaponPenetration;
			currentWeaponDoubleShot = weaponDoubleShot;
			currentWeaponSpeed = weaponSpeed;

			currentWeaponFireRate = weaponFireRate;
			currentWeaponAmmunition = weaponAmmunition;
		}
	}
}
