using UnityEngine;
using System.Collections;

public class LastWishPickup : Pickup {

    public LayerMask layerCoins;

	private Transform playerTransform;

    private bool effectOnGoing = false;
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
	private bool enemyDetected = false;

	public GameObject flyingAngel;
	private float angelRate = 1.5f;
	private float timeBeforeAngel = 0;
	private float variationAngelTime = 0.5f;
	private LastWishAngel currentAngel = null;

	public void DeclareCurrentAngel (LastWishAngel angel) {
		currentAngel = angel;
	}

	public void ClearCurrentAngel () {
		currentAngel = null;
	}

	public void AddTime (float delay) {
		//timeToLive += delay;
		StartCoroutine(SlowlyAddTime(delay));
	}

	private IEnumerator SlowlyAddTime (float delay) {
		float startTTL = timeToLive;
		float endTTL = timeToLive + delay;
		float currentLerp = 0;
		float endLerp = 0.5f;

		while (currentLerp < endLerp) {
			timeToLive = Mathf.Lerp (startTTL, endTTL, currentLerp);

			currentLerp += TimeManager.deltaTime / endLerp;
			yield return null;
		}

		if (timeToLive > endTTL)
			timeToLive = endTTL;
	}

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
		playerTransform = LevelManager.player.transform;

