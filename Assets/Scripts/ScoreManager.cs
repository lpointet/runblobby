using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour {

	public enum Types {
		All,
		Coin,
		Experience
	};
	
	public struct Multiplier {
		public float value;
		public float endTime;
	}
	
	public static Dictionary<Types, Multiplier> multipliers = new Dictionary<Types, Multiplier>();

	private Text scoreText;

	private static float score; // Score de la session - PlayerData pour le score total
	private static float bonusScore; // Score bonus obtenu à la fin d'un niveau
	private static float finalScore; // Somme des deux scores "normal" et bonus

	private int initialLeafDouble; // Probabilité de doubler les points d'une feuille - Issu de PlayerData
	public static int leafDouble; // Chance de doubler les points d'une feuille - Peut être modifié en jeu
	public static float powerLeafDouble = 2f; // Puissance de multiplication du doublage de point (static car identique pour toutes les feuilles)

	private static int leaf; // Le nombre de feuilles courantes
	private static int leafMax; // Le nombre maximum de feuilles obtenables

	private static float experience; // Expérience de la session - PlayerData pour l'exp totale

	public static float GetRatioLeaf() {
		return leafMax == 0 ? 1 : Mathf.Clamp01 ((float)leaf / leafMax);
	}

	public static int GetScore() {
		return Mathf.RoundToInt (score);
	}

	public static int GetBonusScore() {
		return Mathf.RoundToInt (score * GameData.gameData.playerData.talent.leafBonus * GameData.gameData.playerData.talent.leafBonusPointValue / 100f);
	}

	public static int GetFinalScore() {
		return GetScore () + GetBonusScore (); 
	}

	public static int GetExperience() {
		return Mathf.RoundToInt (experience);
	}

	public static int GetLeaf() {
		return leaf;
	}

	public static void AddLeaf(int number) {
		leafMax += number;
	}

	
	void Awake () {
		scoreText = GetComponent<Text> ();

		initialLeafDouble = GameData.gameData.playerData.talent.leafDouble * (int)GameData.gameData.playerData.talent.leafDoublePointValue;
		leafDouble = initialLeafDouble;
	}

	void OnGUI () {
		scoreText.text = GetScore().ToString ();
	}

	public static void AddPoint(float numberPoint, Types type) {
		if (LevelManager.player.IsDead ()) // Empêcher de récupérer des sous/xp après la mort
			return;

		Multiplier multiplier;

		// On multiplie s'il y a lieu
		if( multipliers.TryGetValue( type, out multiplier ) ) {
			numberPoint *= multiplier.value;
		}
		else if( Types.All != type && multipliers.TryGetValue( Types.All, out multiplier ) ) {
			numberPoint *= multiplier.value;
		}

		// On ajoute les points au bon endroit
		if (Types.Coin == type) {
			// Tentative de multiplication des points par rapport aux talents
			if (Random.Range (0, 100) < leafDouble) {
				numberPoint *= powerLeafDouble;
			}
			score += numberPoint;
			// On compte une pièce supplémentaire
			leaf++;
		} else if (Types.Experience == type) {
			experience += numberPoint;
		}
	}
	
	public static void AddMultiplier( float multiplier, Types type, float lifeTime ) {
		Multiplier initial;
		Multiplier newMult;
		
		newMult.value = multiplier;
		newMult.endTime = TimeManager.time + lifeTime;
		
		if( multipliers.TryGetValue( type, out initial ) ) {
			newMult.value = Mathf.Max( multiplier, initial.value );
			multipliers.Remove( type );
		}
		
		multipliers.Add( type, newMult );
	}
	
	public static void MaybeRemoveMultiplier( Types type = Types.All ) {
		Multiplier initial;

		if( multipliers.TryGetValue( type, out initial ) ) {
			if( initial.endTime <= TimeManager.time + 0.05f ) {
				multipliers.Remove( type );
			}
		}
	}
		
	public static void Reset() {
		PlayerData playerData = GameData.gameData.playerData;
		score = Mathf.RoundToInt (playerData.leaf * (playerData.talent.leafLoss * playerData.talent.leafLossPointValue) / 100f);
		experience = 0;
		leaf = 0;
		leafMax = 0;
	}
}