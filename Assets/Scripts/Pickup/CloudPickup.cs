public class CloudPickup : Pickup {

	protected override void Awake() {
		base.Awake();

		parentAttach = true;
	}
	
	protected override void OnPick() {
		base.OnPick ();

		CloudBlock.nuageActif = true;
	}
	
	protected override void OnDespawn() {
        base.OnDespawn();

        CloudBlock.nuageActif = false;
	}
}
