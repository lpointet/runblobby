using UnityEngine;

public class MultiplierPickup : Pickup {

	public int multiplier;
	public ScoreManager.Types type;

	public Material moneyMat;
	public Color BaseColor;
	public Color TargetColor;

	protected override void Awake() {
		base.Awake();

		parentAttach = true;
	}

	protected override void OnPick() {
		base.OnPick();

		ScoreManager.AddMultiplier( multiplier, type, lifeTime );
	}

	protected override void PickEffect() {
		base.PickEffect();

		// Cas d'un multiplicateur COIN
		if (type == ScoreManager.Types.Coin) {
			moneyMat.SetColor ("_BaseColor", BaseColor);
			moneyMat.SetColor ("_TargetColor", TargetColor);
		}

		// Cas d'un multiplicateur EXPERIENCE
		//else if (type == ScoreManager.Types.Experience)
		
	}

	protected override void OnDespawn() {
		base.OnDespawn();

		ScoreManager.MaybeRemoveMultiplier( type );
	}

	protected override void DespawnEffect() {
		base.DespawnEffect();

		// Cas d'un multiplicateur COIN
		if (type == ScoreManager.Types.Coin)
			moneyMat.SetColor ("_TargetColor", BaseColor);
	}
}
