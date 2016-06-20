using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Collections;

public static class _StaticFunction {

	/**
	 * PARTIE MATH
	 */
	// Loi de Poisson (avec k entier) : P(X=k, lambda) = (lambda^k * e^-lambda) / k!
	public static float LoiPoisson(int variable, float esperance, bool cumule = false) {
		if (esperance < 0) // L'esperance ne peut être négative
			esperance = 0;
		
		float lambdaPowered = Mathf.Exp (-esperance);

		if (!cumule) { // On souhaite la valeur discrête
			return (MathPower (esperance, variable) * lambdaPowered) / Factorial (variable);
		} else { // On souhaite les valeurs cumulées
			float sumCumule = 0;
			// On somme les lambda^k / k!
			for (int i = variable; i >= 0; i--) {
				sumCumule += MathPower (esperance, i) / Factorial (i);
			}
			// On multiplie le tout par e^lambda
			return sumCumule * lambdaPowered;
		}
	}

	private static long Factorial(long value) {
		if (value <= 1)
			return 1;
		return value * Factorial (--value);
	}

	public static float MathPower(float number, int exposant) {
		float result = 1.0f;

		while (exposant > 0)
		{
			if (exposant % 2 == 1)
				result *= number;
			exposant >>= 1;
			number *= number;
		}

		return result;
	}
	/**
	 * FIN PARTIE MATH
	 */

	/** 
	 * PARTIE SAUVEGARDE
	 */ 
	public static void Save() {
		BinaryFormatter bf = new BinaryFormatter();
		FileStream file = File.Create (_GameData.saveFile);

		TraitementData ();

		bf.Serialize (file, GameData.gameData);
		file.Close ();

		Debug.Log ("Save successful in: " + _GameData.saveFile);
	}

	public static bool Load() {
		if (File.Exists (_GameData.saveFile)) {
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = File.Open (_GameData.saveFile, FileMode.Open);

			GameData tempData = new GameData ();
			tempData = bf.Deserialize (file) as GameData;

			// Copie des données GameData si on a déjà une sauvegarde
			if (tempData.existingGame) {
				GameData.gameData.existingGame = tempData.existingGame;
				GameData.gameData.musicVolume = tempData.musicVolume;
				GameData.gameData.sfxVolume = tempData.sfxVolume;
				GameData.gameData.firstLevel = tempData.firstLevel;
				GameData.gameData.lastLevel = tempData.lastLevel;

				// Copie des données PlayerData
				GameData.gameData.playerData.name = tempData.playerData.name;
				GameData.gameData.playerData.isStory = tempData.playerData.isStory;
				GameData.gameData.playerData.currentLevel = tempData.playerData.currentLevel;
				GameData.gameData.playerData.currentDifficulty = tempData.playerData.currentDifficulty;
				GameData.gameData.playerData.experience = tempData.playerData.experience;
				GameData.gameData.playerData.level = tempData.playerData.level;
				GameData.gameData.playerData.distanceTotal = tempData.playerData.distanceTotal;
				GameData.gameData.playerData.enemyKilled = tempData.playerData.enemyKilled;
				GameData.gameData.playerData.numberOfDeath = tempData.playerData.numberOfDeath;

				// Permet d'ajouter des levels si jamais certains sont nouveaux depuis la sauvegarde
				// On n'efface pas de GameData.gameData ceux qui sont supérieurs à tempData.playerData.levelData.Count
				// On n'ajoute pas des levels à GameData.gameData si ceux-ci n'existent plus
				for (int i = 0; i < Mathf.Min (GameData.gameData.playerData.levelData.Count, tempData.playerData.levelData.Count); i++) {
					if (GameData.gameData.playerData.levelData [i] != null)
						GameData.gameData.playerData.levelData [i] = tempData.playerData.levelData [i];
				}

				Debug.Log ("Load successful from: " + _GameData.saveFile);
				return true;
			} else
				return false;
		} else
			return false;
	}

	public static bool Erase() {
		if (File.Exists (_GameData.saveFile)) {

			// Suppression de l'ancienne sauvegarde
			File.Delete (_GameData.saveFile);

			// Création d'une nouvelle sauvegarde vierge (pour retenir les paramètres généraux)
			Save ();

			return true;
		} else
			return false;
	}

