﻿using UnityEngine;

public class LastWishPickup : Pickup {

    public float radiusMagnet = 0f;
    public LayerMask layerCoins;

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
	private float followDelay = 1.5f;
	private float dampVelocity = 0f;

	private GameObject divineMesh;
	public LayerMask layerGround;

	public FlyPickup flyPickup;

    protected override void Awake() {
        base.Awake();

        parentAttach = true;
		despawnTime = 2.0f;

		// Manip stupide pour pouvoir désactiver l'objet par défaut (le but étant d'éviter de jouer la musique)
		divineMesh = transform.FindChild ("DivineRay").gameObject;
		divineMesh.SetActive (false);

		flyPickup.gameObject.SetActive (false);
    }

    void Start() {
		playerTransform = LevelManager.GetPlayer().transform;

		angelStartPosition = Mathf.Abs (LevelManager.levelManager.cameraStartPosition) - 2; // Voir dans LevelManager pour la position par défaut
	}

	public void Launch() {
		LevelManager.GetPlayer().Resurrect();
		Effect();
		launched = true;
	}

	public bool IsLaunched() {
		return launched;
	} 

    protected override void Update() {
		base.Update ();

		if (!picked || TimeManager.paused)
			return;

		// Effet commence
		if ( launched ) {
			// T'as un magnet
			LevelManager.GetPlayer().AttractCoins( radiusMagnet, layerCoins );
		} else {
			// Ce pickup ne doit jamais disparaitre jusqu'à la mort du joueur
			timeToLive = lifeTime;
		}
	}

	void LateUpdate() {
		if (TimeManager.paused)
			return;
		
		// Effet visuel de l'ange qui se rapproche jusqu'à la mort
		if (effectOnGoing && !effectEnding) {
			offsetYToPlayer = Mathf.SmoothDamp (myTransform.position.y, playerTransform.position.y, ref dampVelocity, ((timeToLive - despawnTime) / lifeTime) * followDelay);
			distanceToPlayer = _StaticFunction.MappingScale (timeToLive - despawnTime, lifeTime, 0, angelStartPosition, 0);

			myTransform.position = new Vector2 (playerTransform.position.x - distanceToPlayer, offsetYToPlayer);
		}

		// Effet de l'ange qui amène le joueur au ciel
		if (effectEnding) {
			lerpTimeEnding += TimeManager.deltaTime / despawnTime;

			playerTransform.position = new Vector2(playerTransform.position.x, Mathf.Lerp(deathPlayerPosition, endingPlayerPosition, lerpTimeEnding));

			// Rayon divin qui grossit
			divineMesh.transform.localScale = new Vector2( Mathf.Lerp(0.1f, 2, lerpTimeEnding * 3), 20 );
		}
    }

    protected override void OnPick() {
		base.OnPick();

		if( !LevelManager.GetPlayer().HasLastWish() )
			LevelManager.GetPlayer().SetLastWish( this );
    }

	public void Cancel() {
		base.OnDespawn();

		// Supprimer la référence dans le joueur
		LevelManager.GetPlayer().SetLastWish( null );
	}

    protected override void OnDespawn() {
        base.OnDespawn();

        if( effectOnGoing ) {
			if( timeToLive <= 0 ) {
				// Tuer le joueur, vraiment.
				LevelManager.Kill( LevelManager.GetPlayer() );
			}

			// Supprimer la référence dans le joueur
			LevelManager.GetPlayer().SetLastWish( null );
		}
	}
	
	private void Effect() {
        effectOnGoing = true;

		timeToLive += despawnTime; // Car on compte le despawnTime dans le temps complet, à cause de l'animation particulière de ce pickup

        // C'est pour l'instant le seul moyen que j'ai trouvé pour ne pas rester dans la position de la mort (qui peut être bloquante)
        // TODO: remplacer ou améliorer avec une animation ?
		playerTransform.position = LevelManager.levelManager.currentCheckPoint.transform.position;

		// Effet visuel au moment d'activer l'objet
		myTransform.position = new Vector2 (angelStartPosition, playerTransform.position.y);
		myTransform.parent = LevelManager.levelManager.transform; // Pour permettre à l'objet de suivre le joueur

		myAnim.SetBool("picked", false);
		myAnim.SetBool("actif", true);

		soundSource.volume = 1;

		// Tu voles
		LevelManager.GetPlayer().Fly(); // Pour que le joueur vole immédiatement
		flyPickup.gameObject.SetActive (true);
		//flyPickup.transform.position = playerTransform.position + Vector3.right * 0.25f; // On donne au joueur un pickup fly personnalisé pour ce mode
		flyPickup.transform.parent = playerTransform;
		flyPickup.transform.localPosition = Vector2.zero;
		flyPickup.transform.parent = null; // Quelques lignes assez étrange, mais l'autre solution ne fonctionnait pas systématiqueement

		// T'es invul
		LevelManager.GetPlayer().SetInvincible( lifeTime );
    }

	// Effet visuel au moment où on ramasse l'item
	protected override void PickEffect() {
		base.PickEffect();

		myTransform.position = playerTransform.position + Vector3.up * 10 / 16f;
	}

	protected override void DespawnEffect() {
		// On s'assure de ne pas déclencher le Despawn d'un pickup qu'on vient de ramasser en plus de celui qu'on a déjà
		if (LevelManager.GetPlayer().GetLastWish () != this)
			return;

		effectEnding = true;

		LevelManager.GetPlayer().Die (); // Le joueur est mort au début de l'effet, on ne peut pas utiliser d'animation pour cela
		LevelManager.GetPlayer().SetFireAbility (false); // Il n'est pas encore vraiment mort, donc il faut l'empêcher de tirer à ce moment

		// On désactive ses colliders pour éviter les obstacles quand il remonte
		Collider2D[] playerCollider = LevelManager.GetPlayer().GetComponentsInChildren<Collider2D> ();
		foreach (Collider2D col in playerCollider)
			col.enabled = false;

		// On réattribue le pickup au joueur
		myTransform.parent = playerTransform;
		myTransform.position = playerTransform.position + Vector3.up * 11 / 16f; // Léger décalage de 16 pixels

		lerpTimeEnding = 0f;
		deathPlayerPosition = playerTransform.position.y;
		endingPlayerPosition = Camera.main.orthographicSize + Camera.main.GetComponent<CameraManager> ().yOffset + 1; // Pour etre au-dessus

		// Rayon divin
		// Il change de parent pour ne pas bouger
		divineMesh.transform.parent = LevelManager.levelManager.transform;

		divineMesh.transform.position = new Vector2 (divineMesh.transform.position.x, 5);
		divineMesh.transform.localScale = new Vector2 (0.1f, 20); // On affiche une mince ligne au début
		divineMesh.SetActive (true);

		// On supprime tous les pickups potentiels, pour que ce soit plus beau...
		Pickup[] pickups = LevelManager.GetPlayer().GetComponentsInChildren<Pickup> ();
		foreach (Pickup pickup in pickups) {
			if (pickup != this) {
				pickup.Disable ();
			}
		}
	}
}
