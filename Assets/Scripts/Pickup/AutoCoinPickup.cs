using UnityEngine;

public class AutoCoinPickup : Pickup {

	public float radius = 0f;
	public LayerMask layerCoins;

	private Collider2D[] coins = new Collider2D[20];   		// Liste des pièces existantes

	private Vector3 direction; 	 							// Vecteur entre le joueur et une pièce
	private float camHorizontalExtend;						// Offset vers le bord droit de l'écran
	private int nbCoins = 0; 								// Nombre de pièces à ramasser

	public ParticleSystem tornadoEffect;
	public ParticleSystem tornadoRayEffect;
	private ParticleSystem myTornado;
	private ParticleSystem myRay;
	private float mouvement;
	private float mouvementFinal;
	private float ralentissementMouvement;
	private AudioSource myWindSound;
	private float volumeMax;

	protected override void Awake() {
		base.Awake();
		parentAttach = true;
		despawnTime = 0.3f;
	}

	void Start() {
		camHorizontalExtend = CameraManager.cameraManager.camRightEnd;
	}

	protected override void PickEffect() {
		base.PickEffect ();
		//myTornado = Instantiate (tornadoEffect, new Vector2(myTransform.position.x, myTransform.position.y - 5), tornadoEffect.transform.rotation) as ParticleSystem;
		//myRay = Instantiate (tornadoRayEffect, new Vector2(myTransform.position.x + 3.5f, myTransform.position.y - 5), tornadoRayEffect.transform.rotation) as ParticleSystem;
		myWindSound = GetComponent<AudioSource> ();
		volumeMax = myWindSound.volume;
        LevelManager.getPlayer().GetComponent<CharacterSFX>().PlayAnimation("magnet_begin");
    }

	protected override void DespawnEffect() {
		//myRay.Stop ();
        LevelManager.getPlayer().GetComponent<CharacterSFX>().PlayAnimation("magnet_end");
    }

	protected override void Update() {
		base.Update();

		if( !picked || Time.timeScale == 0) {
			return;
		}

		// Attirer toutes les pièces vers le joueur
		AttractCoins();

		// Effet graphique de rotation de la tornade
		/*if (!finMouvement) {
			mouvement = Random.Range (2, 4) * (1 + Mathf.Sin (Time.time) / Random.Range (2, 3)); // Oscille entre 1,3 et 6 en gros.
			mouvementFinal = mouvement;
			ralentissementMouvement = mouvementFinal;
		} else {
			// Diminuer la vitesse de rotation avant la fin
			ralentissementMouvement -= Time.deltaTime / despawnTime;
			mouvement = mouvementFinal * Mathf.Sin (ralentissementMouvement / mouvementFinal);
		}
		myTornado.transform.Rotate (0, 0, mouvement); // Rotation sur l'axe Y*/

		if ( !despawnCalled ) {
			_StaticFunction.AudioFadeIn (myWindSound, volumeMax, despawnTime);
        }
        else {
            _StaticFunction.AudioFadeOut( myWindSound, 0, despawnTime );
        }
    }

    private void AttractCoins() {
		nbCoins = Physics2D.OverlapCircleNonAlloc( myTransform.position, radius, coins, layerCoins );

		for( int i = 0; i < nbCoins; i++ ) {
			if( coins[i].transform.position.x > myTransform.position.x + camHorizontalExtend ) {
				continue;
			}

            // Vérifier que le joueur n'a pas déjà pris cette pièce
            if( LevelManager.getPlayer().HasPickup( coins[i] ) ) {
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
