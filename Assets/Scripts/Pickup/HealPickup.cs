using UnityEngine;

public class HealPickup : Pickup {

	public int heal;

	private Animator frontAnim;
	private Animator backAnim;

	protected override void Awake() {
		base.Awake();
		
		parentAttach = true;
		despawnTime = 1.5f;

		frontAnim = transform.Find ("Heal_Front").GetComponent<Animator> ();
		backAnim = transform.Find ("Heal_Back").GetComponent<Animator> ();
	}

    protected override void OnPick() {
		base.OnPick();

		// Ajouter la vie au joueur
        LevelManager.GetPlayer().SetHealthPoint( LevelManager.GetPlayer().GetHealthPoint() + heal );
    }

	protected override void PickEffect() {
		base.PickEffect ();

		if ( null != backAnim && null != frontAnim ) {
			if (_StaticFunction.ExistsAndHasParameter ("picked", backAnim))
				backAnim.SetBool ("picked", true);
			if (_StaticFunction.ExistsAndHasParameter ("picked", frontAnim))
				frontAnim.SetBool ("picked", true);
		}

		frontAnim.transform.localPosition = Vector2.zero;
		backAnim.transform.localPosition = Vector2.zero;
	}
}
