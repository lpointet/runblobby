using UnityEngine;

public class LastWishPickup : Pickup {

    public float radiusMagnet = 0f;
    public LayerMask layerCoins;

    private PlayerController player;
	private Transform playerTransform;
    private bool effectOnGoing = false;
    private bool launched = false;
	private bool effectEnding = false;
	private float lerpTimeEnding;
	private float deathPlayerPosition;
	private float endingPlayerPosition;

	private float distancetoPlayer = 0f;
	private float offsetYToPlayer = 0f;
	private float followDelay = 1f;
	private float dampVelocity = 0f;

    protected override void Awake() {
        base.Awake();

        parentAttach = true;
		despawnTime = 2.0f;
    }

    void Start() {
        player = LevelManager.GetPlayer();
		playerTransform = player.transform;
	}
	
	public void Launch() {
		player.Resurrect();
		launched = true;
	}

    protected override void Update() {
		base.Update ();

		if (!picked) {
			return;
		}

		// Effet commence
		if ( launched ) {
			if ( !effectOnGoing ) {
				Effect();
			}

			// T'as un magnet
			player.AttractCoins( radiusMagnet, layerCoins );
		} else {
			// Ce pickup ne doit jamais disparaitre jusqu'à la mort du joueur
			timeToLive = lifeTime;
		}
	}

	void LateUpdate() {
		// Effet visuel de l'ange qui se rapproche jusqu'à la mort
		if (effectOnGoing && !effectEnding) {
			offsetYToPlayer = Mathf.SmoothDamp (myTransform.position.y, playerTransform.position.y, ref dampVelocity, (timeToLive / lifeTime) * followDelay);
			distancetoPlayer = _StaticFunction.MappingScale (timeToLive, lifeTime, 0, Mathf.Abs (LevelManager.levelManager.cameraStartPosition), 0);

			myTransform.position = new Vector2 (playerTransform.position.x - distancetoPlayer, offsetYToPlayer);
		}

		// Effet de l'ange qui amène le joueur au ciel
		if (effectEnding) {
			lerpTimeEnding += Time.deltaTime / despawnTime;
			
			//player.SetMoveSpeed (Mathf.Lerp (player.GetInitialMoveSpeed (), 0, lerpTimeEnding));
			playerTransform.position = new Vector2(playerTransform.position.x, Mathf.Lerp(deathPlayerPosition, endingPlayerPosition, lerpTimeEnding));
		}
    }

    protected override void OnPick() {
        base.OnPick();

        if( player.HasLastWish() ) {
            // Un last wish a déjà été récup, on se casse de là
            timeToLive = 0;
        }
        else {
            player.SetLastWish( this );
        }
    }

    protected override void OnDespawn() {
        base.OnDespawn();

        if( effectOnGoing ) {
            // Désactiver le vol
            player.SetLastWish( null );
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

		// Effet visuel au moment d'activer l'objet
		myTransform.position = new Vector2 (myTransform.position.x - Mathf.Abs (LevelManager.levelManager.cameraStartPosition), playerTransform.position.y);
		myTransform.parent = LevelManager.levelManager.transform; // Pour permettre à l'objet de suivre le joueur

		myAnim.SetBool("picked", false);
		myAnim.SetBool("actif", true);
    }

	// Effet visuel au moment où on ramasse l'item
	protected override void PickEffect() {
		base.PickEffect();
		
		myTransform.position = new Vector2 (myTransform.position.x, myTransform.position.y + (16 / 32f));
	}

	protected override void DespawnEffect() {
		effectEnding = true;

		player.Die (); // Le joueur est mort au début de l'effet, on ne peut pas utilisation d'animation pour cela

		myTransform.parent = playerTransform;
		myTransform.position = new Vector2 (myTransform.position.x, myTransform.position.y + 4 / 32f); // Léger décalage de 4 pixels

		lerpTimeEnding = 0f;
		deathPlayerPosition = playerTransform.position.y;
		endingPlayerPosition = Camera.main.orthographicSize + Camera.main.GetComponent<CameraManager>().yOffset + 1; // Pour etre au-dessus
	}
}