	private static void TraitementData() {
		GameData.gameData.existingGame = true;

		GameData.gameData.playerData.experience += ScoreManager.GetExperience ();
		GameData.gameData.playerData.level = LevelFromExp (GameData.gameData.playerData.experience);
		GameData.gameData.playerData.distanceTotal += LevelManager.levelManager.GetDistanceTraveled ();

		// Sauvegardes spécifiques au level
		if (LevelManager.levelManager != null) {
			LevelData levelCourant = GameData.gameData.playerData.levelData [LevelManager.levelManager.GetCurrentLevel () - 1]; // Correction de l'indice du level
			int difficulty = LevelManager.levelManager.GetCurrentDifficulty ();

			if (LevelManager.levelManager.IsStory ()) {
				levelCourant.storyData [difficulty].distanceRecord = Mathf.Max (levelCourant.storyData [difficulty].distanceRecord, LevelManager.levelManager.GetDistanceTraveled ());
			}
		}
	}

	private const float aCoeffXP = 11750;
	private const float bCoeffXP = 0.01f;
	private const float cCoeffXP = -11800;

	// Level = (1 / b) * ln ((xp - c) / a)
	public static int LevelFromExp(int experience) {
		return Mathf.FloorToInt ((1 / bCoeffXP) * Mathf.Log((experience - cCoeffXP) / aCoeffXP)); // XP Level depuis expérience
	}

	// Experience = a * exp(b * lvl) + c
	public static int ExpFromLevel(int level) {
		return Mathf.CeilToInt (aCoeffXP * Mathf.Exp(bCoeffXP * level) + cCoeffXP); // XP Expérience depuis level
	}

	/** 
	 * FIN PARTIE SAUVEGARDE
	 */ 

	// Vérifier qu'une animation existe et possède un paramètre précis
	public static bool ExistsAndHasParameter(string paramName, Animator animator)
	{
		if (animator != null) {
			foreach (AnimatorControllerParameter param in animator.parameters) {
				if (param.name == paramName)
					return true;
			}
		}
		return false;
	}

    // Fonction pour activer/désactiver tous les GameObjects dans un GameObject
    public static void SetActiveRecursively( GameObject rootObject, bool active ) {
        rootObject.SetActive(active);

        foreach (Transform childTransform in rootObject.transform) {
            if (!childTransform.gameObject.activeInHierarchy)
                SetActiveRecursively(childTransform.gameObject, active);
        }
    }

	// Mapping d'une valeur sur une échelle en fonction d'une autre échelle
	// On utilise la fonction : outCurrent = (inCurrent - inMin) * (outMax - outMin) / (inMax - inMin) + outMin
	// (inCurrent - inMin) -> décalage sur l'axe des abscisses pour que le min corresponde à 0
	// (outMax - outMin) / (inMax - inMin) -> Rapport de conversion entre les deux axes directeurs
	// + outMin -> décalage sur l'axe des ordonnées pour que le outMin soit à 0
	public static float MappingScale (float inCurrent, float inMin, float inMax, float outMin, float outMax) {
		// On évite la division par 0
		if (inMax == inMin)
			return 0;

		return (inCurrent - inMin) * (outMax - outMin) / (inMax - inMin) + outMin;
	}

	// Fonction utilisée pour augmenter le volume d'un son progressivement
	public static IEnumerator AudioFadeIn(AudioSource audio, float volumeMax = 1, float delay = 1) {
		if (audio == null || volumeMax < 0)
			yield break;
		
		if (volumeMax > 1)
			volumeMax = 1;
		
		if (delay == 0) {
			audio.volume = volumeMax;
			yield break;
		}

		if (!audio.isPlaying) { // On démarre le son au volume le plus bas
			audio.volume = 0;
			audio.Play ();
		}

		while (audio.volume < volumeMax) {
			audio.volume += TimeManager.deltaTime / delay;
			yield return null;
		}

		if (audio.volume > volumeMax) // On s'assure de ne pas aller plus haut que le volumeMax demandé
			audio.volume = volumeMax;
	}

	// Fonction utilisée pour diminuer le volume d'un son progressivement
	public static IEnumerator AudioFadeOut(AudioSource audio, float volumeMin = 0, float delay = 1) {
		if (audio == null || volumeMin > 1)
			yield break;
		
		if (volumeMin < 0)
			volumeMin = 0;

		if (delay == 0) {
			audio.volume = volumeMin;
			yield break;
		}

		while (audio.volume > volumeMin) {
			audio.volume -= TimeManager.deltaTime / delay;
			yield return null;
		}

		if (audio.volume < volumeMin) // On s'assure de ne pas aller plus bas que le volumeMin demandé
			audio.volume = volumeMin;

		if (audio.isPlaying) {
			audio.Stop (); // On stoppe le son lorsqu'on est au min demandé
		}
	}
}
