public class InviciblePickup : Pickup {

	protected override void Awake() {
		base.Awake();

		parentAttach = true;
	}
	
	protected override void OnPick() {
		base.OnPick();

		LevelManager.getPlayer().SetInvincible( lifeTime );
	}
}
