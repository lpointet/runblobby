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

		int realheal = heal;

		// Ajout des points de vie bonus du talent
		if (GameData.gameData.playerData.talent.healDef > 0)
			realheal += GameData.gameData.playerData.talent.healDef * (int)GameData.gameData.playerData.talent.healDefPointValue;

		// Ajouter la vie au joueur
		LevelManager.player.healthPoint += realheal;
		StartCoroutine (UIManager.uiManager.CombatText (myTransform, realheal.ToString("+0"), LogType.heal));

		// Ajout de la baisse de vie pour le prochain boss si les talents le permettent
		if (GameData.gameData.playerData.talent.healAtk > 0)
			LevelManager.reduceEnemyHealth = Mathf.RoundToInt (GameData.gameData.playerData.talent.healAtk * GameData.gameData.playerData.talent.healAtkPointValue);
    }

	protected override void PickEffect() {
		base.PickEffect ();

		if (_StaticFunction.ExistsAndHasParameter ("picked", backAnim))
			backAnim.SetBool ("picked", true);

		myAnim.transform.localPosition = Vector2.zero;
		backAnim.transform.localPosition = Vector2.zero;
	}
}
