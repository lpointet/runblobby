using UnityEngine;
using System.Collections;

public class FlyPickup : Pickup {

	private PlayerController player;
	private Transform playerTransform;
	private Rigidbody2D playerRb;

	private float basePitch;

	public float spawnTime = 0.5f;
	private bool catched = false;
	private bool weakening = false;

	private float birdStartPosition;
	private float distancetoPlayer = 10f;
	public float offsetYToPlayer = 0f;

	private float maxHeight;

    protected override void Awake() {
		base.Awake();

		parentAttach = true;
		despawnTime = 3f;
	}

	void Start() {
		player = LevelManager.GetPlayer ();
		playerTransform = player.transform;
		playerRb = player.GetComponent<Rigidbody2D> ();

		birdStartPosition = Mathf.Abs (LevelManager.levelManager.cameraStartPosition);
		maxHeight = Camera.main.orthographicSize + CameraManager.cameraManager.yOffset;
		basePitch = soundSource.pitch;
	}

	protected override void Update() {
		if( !picked ) {
			return;
		}

		base.Update ();

		// L'oiseau approche du joueur tant qu'il n'est pas sur lui, puis on le laisse le suivre comme un child
		if (!catched) {
			if (distancetoPlayer > 0) { // Tant que l'oiseau n'est pas sur le joueur, on le rapproche
				distancetoPlayer = _StaticFunction.MappingScale (timeToLive, lifeTime, lifeTime - spawnTime, birdStartPosition, 0);
				myTransform.position = new Vector2 (playerTransform.position.x - distancetoPlayer, playerTransform.position.y + offsetYToPlayer);
				if (distancetoPlayer < 0) { // Pour être sûr qu'il soit en 0 (sur le joueur) à la fin, et pas plus loin
					myTransform.position = new Vector2 (playerTransform.position.x, playerTransform.position.y + offsetYToPlayer);
					catched = true;
					myAnim.SetBool("picked", false);
				}
			}
		}

		if (catched && !weakening) {
			if (!weakening) {
				// Adaptation de l'animation et du son de l'oiseau à la vitesse verticale du joueur
				myAnim.SetFloat ("verticalSpeed", playerRb.velocity.y);
				if (playerRb.velocity.y > 0) {
					soundSource.pitch = basePitch;
				} else {
					soundSource.pitch = basePitch * 1.1f;
				}
			} else {
				// Accélération de la vitesse de battement d'ailes avant la fin et du son
				myAnim.speed += Time.deltaTime / despawnTime;
				soundSource.pitch = basePitch * myAnim.speed;
			}
		}
	}

	protected override void OnPick() {
		// Si jamais on prend un autre pickup en même temps, on le vire
		Pickup[] pickups = LevelManager.GetPlayer().GetComponentsInChildren<Pickup>();

		foreach (Pickup pickup in pickups) {
			if (pickup.name.Contains ("Fly") && pickup.gameObject.GetHashCode() != gameObject.GetHashCode()) {
				StartCoroutine( TakeOff(pickup.transform) );
			}
		}

		base.OnPick();

		if (player.IsZeroGravFlying ())
			myAnim.SetBool ("eternal", true);
		else
			myAnim.SetBool ("eternal", false);

		// On attend que l'oiseau arrive pour que le joueur "vole"
		StartCoroutine( WaitBeforeFly(spawnTime) );
	}
		
	protected override void OnDespawn() {
		if (!player.IsDead ())
			player.Land ();

		// Rétablissement de la vitesse d'animation
		myAnim.speed = 1;
		soundSource.pitch = basePitch;
		catched = false;
		weakening = false;

		// Montée de l'oiseau avant de supprimer
		StartCoroutine( TakeOff(myTransform) );
    }

	protected override void DespawnEffect() {
		base.DespawnEffect();

		weakening = true;
	}

	private IEnumerator TakeOff(Transform flyTransform) {
		float flyDistance = 0f;
		while (flyTransform.position.y < maxHeight) {
			flyDistance += Time.deltaTime;
			flyTransform.position = new Vector2 (flyTransform.position.x + flyDistance / 2f, flyTransform.position.y + flyDistance);
			yield return null;
		}

		// On reprend le code de Despawn de Pickup.cs
		if( parentAttach ) {
			// Attacher le bonus à son parent initial
			flyTransform.parent = initialParent;
		}

		flyTransform.gameObject.SetActive( false );
		LevelManager.GetPlayer().RemovePickup( flyTransform.GetComponent<Collider2D>() );
	}

	private IEnumerator WaitBeforeFly(float milli) {
		yield return new WaitForSeconds (milli);

		player.Fly();
	}
}
