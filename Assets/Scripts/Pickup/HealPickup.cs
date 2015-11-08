using UnityEngine;
using System.Collections;

public class HealPickup : Pickup {

	public int heal;

    protected override void OnPick() {
		base.OnPick();

		// Ajouter la vie au joueur
        LevelManager.getPlayer().SetHealthPoint( LevelManager.getPlayer().GetHealthPoint() + heal );
    }

	protected override void PickEffect() {
		base.PickEffect();

		LevelManager.getPlayer().GetComponent<CharacterSFX>().PlayAnimation("healed");
	}
}
