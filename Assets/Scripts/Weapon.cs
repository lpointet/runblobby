using UnityEngine;

public class Weapon : MonoBehaviour {

	public float fireRate = 0;
	public int weaponPower = 1;
	public LayerMask whatToHit; // Avec Raycast seulement
	public BulletManager bulletType;
	private string bulletName = "Bullet";
	public bool autoFire = false;

	private float timeToFire = 0;
	private Transform firePoint;
	private float timeToSpawnEffect; // Avec Raycast seulement

	private bool wantToShoot = false;
	private int shootingId;
	//private Vector2 shootingFingerPosition;

	private Character weaponOwner;

	/*public Transform muzzleFlashPrefab;
	public Transform bulletTrailPrefab;*/

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
	}

	void Update () {
        // Empêcher que des choses se passent durant la pause
		if (TimeManager.paused || weaponOwner.IsDead() || LevelManager.player.IsDead() || LevelManager.IsEndingScene())
            return;

		if ( (autoFire || wantToShoot) && TimeManager.time > timeToFire ) {
			timeToFire = TimeManager.time + 1 / fireRate;
			Shoot();
		}
	}

	private void ShootController(TouchRight touch) {
		if (fireRate == 0) {
			Shoot ();
		} else {
			wantToShoot = true;
			shootingId = touch.rightId;
			//shootingFingerPosition = touch.rightTouchPosition;
		}
	}

	private void ShootStop(EndTouch touch) {
		if (shootingId == touch.fingerId)
			wantToShoot = false;
	}

	private void Shoot() {
		/* Avec Raycast
		Vector2 mousePosition = new Vector2 (Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
		Vector2 firePointPosition = new Vector2(firePoint.position.x, firePoint.position.y);

		RaycastHit2D hitLine = Physics2D.Raycast (firePointPosition, mousePosition - firePointPosition, 100, whatToHit);

		Debug.DrawLine (firePointPosition, (mousePosition - firePointPosition) * 100, Color.cyan);
		if (hitLine.collider != null)
			Debug.DrawLine (firePointPosition, hitLine.point, Color.red);
		Fin Raycast */

		Effect ();

		GameObject obj = PoolingManager.current.Spawn( bulletName );
		
		if (obj != null) {
			obj.transform.position = firePoint.position;
			obj.transform.rotation = firePoint.rotation;
			obj.SetActive (true);
		}
	}

	private void Effect() {
		/* test muzzle
		Transform clone = Instantiate (muzzleFlashPrefab, firePoint.position+Vector3.right*0.3f, firePoint.rotation) as Transform;
		clone.parent = firePoint;
		float size = Random.Range (0.6f, 0.9f);
		clone.localScale = new Vector3 (size, size, size);
		Destroy (clone.gameObject, 0.05f);
		fin test muzzle */
	}
}
