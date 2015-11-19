using UnityEngine;

public class AutoCoinPickup : Pickup {

	public float radius = 0f;
	public LayerMask layerCoins;

	/*public ParticleSystem tornadoEffect;
	public ParticleSystem tornadoRayEffect;
	private ParticleSystem myTornado;
	private ParticleSystem myRay;
	private float mouvement;
	private float mouvementFinal;
	private float ralentissementMouvement;*/
	private AudioSource myWindSound;
	private float volumeMax;

	private Animator backAnim;

	protected override void Awake() {
		base.Awake();

		parentAttach = true;
		despawnTime = 0.3f;

		backAnim = transform.Find ("Magnet_Back").GetComponent<Animator> ();
	}

	protected override void PickEffect() {
		base.PickEffect ();
		//myTornado = Instantiate (tornadoEffect, new Vector2(myTransform.position.x, myTransform.position.y - 5), tornadoEffect.transform.rotation) as ParticleSystem;
		//myRay = Instantiate (tornadoRayEffect, new Vector2(myTransform.position.x + 3.5f, myTransform.position.y - 5), tornadoRayEffect.transform.rotation) as ParticleSystem;
		myWindSound = GetComponent<AudioSource> ();
		volumeMax = myWindSound.volume;
        //LevelManager.GetPlayer().GetComponent<CharacterSFX>().PlayAnimation("magnet_begin");

		if (_StaticFunction.ExistsAndHasParameter ("picked", backAnim))
			backAnim.SetBool("picked", true);

		transform.localPosition = new Vector2(0, 0.875f); // 4 pixels sous le joueur
    }

	protected override void DespawnEffect() {
		base.DespawnEffect ();
		//myRay.Stop ();
        //LevelManager.GetPlayer().GetComponent<CharacterSFX>().PlayAnimation("magnet_end");
		if (_StaticFunction.ExistsAndHasParameter ("end", backAnim)) // On cache directement ceux qui n'ont pas d'animation de fin
			backAnim.SetBool ("end", true);
    }

	protected override void Update() {
		base.Update();

		if( !picked || Time.timeScale == 0) {
			return;
		}

        // Attirer toutes les pièces vers le joueur
        LevelManager.GetPlayer().AttractCoins( radius, layerCoins );

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
}
