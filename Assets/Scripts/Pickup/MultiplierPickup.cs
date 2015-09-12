public class MultiplierPickup : Pickup {

	public int multiplier;
	public ScoreManager.Types type;

	protected override void OnPick() {
		base.OnPick();

		ScoreManager.AddMultiplier( multiplier, type, lifeTime );
	}

	protected override void OnDespawn() {
		base.OnDespawn();

		ScoreManager.MaybeRemoveMultiplier( type );
	}

}
