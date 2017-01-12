using UnityEngine;
using System.Collections;

public class LastWishAngel : MonoBehaviour {

	private bool isActif = false;

	private Transform myTransform;
	[SerializeField] private float movingSpeed = 15f;
	private float correctedMovingSpeed;
	private float distanceToTravel;

	private enum AngelDirection { top, right, bottom, left };
	private AngelDirection myDirection;

	public float addedTime = 2f;

	void Awake () {
		myTransform = transform;
	}

	void Start () {
		Mediator.current.Subscribe<TouchClickable> (IsTouched);
	}

	public void StartAngel () {
		if (!LevelManager.player.HasLastWish ())
			return;

		LevelManager.player.GetLastWish ().DeclareCurrentAngel (this);

		// On place l'ange pour qu'il regarde vers la droite
		myTransform.localScale = Vector3.one;

		if (myTransform.position.y < CameraManager.cameraDownPosition) {
			myDirection = AngelDirection.top;
		}
		else if (myTransform.position.y > CameraManager.cameraUpPosition) {
			myDirection = AngelDirection.bottom;
		}
		else if (myTransform.position.x < CameraManager.cameraLeftPosition) {
			myDirection = AngelDirection.right;
		}
		else if (myTransform.position.x > CameraManager.cameraRightPosition) {
			myDirection = AngelDirection.left;
			myTransform.localScale = new Vector3 (-1, 1, 1); // Exceptionnellement il regarde à gauche
		}
	}

	void Update () {
		if (!isActif)
			return;
		
		switch (myDirection) {
		case AngelDirection.top:
			myTransform.Translate (Vector2.up * correctedMovingSpeed * TimeManager.deltaTime);
			break;
		case AngelDirection.right:
			myTransform.Translate (Vector2.right * correctedMovingSpeed * TimeManager.deltaTime);
			break;
		case AngelDirection.bottom:
			myTransform.Translate (Vector2.down * correctedMovingSpeed * TimeManager.deltaTime);
			break;
		case AngelDirection.left:
			myTransform.Translate (Vector2.left * correctedMovingSpeed * TimeManager.deltaTime);
			break;
		}
	}

	// On considère qu'il est actif quand on le voit
	void OnBecameVisible () {
		isActif = true;
		correctedMovingSpeed = movingSpeed + Random.Range (-movingSpeed / 5f, movingSpeed / 5f);
		GetComponent<Animator> ().SetBool ("actif", true);
	}

	// On le désactive quand il sort de la caméra
	void OnBecameInvisible () {
		if (isActif) {
			DisableAngel ();
		}
	}

	private void IsTouched (TouchClickable touch) {
		if (touch.objectId == this.gameObject.GetInstanceID ()) {
			DisableAngel ();

			// Ajout du temps pour l'ange
			LevelManager.player.GetLastWish ().AddTime (2f);

			// Effet d'explosion
			GameObject dust = PoolingManager.current.Spawn("AerialDust");

			if (dust != null) {
				dust.transform.position = myTransform.position;
				dust.transform.rotation = Quaternion.identity;

				dust.gameObject.SetActive (true);
			}
		}
	}

	private void DisableAngel () {
		LevelManager.player.GetLastWish ().ClearCurrentAngel ();
		isActif = false;
		gameObject.SetActive (false);
	}
}
