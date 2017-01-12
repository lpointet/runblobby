using System.Collections;
using UnityEngine;

public class JumpFireBall : MonoBehaviour {

	private Rigidbody2D myRb;
	private Transform myTransform;
	private Transform parent;

	private float timeToFire;
	private float timeBetweenBall = 5f;
	private float rotaZ;
	private float minStart;
	private float maxStart;
	private float verticalSpeed;
	private float horizontalSpeed;

	private float damageToPlayer;
	[SerializeField] private float normalDamage;
	[SerializeField] private float hardDamage;
	[SerializeField] private float hellDamage;

	private bool ballActivated;

	void Awake () {
		myRb = GetComponent<Rigidbody2D> ();
		myTransform = transform;
		parent = myTransform.parent;

		// Chargement différent selon la difficulté et le mode
		if (LevelManager.levelManager.IsStory ()) {
			switch (LevelManager.levelManager.GetCurrentDifficulty ()) {
			// Normal
			case 0:
				damageToPlayer = normalDamage;
				break;
			// Hard
			case 1:
				damageToPlayer = hardDamage;
				break;
			// Hell
			case 2:
				damageToPlayer = hellDamage;
				break;
			}
		}
	}

	void Start () {
		GetComponent<Animator> ().SetBool ("born", true);
		ballActivated = false;

		minStart = CameraManager.cameraRightPosition / 2f;
		maxStart = CameraManager.cameraRightPosition / 1.5f;
		verticalSpeed = Camera.main.orthographicSize - 1f;
		horizontalSpeed = LevelManager.player.moveSpeed + 0.5f;

		timeToFire = TimeManager.time + timeBetweenBall;
	}

	void Update () {
		if (ballActivated && myTransform.position.y < -3f) { // Quand il repasse sous la lave
			DisableBall ();
		}

		if (!ballActivated && TimeManager.time > timeToFire) {
			ActivateBall ();
		}

		// Rotation pour suivre le mouvement
		rotaZ = Mathf.Rad2Deg * Mathf.Atan2 (myRb.velocity.x, myRb.velocity.y);
		myTransform.rotation = Quaternion.Euler (0, 0, -90 - rotaZ);
	}

	void OnTriggerEnter2D (Collider2D other) {
		if (other.CompareTag ("Player")) {
			LevelManager.player.Hurt (damageToPlayer);
		}
	}

	private void ActivateBall () {
		// Elle perd son parent
		myTransform.SetParent (null);
		// Placement de la boule de feu dans la lave
		myTransform.position = new Vector2 (Random.Range (minStart, maxStart), -1f);
		// On la fait sauter (attention hauteur max et vitesse du héros)
		myRb.velocity = new Vector2 (-horizontalSpeed * Random.Range (0.75f, 1.25f), verticalSpeed * Random.Range (0.75f, 1f));

		timeToFire = TimeManager.time + timeBetweenBall * Random.Range (3 / 4f, 5 / 4f);

		ballActivated = true;
	}

	private void DisableBall () {
		ballActivated = false;
		myTransform.SetParent (parent);
	}

	public void Despawn () {
		gameObject.SetActive (false);
	}
}
