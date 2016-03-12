using UnityEngine;
using System.Collections;

public class FlyPickup : Pickup {

	private PlayerController player;
	private Transform playerTransform;
	private Rigidbody2D playerRb;

	private float basePitch;

	public float spawnTime = 0.5f;
	private bool catched = false;

	private float birdStartPosition;
	private float distancetoPlayer = 10f;
	public float offsetYToPlayer = 0f;

	private float maxHeight;

    protected override void Awake() {
		base.Awake();

		parentAttach = true;
		despawnTime = 0.6f;
		weakTime = 3f;
	}

	void Start() {
		player = LevelManager.GetPlayer ();
		playerTransform = player.transform;
		playerRb = player.GetComponent<Rigidbody2D> ();

		birdStartPosition = Mathf.Abs (LevelManager.levelManager.cameraStartPosition);
		maxHeight = Camera.main.orthographicSize + CameraManager.cameraManager.yOffset;
		basePitch = 1.13f; // TODO si on prend deux vols à la suite, ça ne revient pas à cette valeur... Donc on force
	}

	protected override void Update() {
		if( !picked || TimeManager.paused) {
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
		} else { // Une fois que l'oiseau est sur lui, on agit différemment tant qu'il n'est pas affaibli
			if (timeToLive > weakTime) {
				// Adaptation de l'animation et du son de l'oiseau à la vitesse verticale du joueur
				myAnim.SetFloat ("verticalSpeed", playerRb.velocity.y);
				if (playerRb.velocity.y > 0 || player.IsZeroGravFlying()) {
					soundSource.pitch = basePitch;
				} else {
					soundSource.pitch = basePitch * 1.2f;
				}
			}
		}
	}

	protected override void OnPick() {
		// Si jamais on prend un autre pickup en même temps, on le vire
		Pickup[] pickups = LevelManager.GetPlayer().GetComponentsInChildren<Pickup>();

		foreach (Pickup pickup in pickups) {
			if (pickup.name.Contains ("Fly")) {
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

	protected override void WeakEffect() {
		// Accélération de la vitesse de battement d'ailes avant la fin et du son
		myAnim.SetBool ("end", true);
		myAnim.speed += TimeManager.deltaTime / weakTime;
		soundSource.pitch = basePitch * myAnim.speed;
	}
		
	protected override void OnDespawn() {
		if (!player.IsDead ())
			player.Land ();

		// Rétablissement de la vitesse d'animation
		myAnim.speed = 1;
		soundSource.pitch = basePitch;
		catched = false;

		// Montée de l'oiseau avant de supprimer
		StartCoroutine( TakeOff(myTransform) );
    }

	protected override void DespawnEffect() {
		if (player.IsDead ())
			despawnTime = 0; // Permet de faire décoller l'oiseau directement lorsque l'on meurt

		base.DespawnEffect();
	}

	// L'oiseau s'envole
	private IEnumerator TakeOff(Transform flyTransform) {
		soundSource.Stop();

		float flyDistance = 0f;
		while (flyTransform.position.y < maxHeight) {
			flyDistance += TimeManager.deltaTime;
			flyTransform.position = new Vector2 (flyTransform.position.x + flyDistance / 2f, flyTransform.position.y + flyDistance);
			yield return null;
		}

		// On reprend le code de Despawn de Pickup.cs (avec la variation du flyTransform)
		if( parentAttach ) {
			// Attacher le bonus à son parent initial
			flyTransform.parent = initialParent;
		}
			
		flyTransform.gameObject.SetActive( false );
		LevelManager.GetPlayer().RemovePickup( flyTransform.GetComponent<Collider2D>() );
	}

	// On attend un certain temps avant que l'oiseau récupère le joueur
	private IEnumerator WaitBeforeFly(float delay) {
		yield return new WaitForSeconds (delay * Time.timeScale);

		player.Fly();
	}
}
