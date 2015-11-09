using UnityEngine;

public class LastWishPickup : Pickup {

    public float radiusMagnet = 0f;
    public LayerMask layerCoins;

    private PlayerController player;
	private Transform playerTransform;
    private bool effectOnGoing = false;

	private float distancetoPlayer = 0f;
	private float offsetYToPlayer = 0f;
	private float followDelay = 0.5f;
	private float dampVelocity = 0f;

    protected override void Awake() {
        base.Awake();

        parentAttach = true;
    }

    void Start() {
        player = LevelManager.getPlayer();
		playerTransform = player.transform;
    }

    protected override void Update() {
		base.Update ();

		if (!picked) {
			return;
		}

		if (player.IsDead ()) {
			if (!effectOnGoing) {
				Effect ();
			}

			// T'as un magnet
			player.AttractCoins (radiusMagnet, layerCoins);
		} else {
			// Ce pickup ne doit jamais disparaitre jusqu'à la mort du joueur
			timeToLive = lifeTime;
		}
	}

	void LateUpdate() {
		// Effet visuel de l'ange qui se rapproche jusqu'à la mort
		if (effectOnGoing) {
			offsetYToPlayer = Mathf.SmoothDamp (myTransform.position.y, playerTransform.position.y, ref dampVelocity, (timeToLive / lifeTime) * followDelay);
			distancetoPlayer = _StaticFunction.MappingScale (timeToLive, lifeTime, 0, Mathf.Abs (LevelManager.levelManager.cameraStartPosition), 0);

			myTransform.position = new Vector2 (playerTransform.position.x - distancetoPlayer, offsetYToPlayer);
		}
    }

    protected override void OnPick() {
        base.OnPick();

        if( player.HasLastWish() ) {
            // Un last wish a déjà été récup, on se casse de là
            timeToLive = 0;
        }
        else {
            player.SetLastWish( true );
        }
    }

    protected override void OnDespawn() {
        base.OnDespawn();

        if( effectOnGoing ) {
            // Désactiver le vol
            player.Land();
            player.SetLastWish( false );
            player.OnKill();
        }
    }

    public void Effect() {
        effectOnGoing = true;

        // C'est pour l'instant le seul moyen que j'ai trouvé pour ne pas rester dans la position de la mort (qui peut être bloquante)
        // TODO: remplacer ou améliorer avec une animation ?
		playerTransform.position = LevelManager.levelManager.currentCheckPoint.transform.position;

        // Tu voles
        player.Fly();

        // T'es invul
        player.SetInvincible( lifeTime );

		// Effet visuel
		myTransform.position = new Vector2 (myTransform.position.x - Mathf.Abs (LevelManager.levelManager.cameraStartPosition), playerTransform.position.y);
		myTransform.parent = LevelManager.levelManager.transform; // Pour permettre à l'objet de suivre le joueur
    }

	protected override void PickEffect() {
		base.PickEffect();
		
		myTransform.position = new Vector2 (myTransform.position.x, myTransform.position.y + (16 / 32f));
	}

	protected override void DespawnEffect() {

	}
}
