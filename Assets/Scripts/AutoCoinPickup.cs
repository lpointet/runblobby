using UnityEngine;
using System.Collections;

public class AutoCoinPickup : Pickup {

	public float radius = 0f;
	public LayerMask layerCoins;

	private Collider2D[] coins = new Collider2D[20];   		// Liste des pièces existantes

	private Transform myTransform; 							// Référence vers le transform du bonus
	private Vector3 direction; 	 							// Vecteur entre le joueur et une pièce
	private Transform initialParent;						// Référence vers le parent initial
	private Camera cam; 									// Référence vers la caméra
	private float camHorizontalExtend;						// Offset vers le bord droit de l'écran
	private int nbCoins = 0; 								// Nombre de pièces à ramasser

	public ParticleSystem tornadoEffect;
	public ParticleSystem tornadoRayEffect;
	private ParticleSystem myTornado;
	private ParticleSystem myRay;
	private float mouvement;
	private AudioSource myWindSound;
	private float volumeMax;

	protected override void Awake() {
		base.Awake();
		myTransform = transform;
		initialParent = myTransform.parent;
		cam = Camera.main;
		despawnTime = 2f;
	}

	void Start() {
		camHorizontalExtend = cam.transform.position.x + cam.orthographicSize * cam.aspect;
	}
	
	protected override void OnPick() {
		// Attacher le bonus au joueur
		myTransform.parent = LevelManager.getPlayer().transform;
		myTransform.position = LevelManager.getPlayer ().transform.position;
	}

	protected override void PickEffect() {
		Hide();
		myTornado = Instantiate (tornadoEffect, new Vector2(myTransform.position.x, myTransform.position.y - 5), tornadoEffect.transform.rotation) as ParticleSystem;
		myRay = Instantiate (tornadoRayEffect, new Vector2(myTransform.position.x + 3.5f, myTransform.position.y - 5), tornadoRayEffect.transform.rotation) as ParticleSystem;
		myWindSound = myRay.GetComponent<AudioSource> ();
		volumeMax = myWindSound.volume;
	}

	protected override void DespawnEffect() {
		_StaticFunction.AudioFadeOut (myWindSound, 0, 2);
		myRay.Stop ();
	}
	
	protected override void OnDespawn() {
		// Attacher le bonus à son parent initial
		myTransform.parent = initialParent;
		myTornado.Stop ();
	}

	protected override void Update() {
		base.Update();

		if( !picked ) {
			return;
		}

		mouvement = Random.Range(2, 4) * (1 + Mathf.Sin (Time.time) / Random.Range(2, 3)); // Oscille entre 1,3 et 6 en gros.
		myTornado.transform.Rotate (0, 0, mouvement); // Rotation sur l'axe Y

		if (timeToLive > lifeTime - 2)
			_StaticFunction.AudioFadeIn (myWindSound, volumeMax, 2);
	}

	void FixedUpdate() {
		if( !picked ) {
			return;
		}

		// Attirer toutes les pièces vers le joueur
		AttractCoins();
	}

	private void AttractCoins() {
		nbCoins = Physics2D.OverlapCircleNonAlloc( myTransform.position, radius, coins, layerCoins );

		for( int i = 0; i < nbCoins; i++ ) {
			if( coins[i].transform.position.x > myTransform.position.x + camHorizontalExtend ) {
				continue;
			}

			// Le vecteur direction nous donne la droite entre la pièce et le bonus, donc le joueur
			direction = coins[i].transform.position - myTransform.position;

			// Faire venir la pièce vers le joueur
			// Vitesse inversement proportionelle à la distance, minimum 0.5
			coins[i].transform.Translate(Mathf.Min (0.5f, 1 / direction.magnitude) * -direction.normalized);
		}
	}
}
