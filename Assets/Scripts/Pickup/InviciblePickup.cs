using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class InviciblePickup : Pickup {

	private float clignottant = 0;
	private float coefClignottant = 5f;

	public LayerMask layerCoins;

	private AudioMixer myMusicMixer;
	private AudioMixer mySfxMixer;
	private float musicCutoffDelta = 3500;
	private float sfxCutoffDela = 3000;

	protected override void Awake() {
		base.Awake();

		myMusicMixer = LevelManager.levelManager.GetComponent<AudioSource> ().outputAudioMixerGroup.audioMixer;
		mySfxMixer = LevelManager.levelManager.GetComponentInChildren<SFXBackground> ().GetComponent<AudioSource> ().outputAudioMixerGroup.audioMixer;

		parentAttach = true;
		despawnTime = 0.3f;
		weakTime = 3f;
	}

	void Start () {
		Mediator.current.Subscribe<TouchPlayer> (AttractAllCoins);
	}
	
	protected override void OnPick() {
		base.OnPick();

		LevelManager.player.SetInvincible( lifeTime );
		clignottant = 0;

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

	protected override void PickEffect () {
		base.PickEffect ();

		AdjustLowPass (myMusicMixer, -musicCutoffDelta);
		AdjustLowPass (mySfxMixer, -sfxCutoffDela);
	}

	protected override void WeakEffect() {
		Color tempColor = mySprite.color;
		tempColor.a = Mathf.Cos (coefClignottant * clignottant * clignottant);
		mySprite.color = tempColor;

		clignottant += TimeManager.deltaTime;
	}
	
	protected override void DespawnEffect() {
		base.DespawnEffect();

		AdjustLowPass (myMusicMixer, musicCutoffDelta);
		AdjustLowPass (mySfxMixer, sfxCutoffDela);

		mySprite.color = new Color (mySprite.color.r, mySprite.color.g, mySprite.color.b, 1f);
	}


	private void AttractAllCoins (TouchPlayer touch) {
		// Quand on appuye sur le joueur, on attire toutes les feuilles visibles à soi (rayon de 25, largement suffisant)
		LevelManager.player.AttractCoins( 25, layerCoins, 0 );

		// TODO SFX ? Genre une 'explosion'
		// TODO Est-ce que ça tue tous les objets qui peuvent être cassés également ?
	}

	private void AdjustLowPass (AudioMixer audioMixer, float cutoffDelta) {
		float currentCutoff;
		float newCutoff;

		audioMixer.GetFloat ("cutoffLowPass", out currentCutoff);

		newCutoff = Mathf.Max (1500, currentCutoff + cutoffDelta); // On ne descend pas sous 1500 (pour avoir du son quand même...)

		audioMixer.SetFloat ("cutoffLowPass", newCutoff);
	}
}
