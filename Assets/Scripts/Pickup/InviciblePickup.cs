using UnityEngine;
using System.Collections;

public class InviciblePickup : Pickup {

	private float clignottant;
	private float coefClignottant = 0.05f;

	public LayerMask layerCoins;

	protected override void Awake() {
		base.Awake();

		parentAttach = true;
		despawnTime = 0.3f;
		weakTime = 3f;

		clignottant = Mathf.Sqrt (Mathf.Sqrt (Mathf.PI / (2f * coefClignottant)));
	}

	void Start () {
		Mediator.current.Subscribe<TouchPlayer> (AttractAllCoins);
	}
	
	protected override void OnPick() {
		base.OnPick();

		LevelManager.player.SetInvincible( lifeTime );

		// Ajout du bouclier permanent si les talents le permettent
		if (GameData.gameData.playerData.talent.shieldDef > 0)
			LevelManager.player.rotatingShield.CreateShield (Mathf.RoundToInt (GameData.gameData.playerData.talent.shieldDef * GameData.gameData.playerData.talent.shieldDefPointValue));

		// Ajout de la baisse d'armure pour le prochain boss si les talents le permettent
		if (GameData.gameData.playerData.talent.shieldAtk > 0) {
			int reduceEnemyArmor;
			reduceEnemyArmor = Mathf.RoundToInt (GameData.gameData.playerData.talent.shieldAtk * GameData.gameData.playerData.talent.shieldAtkPointValue);

			// Si on possède le LastWish, on augmente cette réduction de 1 par 2 points talentés
			if (LevelManager.player.HasLastWish ()) {
				reduceEnemyArmor -= Mathf.RoundToInt (GameData.gameData.playerData.talent.lastWishDef / 2f);
			}

			LevelManager.reduceEnemyDefense = reduceEnemyArmor;
		}
	}

	protected override void WeakEffect() {
		Color tempColor = myRender.color;
		tempColor.a = Mathf.Abs (Mathf.Sin (coefClignottant * _StaticFunction.MathPower(clignottant, 4)));
		myRender.color = tempColor;

		clignottant += TimeManager.deltaTime;
	}
	
	protected override void DespawnEffect() {
		base.DespawnEffect();

		myRender.color = new Color (myRender.color.r, myRender.color.g, myRender.color.b, 1f);
	}


	private void AttractAllCoins (TouchPlayer touch) {
		// Quand on appuye sur le joueur, on attire toutes les feuilles visibles à soi (rayon de 25, largement suffisant)
		LevelManager.player.AttractCoins( 25, layerCoins, lifeTime );

		// TODO SFX ? Genre une 'explosion'
		// TODO Est-ce que ça tue tous les objets qui peuvent être cassés également ?
	}
}
