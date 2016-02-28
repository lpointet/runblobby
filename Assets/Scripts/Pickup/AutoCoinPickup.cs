using UnityEngine;

public class AutoCoinPickup : Pickup {

	public float radius = 0f;
	public LayerMask layerCoins;

	private float volumeMax;

	private Animator frontAnim;
	private Animator backAnim;

	protected override void Awake() {
		base.Awake();

		parentAttach = true;
		despawnTime = 0.3f;

		frontAnim = transform.Find ("Magnet_Front").GetComponent<Animator> ();
		backAnim = transform.Find ("Magnet_Back").GetComponent<Animator> ();
	}

	protected override void PickEffect() {
		volumeMax = soundSource.volume;

		if ( null != backAnim && null != frontAnim ) {
			if (_StaticFunction.ExistsAndHasParameter ("picked", backAnim)) {
				backAnim.SetBool ("picked", true);
				frontAnim.SetBool ("picked", true);
			}
		}

		transform.localPosition = new Vector2(0, 28/32f); // 4 pixels sous le joueur
		frontAnim.transform.localPosition = Vector2.zero;
		backAnim.transform.localPosition = Vector2.zero;
    }

	protected override void DespawnEffect() {
		if (_StaticFunction.ExistsAndHasParameter ("end", backAnim)) // On cache directement ceux qui n'ont pas d'animation de fin
			backAnim.SetBool ("end", true);
		if (_StaticFunction.ExistsAndHasParameter ("end", frontAnim)) // On cache directement ceux qui n'ont pas d'animation de fin
			frontAnim.SetBool ("end", true);
    }

	protected override void Update() {
		base.Update();

		if( !picked || Time.timeScale == 0) {
			return;
		}

        // Attirer toutes les pièces vers le joueur
        LevelManager.GetPlayer().AttractCoins( radius, layerCoins );

		if ( !despawnCalled ) {
			_StaticFunction.AudioFadeIn (soundSource, volumeMax, despawnTime);
        }
        else {
			_StaticFunction.AudioFadeOut( soundSource, 0, despawnTime * 5 );
        }
    }
}
