﻿using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour {

	public enum Types {
		All,
		Coin,
		Experience
	};
	
	public struct Multiplier {
		public int value;
		public float endTime;
	}
	
	public static Dictionary<Types, Multiplier> multipliers = new Dictionary<Types, Multiplier>();

	private Text scoreText;

	private static int score; // Score de la session - PlayerData pour le score total
	private static int experience; // Expérience de la session - PlayerData pour l'exp totale

	private static int leaf; // Le nombre de pièces obtenues
	private static int leafMax; // Le nombre maximum de pièces obtenables

	public static float GetRatioLeaf() {
		return leafMax == 0 ? 1 : (float)leaf / leafMax;
	}

	public static int GetScore() {
		return score;
	}

	public static int GetExperience() {
		return experience;
	}
	
	void Awake () {
		scoreText = GetComponent<Text> ();
	}

	void OnGUI () {
		scoreText.text = score.ToString ();
	}

	public static void AddPoint(int numberPoint, Types type) {
		if (LevelManager.GetPlayer ().IsDead ()) // Empêcher de récupérer des sous/xp après la mort
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
			score += numberPoint;
			// On compte une pièce supplémentaire
			leaf++;
		} else if (Types.Experience == type) {
			experience += numberPoint;
		}
	}
	
	public static void AddMultiplier( int multiplier, Types type, float lifeTime ) {
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

	public static void AddLeaf(int number) {
		leafMax += number;
	}

	public static void Reset() {
		score = 0;
		experience = 0;
		leaf = 0;
		leafMax = 0;
	}
}