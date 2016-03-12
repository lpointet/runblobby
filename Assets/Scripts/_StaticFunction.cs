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

			// Copie des données GameData
			if (tempData.existingGame != null) GameData.gameData.existingGame = tempData.existingGame;
			if (tempData.musicVolume != null) GameData.gameData.musicVolume = tempData.musicVolume;
			if (tempData.sfxVolume != null) GameData.gameData.sfxVolume = tempData.sfxVolume;
			if (tempData.firstLevel != null) GameData.gameData.firstLevel = tempData.firstLevel;
			if (tempData.lastLevel != null) GameData.gameData.lastLevel = tempData.lastLevel;


			// Copie des données PlayerData
			if (tempData.playerData.name != null) GameData.gameData.playerData.name = tempData.playerData.name;
			if (tempData.playerData.isStory != null) GameData.gameData.playerData.isStory = tempData.playerData.isStory;
			if (tempData.playerData.currentLevel != null) GameData.gameData.playerData.currentLevel = tempData.playerData.currentLevel;
			if (tempData.playerData.experience != null) GameData.gameData.playerData.experience = tempData.playerData.experience;
			if (tempData.playerData.level != null) GameData.gameData.playerData.level = tempData.playerData.level;
			if (tempData.playerData.distanceTotal != null) GameData.gameData.playerData.distanceTotal = tempData.playerData.distanceTotal;
			if (tempData.playerData.enemyKilled != null) GameData.gameData.playerData.enemyKilled = tempData.playerData.enemyKilled;

			// Permet d'ajouter des levels si jamais certains sont nouveaux depuis la sauvegarde
			// On n'efface pas de GameData.gameData ceux qui sont supérieurs à tempData.playerData.levelData.Count
			// On n'ajoute pas des levels à GameData.gameData si ceux-ci n'existent plus
			for (int i = 0; i < Mathf.Min(GameData.gameData.playerData.levelData.Count, tempData.playerData.levelData.Count); i++) {
				if (GameData.gameData.playerData.levelData [i] != null)
					GameData.gameData.playerData.levelData [i] = tempData.playerData.levelData [i];
			}

			Debug.Log ("Load successful from: " + _GameData.saveFile);
			return true;
		} else
			return false;
	}

	private static void TraitementData() {
		GameData.gameData.existingGame = true;

		GameData.gameData.playerData.experience += ScoreManager.GetExperience ();
		GameData.gameData.playerData.level = LevelFromExp (GameData.gameData.playerData.experience);

		// Sauvegardes spécifiques au level
		if (LevelManager.levelManager != null) {
			LevelData levelCourant = GameData.gameData.playerData.levelData [LevelManager.levelManager.GetCurrentLevel () - 1]; // Correction de l'indice du level
			int difficulty = LevelManager.levelManager.GetCurrentDifficulty ();

			if (LevelManager.levelManager.IsStory ()) {
				levelCourant.storyData [difficulty].distanceRecord = Mathf.Max (levelCourant.storyData [difficulty].distanceRecord, LevelManager.levelManager.GetDistanceTraveled ());
			}
		}
	}

	public static int LevelFromExp(int experience) {
		return Mathf.FloorToInt (experience / 100.0f); // TODO adapter la règle en fonction de l'xp
	}

	public static int ExpFromLevel(int level) {
		return Mathf.FloorToInt (level * 100.0f); // TODO adapter la règle en fonction de l'xp
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

	// Renvoie une Color32 à aprtir d'un code HEX, avec une opacité maximale
	public static Color32 ToColor(int HexVal)
	{
		byte R = (byte)((HexVal >> 16) & 0xFF);
		byte G = (byte)((HexVal >> 8) & 0xFF);
		byte B = (byte)((HexVal) & 0xFF);
		return new Color32(R, G, B, 255);
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

	// TODO changer ces fonctions en Coroutine et adapter les codes les appelant
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
	// Fonction utilisée pour augmenter le volume d'un son progressivement
//	public static void AudioFadeIn(AudioSource audio, float volumeMax = 1, float delay = 1) {
//		if (audio == null || volumeMax < 0 || delay == 0)
//			return;
//		if (volumeMax > 1)
//			volumeMax = 1;
//		if (audio.volume > volumeMax)
//			return;
//
//		if (!audio.isPlaying) { // On démarre le son au volume le plus bas
//			audio.volume = 0;
//			audio.Play ();
//		}
//
//		audio.volume += TimeManager.deltaTime / delay;
//	}

	// Fonction utilisée pour diminuer le volume d'un son progressivement
//	public static void AudioFadeOut(AudioSource audio, float volumeMin = 0, float delay = 1) {
//		if (audio == null || volumeMin > 1 || delay == 0)
//			return;
//		if (volumeMin < 0)
//			volumeMin = 0;
//		if (audio.volume < volumeMin) { // On stoppe le son lorsqu'on est au min demandé
//			audio.Stop ();
//			return;
//		}
//
//		audio.volume -= TimeManager.deltaTime / delay;
//	}

	public static Color ColorFromHSV(float h, float s, float v, float a = 1)
	{
		// no saturation, we can return the value across the board (grayscale)
		if (s == 0)
			return new Color(v, v, v, a);
		
		// which chunk of the rainbow are we in?
		float sector = h / 60;
		
		// split across the decimal (ie 3.87 into 3 and 0.87)
		int i = (int)sector;
		float f = sector - i;
		
		float p = v * (1 - s);
		float q = v * (1 - s * f);
		float t = v * (1 - s * (1 - f));
		
		// build our rgb color
		Color color = new Color(0, 0, 0, a);
		
		switch(i)
		{
		case 0:
			color.r = v;
			color.g = t;
			color.b = p;
			break;
			
		case 1:
			color.r = q;
			color.g = v;
			color.b = p;
			break;
			
		case 2:
			color.r  = p;
			color.g  = v;
			color.b  = t;
			break;
			
		case 3:
			color.r  = p;
			color.g  = q;
			color.b  = v;
			break;
			
		case 4:
			color.r  = t;
			color.g  = p;
			color.b  = v;
			break;
			
		default:
			color.r  = v;
			color.g  = p;
			color.b  = q;
			break;
		}
		
		return color;
	}
	
	public static void ColorToHSV(Color color, out float h, out float s, out float v)
	{
		float min = Mathf.Min(Mathf.Min(color.r, color.g), color.b);
		float max = Mathf.Max(Mathf.Max(color.r, color.g), color.b);
		float delta = max - min;
		
		// value is our max color
		v = max;
		
		// saturation is percent of max
		if (!Mathf.Approximately(max, 0))
			s = delta / max;
		else
		{
			// all colors are zero, no saturation and hue is undefined
			s = 0;
			h = -1;
			return;
		}
		
		// grayscale image if min and max are the same
		if (Mathf.Approximately(min, max))
		{
			v = max;
			s = 0;
			h = -1;
			return;
		}
		
		// hue depends which color is max (this creates a rainbow effect)
		if (color.r == max)
			h = (color.g - color.b) / delta;         	// between yellow & magenta
		else if (color.g == max)
			h = 2 + (color.b - color.r) / delta; 		// between cyan & yellow
		else
			h = 4 + (color.r - color.g) / delta; 		// between magenta & cyan
		
		// turn hue into 0-360 degrees
		h *= 60;
		if (h < 0 )
			h += 360;
	}
}
