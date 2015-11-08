public class InviciblePickup : Pickup {

	protected override void Awake() {
		base.Awake();

		parentAttach = true;
	}
	
	protected override void OnPick() {
		base.OnPick();

		LevelManager.getPlayer().SetInvincible( lifeTime );
	}

	protected override void PickEffect() {
		base.PickEffect();
		
		LevelManager.getPlayer().GetComponent<CharacterSFX>().PlayAnimation("shield_begin");
	}
	
	protected override void DespawnEffect() {
		LevelManager.getPlayer().GetComponent<CharacterSFX>().PlayAnimation("shield_end");
	}
}
