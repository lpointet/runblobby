using UnityEngine;
using System.Collections;

// TODO si zerogravity et qu'on déclenche le lastiwsh, movespeed = 0.

public class FlyPickup : Pickup {

	private Transform playerTransform;
	private Rigidbody2D playerRb;

	private float basePitch;

	public float spawnTime = 0.5f;
	private bool catched = false;

	private float birdStartPosition;
	private float distancetoPlayer = 10f;
	public float offsetYToPlayer = 0f;

	public AutoCoinPickup autoCoinBonus;

	void OnValidate () {
		if (autoCoinBonus != null && autoCoinBonus.lifeTime != lifeTime)
			autoCoinBonus.lifeTime = lifeTime;
	}

    protected override void Awake() {
		base.Awake();

		parentAttach = true;
		despawnTime = 0.6f;
		weakTime = 3f;
	}

	void Start() {
		playerTransform = LevelManager.player.transform;
		playerRb = LevelManager.player.GetComponent<Rigidbody2D> ();

		birdStartPosition = Mathf.Abs (CameraManager.cameraStartPosition);

		if (soundSource != null)
			basePitch = this.soundSource.pitch;
	}

	void Reset () {
		catched = false;
		distancetoPlayer = 10f;
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
					LevelManager.player.Fly();
				}
			}
		} else { // Une fois que l'oiseau est sur lui, on agit différemment tant qu'il n'est pas affaibli
			if (timeToLive > weakTime) {
				// Adaptation de l'animation et du son de l'oiseau à la vitesse verticale du joueur
				myAnim.SetFloat ("verticalSpeed", playerRb.velocity.y);

				if (soundSource != null) {
					if (playerRb.velocity.y > 0 || LevelManager.player.IsZeroGravFlying ()) {
						soundSource.pitch = basePitch;
					} else {
						soundSource.pitch = basePitch * 1.2f;
					}
				}
			}
		}
	}

	protected override void OnPick() {
		// Si jamais on prend un autre pickup en même temps, on le vire
		Pickup[] pickups = LevelManager.player.GetComponentsInChildren<Pickup>();

		foreach (Pickup pickup in pickups) {
			if (pickup.name.Contains ("Fly")) {
				StartCoroutine( TakeOff(pickup.transform) );
			}
		}

		Reset ();

		// Règles spéciales pour le Fly
		if( parentAttach ) {
			// Attacher le bonus au joueur
			myTransform.parent = LevelManager.player.transform;
			myTransform.position = myTransform.parent.position;
		}

		if (mySprite != null)
			mySprite.transform.localPosition = new Vector2(1 / 16f, -5.4f / 16f); // On place le Sprite correctement par rapport au joueur

		LevelManager.player.AddPickup( myCollider );

		if (LevelManager.player.IsZeroGravFlying ())
			myAnim.SetBool ("eternal", true);
		else
			myAnim.SetBool ("eternal", false);

		// On attend que l'oiseau arrive pour que le joueur "vole"
		LevelManager.player.Jump(LevelManager.player.jumpHeight);

		// On lui donne un bonus d'AutoCoin réduit
		autoCoinBonus.ForceOnPick();
		autoCoinBonus.gameObject.SetActive(true);
	}

	protected override void WeakEffect() {
		// Accélération de la vitesse de battement d'ailes avant la fin et du son
		myAnim.SetBool ("end", true);
		myAnim.speed += TimeManager.deltaTime / weakTime;
		if (soundSource != null)
			soundSource.pitch = basePitch * myAnim.speed;
	}
		
	protected override void OnDespawn() {
		if (!LevelManager.player.IsDead ())
			LevelManager.player.Land ();

		// Rétablissement de la vitesse d'animation
		myAnim.speed = 1;
		if (soundSource != null)
			soundSource.pitch = basePitch;

		// Montée de l'oiseau avant de supprimer
		StartCoroutine( TakeOff(myTransform) );

		autoCoinBonus.ForceOnDespawn();
		autoCoinBonus.gameObject.SetActive(false);
    }

	protected override void DespawnEffect() {
		if (LevelManager.player.IsDead ())
			despawnTime = 0; // Permet de faire décoller l'oiseau directement lorsque l'on meurt

		base.DespawnEffect();
	}

	// L'oiseau s'envole
	private IEnumerator TakeOff(Transform flyTransform) {
		float maxHeight = Camera.main.orthographicSize + CameraManager.cameraManager.yOffset;
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

		if (soundSource != null)
			soundSource.Stop ();
		
		LevelManager.player.RemovePickup( flyTransform.GetComponent<Collider2D>() );
		flyTransform.gameObject.SetActive( false );
	}
}