		angelStartPosition = Mathf.Abs(CameraManager.cameraLeftPosition) + 0.5f; // Il commence en dehors de l'écran
	}

	public void Launch() {
		LevelManager.player.Resurrect();

		StartCoroutine(Effect());
	}

	public bool IsLaunched() {
		return effectOnGoing;
	} 

    protected override void Update() {
		base.Update ();

		if (!picked || TimeManager.paused)
			return;

		// Effet commence
		if ( !effectOnGoing ) {
			// Ce pickup ne doit jamais disparaitre jusqu'à la mort du joueur
			timeToLive = lifeTime;
		}
	}

	void LateUpdate() {
		if (TimeManager.paused)
			return;

		// L'effet est lancé, mais le joueur n'est pas encore "mort"
		if (effectOnGoing && !effectEnding) {
			// Effet visuel de l'ange qui se rapproche jusqu'à la mort
			offsetYToPlayer = Mathf.SmoothDamp (myTransform.position.y, playerTransform.position.y, ref dampVelocity, ((timeToLive - despawnTime) / lifeTime) * followDelay);
			distanceToPlayer = _StaticFunction.MappingScale (timeToLive - despawnTime, lifeTime, 0, angelStartPosition, 0);

			myTransform.position = new Vector2 (playerTransform.position.x - distanceToPlayer, offsetYToPlayer);

			// On rafraichit l'enemyEnCours pour que l'arme puisse suivre l'ennemi s'il se met à jour
			if (!enemyDetected && LevelManager.levelManager.GetEnemyEnCours () != null && GameData.gameData.playerData.talent.lastWishSkill > 0) {
				Weapon playerWeapon = LevelManager.player.GetWeapon ();

				playerWeapon.autoFire = true;
				playerWeapon.GetComponent<WeaponRotation> ().followName = LevelManager.levelManager.GetEnemyEnCours ().name;
				playerWeapon.remoteBullet = true;
				playerWeapon.numberBulletRebound = 0;

				enemyDetected = true;
			}

			// Mouvement de l'ange qui passe en cas de skill amélioré
			if (GameData.gameData.playerData.talent.lastWishSkill > 0) {

				// Création d'un nouvel ange quand le temps est échu et qu'il n'y a plus d'ange
				if (TimeManager.time > timeBeforeAngel && currentAngel == null) {
					LastWishAngel newAngel = PoolingManager.current.Spawn (flyingAngel.name).GetComponent<LastWishAngel> ();

					if (newAngel != null) {
						float deltaMinWithScreen = 3f; // Distance minimale à respecter pour ne pas coller l'écran

						// On place le début de l'ange de manière aléatoire
						switch (Random.Range (0, 4)) {
						case 0: // Top
							newAngel.transform.position = new Vector2 (Random.Range (CameraManager.cameraLeftPosition + deltaMinWithScreen, CameraManager.cameraRightPosition - deltaMinWithScreen), CameraManager.cameraUpPosition + 0.5f);
							break;
						case 1: // Right
							newAngel.transform.position = new Vector2 (CameraManager.cameraRightPosition + 0.5f, Random.Range (CameraManager.cameraDownPosition + deltaMinWithScreen, CameraManager.cameraUpPosition - deltaMinWithScreen));
							break;
						case 2: // Bottom
							newAngel.transform.position = new Vector2 (Random.Range (CameraManager.cameraLeftPosition + deltaMinWithScreen, CameraManager.cameraRightPosition - deltaMinWithScreen), CameraManager.cameraDownPosition - 0.5f);
							break;
						case 3: // Left
							newAngel.transform.position = new Vector2 (CameraManager.cameraLeftPosition - 0.5f, Random.Range (CameraManager.cameraDownPosition + deltaMinWithScreen, CameraManager.cameraUpPosition - deltaMinWithScreen));
							break;
						}

						newAngel.transform.rotation = Quaternion.identity;
						newAngel.gameObject.SetActive (true);

						newAngel.StartAngel ();
						timeBeforeAngel = TimeManager.time + angelRate + Random.Range (-variationAngelTime, variationAngelTime);
					}
				}
			}
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

		if( !LevelManager.player.HasLastWish() )
			LevelManager.player.SetLastWish( this );
    }

	public void Cancel() {
		base.OnDespawn();

		// Supprimer la référence dans le joueur
		LevelManager.player.SetLastWish( null );
	}

    protected override void OnDespawn() {
        base.OnDespawn();

        if( effectOnGoing ) {
			if( timeToLive <= 0 ) {
				// Tuer le joueur, vraiment.
				LevelManager.Kill( LevelManager.player );
			}

			// Supprimer la référence dans le joueur
			LevelManager.player.SetLastWish( null );
		}
	}
	
	private IEnumerator Effect() {
		// On freine le joueur le temps qu'il aille en position
		float currentMoveSpeed = LevelManager.player.moveSpeed;
		LevelManager.player.moveSpeed = 0;

		Vector2 startPosition = LevelManager.player.transform.position;
		float currentTimer = 0;
		float delay = 0.5f;

		while (currentTimer < 1) {
			// On le déplace sur une position cohérente
			LevelManager.player.transform.position = Vector2.Lerp (startPosition, new Vector2 (0, Camera.main.orthographicSize), currentTimer);
			// On le soigne progressivement
			LevelManager.player.healthPoint = Mathf.RoundToInt (Mathf.Lerp (0, LevelManager.player.healthPointMax, currentTimer));

			currentTimer += TimeManager.deltaTime / delay;
			yield return null;
		}
		// On rétablit sa vitesse avant de partir
		LevelManager.player.moveSpeed = currentMoveSpeed;

        effectOnGoing = true;
		LevelManager.player.canCollectAngel = true;

		timeToLive += despawnTime; // Car on compte le despawnTime dans le temps complet, à cause de l'animation particulière de ce pickup
		timeBeforeAngel = TimeManager.time + angelRate + Random.Range (-variationAngelTime, variationAngelTime); // On prépare la première itération d'apparition des anges

		// Effets visuel et audio au moment d'activer l'objet
		SFXEffect ();

		// T'es invul
		LevelManager.player.SetInvincible( lifeTime );

		// Tu voles
		LevelManager.player.Fly (); // Pour que le joueur vole immédiatement
		flyPickup.gameObject.SetActive (true);
		//flyPickup.transform.position = playerTransform.position + Vector3.right * 0.25f; // On donne au joueur un pickup fly personnalisé pour ce mode
		flyPickup.transform.parent = playerTransform;
		flyPickup.transform.localPosition = Vector2.zero;
		flyPickup.transform.parent = null; // Quelques lignes assez étrange, mais l'autre solution ne fonctionnait pas systématiquement

		// On force à voler en ligne droite
		LevelManager.player.SetZeroGravFlying (true);
		// On désactive les actions du joueur
		LevelManager.player.SwitchTouch(false);

		/*if (GameData.gameData.playerData.talent.lastWishSkill > 0)
			UltimateEffect ();*/
    }

	// Effet dans le cas où le talent est amélioré dans l'arbre de talents
	private void UltimateEffect () {
		// On force à voler en ligne droite
		LevelManager.player.SetZeroGravFlying (true);
		// On désactive les actions du joueur
		LevelManager.player.SwitchTouch(false);
	}

	private void SFXEffect () {
		myTransform.position = new Vector2 (angelStartPosition, playerTransform.position.y);
		myTransform.parent = LevelManager.levelManager.transform; // Pour permettre à l'objet de suivre le joueur

		myAnim.SetBool("picked", false);
		myAnim.SetBool("actif", true);

		soundSource.volume = 1;
	}

	// Effet visuel au moment où on ramasse l'item
	protected override void PickEffect() {
		base.PickEffect();

		myTransform.position = playerTransform.position + Vector3.up * 10 / 16f;
	}

	protected override void DespawnEffect() {
		// On s'assure de ne pas déclencher le Despawn d'un pickup qu'on vient de ramasser en plus de celui qu'on a déjà
		if (LevelManager.player.GetLastWish () != this)
			return;

		effectEnding = true;

		LevelManager.player.Die (); // Le joueur est mort au début de l'effet, on ne peut pas utiliser d'animation pour cela
		LevelManager.player.SetFireAbility (false); // Il n'est pas encore vraiment mort, donc il faut l'empêcher de tirer à ce moment

		// On désactive ses colliders pour éviter les obstacles quand il remonte
		Collider2D[] playerCollider = LevelManager.player.GetComponentsInChildren<Collider2D> ();
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
		Pickup[] pickups = LevelManager.player.GetComponentsInChildren<Pickup> ();
		foreach (Pickup pickup in pickups) {
			if (pickup != this) {
				pickup.Disable ();
			}
		}
	}
}
