using UnityEngine;
using System.Collections;

public class HealPickup : Pickup {

	public int heal;

    protected override void OnPick() {
		base.OnPick();

		// Ajouter la vie au joueur
        LevelManager.GetPlayer().SetHealthPoint( LevelManager.GetPlayer().GetHealthPoint() + heal );
    }

	protected override void PickEffect() {
		base.PickEffect();

		LevelManager.GetPlayer().GetComponent<CharacterSFX>().PlayAnimation("healed");
	}
}
