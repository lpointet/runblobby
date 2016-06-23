using UnityEngine;

public class HealPickup : Pickup {

	public int heal;

	private Animator backAnim;

	protected override void Awake() {
		base.Awake();
		
		parentAttach = true;
		despawnTime = 1.5f;

		backAnim = transform.Find ("Heal_Back").GetComponent<Animator> ();
	}

    protected override void OnPick() {
		base.OnPick();

		// Ajouter la vie au joueur
        LevelManager.player.healthPoint += heal;
    }

	protected override void PickEffect() {
		base.PickEffect ();

		if (_StaticFunction.ExistsAndHasParameter ("picked", backAnim))
			backAnim.SetBool ("picked", true);

		myAnim.transform.localPosition = Vector2.zero;
		backAnim.transform.localPosition = Vector2.zero;
	}
}
