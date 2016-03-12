using UnityEngine;

public class AutoCoinPickup : Pickup {

	public float radius = 0f;
	public LayerMask layerCoins;

	private float volumeMax;

	private Animator backAnim;

	private float animSpeed = 1;

	protected override void Awake() {
		base.Awake();

		parentAttach = true;
		despawnTime = 0.3f;
		weakTime = 3f;

		backAnim = transform.Find ("Magnet_Back").GetComponent<Animator> ();
	}

	protected override void PickEffect() {
		base.PickEffect ();

		if (_StaticFunction.ExistsAndHasParameter ("picked", backAnim))
			backAnim.SetBool ("picked", true);

		transform.localPosition = new Vector2(0, 28/32f); // 4 pixels sous le joueur
		myAnim.transform.localPosition = Vector2.zero;
		backAnim.transform.localPosition = Vector2.zero;
    }

	protected override void DespawnEffect() {
		backAnim.speed = 1;
		myAnim.speed = 1;

		base.DespawnEffect ();

		if (_StaticFunction.ExistsAndHasParameter ("end", backAnim))
			backAnim.SetBool ("end", true);
    }

	protected override void WeakEffect() {
		// On diminue la vitesse de la tornade légèrement vers la fin
		animSpeed = _StaticFunction.MappingScale (timeToLive, weakTime, 0, 1, 0.5f);
		backAnim.speed = animSpeed;
		myAnim.speed = animSpeed;

		soundSource.volume -= TimeManager.deltaTime / (2 * weakTime);
	}

	protected override void Update() {
		base.Update();

		if( !picked || TimeManager.paused ) {
			return;
		}

        // Attirer toutes les pièces vers le joueur
        LevelManager.GetPlayer().AttractCoins( radius, layerCoins );
    }
}
