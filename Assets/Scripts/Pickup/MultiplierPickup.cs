using UnityEngine;

public class MultiplierPickup : Pickup {

	public int multiplier;
	public ScoreManager.Types type;

	public Material moneyMat;
	public Color BaseColor;
	public Color TargetColor;

	private int startLeaf = 0;
	private int initialPlayerStock = 0;

	protected override void Awake() {
		base.Awake();

		parentAttach = true;
		despawnTime = despawnSound.length;
	}

	protected override void Update () {
		base.Update ();

		if (!picked || TimeManager.paused)
			return;

		// On calcule le nombre de feuille récupérées
		LevelManager.player.valueLeafBoost = ScoreManager.GetLeaf () - startLeaf + initialPlayerStock;
	}

	protected override void OnPick() {
		base.OnPick();

		float talentBonus = 0;

		// Cas d'un multiplicateur COIN
		if (type == ScoreManager.Types.Coin) {
			// Ajout du coefficient des talents pour augmenter la force de la multiplication
			talentBonus = ScoreManager.powerLeafDouble * GameData.gameData.playerData.talent.leafDef * GameData.gameData.playerData.talent.leafDefPointValue / 100f;

			// Remise à zéro du compteur de feuilles ramassées
			startLeaf = ScoreManager.GetLeaf ();
			initialPlayerStock = LevelManager.player.valueLeafBoost;

			// Augmentation du compteur de missiles disponibles avec un buff de feuilles
			LevelManager.player.numberLeafBoost = Mathf.RoundToInt (GameData.gameData.playerData.talent.leafAtk * GameData.gameData.playerData.talent.leafAtkPointValue);
		}

		ScoreManager.AddMultiplier( multiplier + talentBonus, type, lifeTime );
	}

	protected override void PickEffect() {
		base.PickEffect();

		// Cas d'un multiplicateur COIN
		if (type == ScoreManager.Types.Coin) {
			moneyMat.SetColor ("_BaseColor", BaseColor);
			moneyMat.SetColor ("_TargetColor", TargetColor);
		}

		// Cas d'un multiplicateur EXPERIENCE
		//else if (type == ScoreManager.Types.Experience)
		
	}

	protected override void OnDespawn() {
		base.OnDespawn();

		ScoreManager.MaybeRemoveMultiplier( type );
	}

	protected override void DespawnEffect() {
		base.DespawnEffect();

		// Cas d'un multiplicateur COIN
		if (type == ScoreManager.Types.Coin)
			moneyMat.SetColor ("_TargetColor", BaseColor);
	}
}
