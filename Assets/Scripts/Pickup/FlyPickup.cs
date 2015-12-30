using UnityEngine;

public class FlyPickup : Pickup {

	private PlayerController player;

    public LayerMask groundMask;

    protected override void Awake() {
		base.Awake();

		parentAttach = true;
	}

	void Start() {
		player = LevelManager.GetPlayer ();
	}

	protected override void OnPick() {
		base.OnPick();

        player.Fly();
	}

	protected override void OnDespawn() {
		base.OnDespawn();

		if (!player.IsDead ())
			player.Land ();
    }
}
