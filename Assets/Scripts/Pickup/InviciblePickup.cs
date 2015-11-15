public class InviciblePickup : Pickup {

	protected override void Awake() {
		base.Awake();

		parentAttach = true;
	}
	
	protected override void OnPick() {
		base.OnPick();

		LevelManager.GetPlayer().SetInvincible( lifeTime );
	}

	protected override void PickEffect() {
		base.PickEffect();
		
		LevelManager.GetPlayer().GetComponent<CharacterSFX>().PlayAnimation("shield_begin", false);
	}
	
	protected override void DespawnEffect() {
		LevelManager.GetPlayer().GetComponent<CharacterSFX>().PlayAnimation("shield_end", false);
	}
}
