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
	private float angelStartPosition;

	private float distanceToPlayer = 0f;
	private float offsetYToPlayer = 0f;
	private float followDelay = 1f;
	private float dampVelocity = 0f;

	private GameObject divineMesh;
	public LayerMask layerGround;

    protected override void Awake() {
        base.Awake();

        parentAttach = true;
		despawnTime = 2.0f;

		// Manip stupide pour pouvoir désactiver l'objet par défaut (le but étant d'éviter de jouer la musique)
		divineMesh = transform.FindChild ("DivineRay").gameObject;
		divineMesh.SetActive (false);
    }

    void Start() {
        player = LevelManager.GetPlayer();
		playerTransform = player.transform;

		angelStartPosition = Mathf.Abs (LevelManager.levelManager.cameraStartPosition); // Voir dans LevelManager pour la position par défaut
	}
	
	public void Launch() {
		player.Resurrect();
		Effect();
		launched = true;
	}

	public bool IsLaunched() {
		return launched;
	} 

    protected override void Update() {
		base.Update ();

		if (!picked) {
			return;
		}

		// Effet commence
		if ( launched ) {
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
			distanceToPlayer = _StaticFunction.MappingScale (timeToLive, lifeTime, 0, angelStartPosition, 0);

			myTransform.position = new Vector2 (playerTransform.position.x - distanceToPlayer, offsetYToPlayer);
		}

		// Effet de l'ange qui amène le joueur au ciel
		if (effectEnding) {
			lerpTimeEnding += Time.deltaTime / despawnTime;
			
			//player.SetMoveSpeed (Mathf.Lerp (player.GetInitialMoveSpeed (), 0, lerpTimeEnding));
			playerTransform.position = new Vector2(playerTransform.position.x, Mathf.Lerp(deathPlayerPosition, endingPlayerPosition, lerpTimeEnding));

			// Rayon divin qui grossit
			divineMesh.transform.localScale = new Vector2( Mathf.Lerp(0.1f, 2, lerpTimeEnding * 3), 20 );
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
			//myAudio.enabled = true;
        }
    }

	public void Cancel() {
		base.OnDespawn();

		// Supprimer la référence dans le joueur
		player.SetLastWish( null );
	}

    protected override void OnDespawn() {
        base.OnDespawn();

        if( effectOnGoing ) {
			if( timeToLive <= 0 ) {
				// Tuer le joueur, vraiment.
	            LevelManager.Kill( player );
			}

			// Supprimer la référence dans le joueur
			player.SetLastWish( null );
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

		soundSource.volume = 1;
    }

	// Effet visuel au moment où on ramasse l'item
	protected override void PickEffect() {
		base.PickEffect();
		
		myTransform.position = new Vector2 (myTransform.position.x, myTransform.position.y + (16 / 32f));
	}

	protected override void DespawnEffect() {
		effectEnding = true;

		player.Die (); // Le joueur est mort au début de l'effet, on ne peut pas utiliser d'animation pour cela

		// On désactive ses colliders pour éviter les obstacles quand il remonte
		Collider2D[] playerCollider = player.GetComponentsInChildren<Collider2D> ();
		foreach (Collider2D col in playerCollider)
			col.enabled = false;

		// On réattribue le pickup au joueur
		myTransform.parent = playerTransform;
		myTransform.position = new Vector2 (myTransform.position.x, myTransform.position.y + 4 / 32f); // Léger décalage de 4 pixels

		lerpTimeEnding = 0f;
		deathPlayerPosition = playerTransform.position.y;
		endingPlayerPosition = Camera.main.orthographicSize + Camera.main.GetComponent<CameraManager>().yOffset + 1; // Pour etre au-dessus

		// Rayon divin
		// Il change de parent pour ne pas bouger
		divineMesh.transform.parent = LevelManager.levelManager.transform;

		divineMesh.transform.position = new Vector2(divineMesh.transform.position.x, 5);
		divineMesh.transform.localScale = new Vector2 (0.1f, 20); // On affiche une mince ligne au début
		divineMesh.SetActive (true);
	}
}
