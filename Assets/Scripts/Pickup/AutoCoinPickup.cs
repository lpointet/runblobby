using UnityEngine;

public class AutoCoinPickup : Pickup {

	public float radius = 0f;
	private float poweredRadius = 0f;
	public LayerMask layerCoins;

	private float volumeMax;
	private float animSpeed = 1;

	private Animator backAnim;

	private bool forcedPick = false;

	protected override void Awake() {
		base.Awake();

		parentAttach = true;
		despawnTime = 0.3f;
		weakTime = 3f;
		poweredRadius = radius * (100 + GameData.gameData.playerData.talent.tornadoDef * GameData.gameData.playerData.talent.tornadoDefPointValue) / 100f;

		if (transform.Find ("Magnet_Back") != null)
			backAnim = transform.Find ("Magnet_Back").GetComponent<Animator> ();
	}

	public void ForceOnPick () {
		forcedPick = true;
		OnPick ();
	}

	public void ForceOnDespawn () {
		OnDespawn ();
	}

	protected override void OnPick() {
		base.OnPick ();

		// On ne change pas le parent
		if (forcedPick)
			myTransform.parent = initialParent;

		// Prise en compte du talent de la tornade
		radius = poweredRadius;

		// Prise en compte du talent du Lastwish s'il est possédé
		if (LevelManager.player.HasLastWish ()) {
			radius *= 1 + GameData.gameData.playerData.talent.lastWishDef * GameData.gameData.playerData.talent.lastWishDefPointValue / 100f;
		}

		// Attirer toutes les pièces vers le joueur
		LevelManager.player.AttractCoins( radius, layerCoins, lifeTime );
		LevelManager.player.activeAttract = true;
	}

	protected override void PickEffect() {
		base.PickEffect ();

		if (_StaticFunction.ExistsAndHasParameter ("picked", backAnim))
			backAnim.SetBool ("picked", true);

		transform.localPosition = new Vector2(0, 28/32f); // 4 pixels sous le joueur

		// Si on est dans le cas d'un "vrai" pickup (il n'y a pas l'animation s'il accompagne le vol)
		if (myAnim != null) {
			myAnim.transform.localPosition = Vector2.zero;
			backAnim.transform.localPosition = Vector2.zero;

			LevelManager.player.canBreakByClick = Mathf.RoundToInt (GameData.gameData.playerData.talent.tornadoAtk * GameData.gameData.playerData.talent.tornadoAtkPointValue);
		}
    }

	protected override void OnDespawn() {
		base.OnDespawn ();

		LevelManager.player.activeAttract = false;
		forcedPick = false;
	}

	protected override void DespawnEffect() {
		if (myAnim != null) {
			myAnim.speed = 1;
			backAnim.speed = 1;

			LevelManager.player.canBreakByClick = 0;
		}

		base.DespawnEffect ();

		if (_StaticFunction.ExistsAndHasParameter ("end", backAnim))
			backAnim.SetBool ("end", true);
    }

	protected override void WeakEffect() {
		// On diminue la vitesse de la tornade légèrement vers la fin
		if (myAnim != null) {
			animSpeed = _StaticFunction.MappingScale (timeToLive, weakTime, 0, 1, 0.5f);
			myAnim.speed = animSpeed;
			backAnim.speed = animSpeed;
		}

		if (soundSource != null)
			soundSource.volume -= TimeManager.deltaTime / (2 * weakTime);
	}

	void OnDrawGizmos() {
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(transform.position, radius);
	}
}
