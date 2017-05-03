using System.Collections;
using UnityEngine;

public class Stalactite : MonoBehaviour {

	private Transform myTransform;
	private AudioSource myAudio;
	private Rigidbody2D myRb;
	private Collider2D myCollider;

	[SerializeField] private MeshRenderer stalactiteBackground; // Pour pouvoir faire trembler le haut de l'écran
	private Material stalactiteBgMaterial;
	[SerializeField] private MeshRenderer stalagmiteOriginal; // Pour pouvoir suivre le mouvement "normal" du background
	private Material stalagmiteBgMaterial;

	[SerializeField] private int numberOfLandBeforeFall; // Permet de savoir combien de fois le joueur doit atterrir avant de faire tomber une météorite
	private int countBeforeFall = 0;

	private Transform initialParent;
	private LayerMask isGroundLayer;

	private bool playerWasGrounded;

	void Awake () {
		myTransform = transform;
		initialParent = myTransform.parent;
		myRb = GetComponent<Rigidbody2D> ();
		myCollider = GetComponent<Collider2D> ();
		myAudio = GetComponent<AudioSource> ();
		stalactiteBgMaterial = stalactiteBackground.GetComponent<Renderer> ().material;
		stalagmiteBgMaterial = stalagmiteOriginal.GetComponent<Renderer> ().material;
		isGroundLayer = LayerMask.NameToLayer("Ground");
	}

	void Start () {
		// Chargement différent selon la difficulté et le mode
		if (LevelManager.levelManager.IsStory ()) {
			switch (LevelManager.levelManager.GetCurrentDifficulty ()) {
			// Normal
			case 0:
			// Hard
			case 1:
				stalactiteBackground.gameObject.SetActive (false);
				gameObject.SetActive (false);
				break;
			// Hell
			case 2:

				break;
			}
		}
	}

	void Update () {
		// Mouvement du background des stalactites en fonction des stalagmites
		stalactiteBgMaterial.mainTextureOffset = stalagmiteBgMaterial.mainTextureOffset;

		// Contrôle pour savoir si le joueur a atteri
		// On ne vérifie pas qu'on est sur le stalactite ou non, le cas ne doit pas se présenter...
		if (!playerWasGrounded && LevelManager.player.IsGrounded () && !LevelManager.player.IsFlying ()) {
			ShakeMeteor ();
		}
		playerWasGrounded = LevelManager.player.IsGrounded () && !LevelManager.player.wasFlying;
	}

	private void ShakeMeteor () {
		countBeforeFall++;

		if (countBeforeFall < numberOfLandBeforeFall) {
			StartCoroutine (ShakingMeteor (countBeforeFall / 2.0f, 1.0f));
			CameraManager.cameraManager.ShakeScreen (countBeforeFall);
		}
		else {
			FallMeteor ();
			countBeforeFall = 0; // Reset du compteur
		}
	}

	private IEnumerator ShakingMeteor (float strength, float duration) {
		float currentTime = 0;
		float xOffset = 0;

		while (currentTime < duration) {
			// Diminution des décalages au cours du temps (ratio de 5.0f)
			xOffset = stalagmiteBgMaterial.mainTextureOffset.x + Mathf.Sin (Mathf.PI * currentTime * 15.0f) * strength / (50.0f * (1.0f + 5.0f * currentTime / duration));
			Vector2 offset = new Vector2 (xOffset, stalactiteBgMaterial.mainTextureOffset.y);
			stalactiteBgMaterial.mainTextureOffset = offset;

			currentTime += TimeManager.deltaTime;
			yield return null;
		}
	}

	private void FallMeteor () {
		// On remet le bloc en "trigger"
		myCollider.isTrigger = true;
		myTransform.parent = initialParent;

		// Le but est de la faire tomber 10 blocs devant le joueur (à hauteur équivalente)
		// Il faut donc calculer sa position par rapport à la vitesse du héros et la hauteur de l'écran
		float fallingHeight = CameraManager.cameraUpPosition - LevelManager.player.transform.position.y;
		float fallingSpeed = 5.0f;
		float fallingTime = fallingHeight / fallingSpeed;
		float fallingStartX = LevelManager.player.moveSpeed * fallingTime + 10.0f;
		// Position initiale
		myTransform.position = new Vector2 (fallingStartX, CameraManager.cameraUpPosition);
		myRb.velocity = Vector2.down * fallingSpeed + Vector2.left * LevelManager.player.moveSpeed;
	}

	void OnTriggerEnter2D (Collider2D other) {
		if (other.gameObject.layer == isGroundLayer) {
			RaycastHit2D hit;
			hit = Physics2D.Raycast (myTransform.position, Vector2.down);

			if (hit.collider != null) {
				StartCoroutine (BlockingFall (other.transform, hit.transform.position.y));
			}
		}
	}

	private IEnumerator BlockingFall (Transform parent, float heightContact) {
		myTransform.parent = parent;

		while (myTransform.position.y > heightContact - 1.0f) // On arrête le stalactite à -1.0f du point de contact
			yield return null;
		
		// On bloque le stalactite
		myRb.velocity = Vector2.zero;
		myCollider.isTrigger = false;

		// On fait trembler l'écran
		CameraManager.cameraManager.ShakeScreen ();
	}

	private void OnBecameInvisible () {
		// On le place hors de l'écran, mais on le laisse actif
		myTransform.parent = initialParent;
		myTransform.position = Vector2.up * CameraManager.cameraUpPosition * 2.0f;
	}
}
