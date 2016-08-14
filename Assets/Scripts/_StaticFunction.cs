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
				GameData.gameData.playerData.leaf = tempData.playerData.leaf;
				GameData.gameData.playerData.experience = tempData.playerData.experience;
				GameData.gameData.playerData.level = tempData.playerData.level;
				GameData.gameData.playerData.distanceTotal = tempData.playerData.distanceTotal;
				GameData.gameData.playerData.enemyKilled = tempData.playerData.enemyKilled;
				GameData.gameData.playerData.numberOfDeath = tempData.playerData.numberOfDeath;
				GameData.gameData.playerData.maxLeaf = tempData.playerData.maxLeaf;

				// Copie des données Talents
				// Sac à dos
				GameData.gameData.playerData.talent.backPack = tempData.playerData.talent.backPack;
				GameData.gameData.playerData.talent.backPackPointValue = tempData.playerData.talent.backPackPointValue;

				// Armurerie
				GameData.gameData.playerData.talent.armory = tempData.playerData.talent.armory;
				GameData.gameData.playerData.talent.healthPoint = tempData.playerData.talent.healthPoint;
				GameData.gameData.playerData.talent.healthPointPointValue = tempData.playerData.talent.healthPointPointValue;
				GameData.gameData.playerData.talent.defense = tempData.playerData.talent.defense;
				GameData.gameData.playerData.talent.defensePointValue = tempData.playerData.talent.defensePointValue;
				GameData.gameData.playerData.talent.sendBack = tempData.playerData.talent.sendBack;
				GameData.gameData.playerData.talent.sendBackPointValue = tempData.playerData.talent.sendBackPointValue;
				GameData.gameData.playerData.talent.dodge = tempData.playerData.talent.dodge;
				GameData.gameData.playerData.talent.dodgePointValue = tempData.playerData.talent.dodgePointValue;
				GameData.gameData.playerData.talent.reflection = tempData.playerData.talent.reflection;
				GameData.gameData.playerData.talent.reflectionPointValue = tempData.playerData.talent.reflectionPointValue;
				GameData.gameData.playerData.talent.invulnerabilityTime = tempData.playerData.talent.invulnerabilityTime;
				GameData.gameData.playerData.talent.invulnerabilityTimePointValue = tempData.playerData.talent.invulnerabilityTimePointValue;

				// Arena
				GameData.gameData.playerData.talent.arena = tempData.playerData.talent.arena;
				GameData.gameData.playerData.talent.criticalHit = tempData.playerData.talent.criticalHit;
				GameData.gameData.playerData.talent.criticalHitPointValue = tempData.playerData.talent.criticalHitPointValue;
				GameData.gameData.playerData.talent.criticalPower = tempData.playerData.talent.criticalPower;
				GameData.gameData.playerData.talent.criticalPowerPointValue = tempData.playerData.talent.criticalPowerPointValue;
				GameData.gameData.playerData.talent.attack = tempData.playerData.talent.attack;
				GameData.gameData.playerData.talent.attackPointValue = tempData.playerData.talent.attackPointValue;
				GameData.gameData.playerData.talent.sharp = tempData.playerData.talent.sharp;
				GameData.gameData.playerData.talent.sharpPointValue = tempData.playerData.talent.sharpPointValue;
				GameData.gameData.playerData.talent.machineGun = tempData.playerData.talent.machineGun;
				GameData.gameData.playerData.talent.machineGunPointValue = tempData.playerData.talent.machineGunPointValue;
				GameData.gameData.playerData.talent.shotDouble = tempData.playerData.talent.shotDouble;
				GameData.gameData.playerData.talent.shotDoublePointValue = tempData.playerData.talent.shotDoublePointValue;
				GameData.gameData.playerData.talent.shotWidth = tempData.playerData.talent.shotWidth;
				GameData.gameData.playerData.talent.shotWidthPointValue = tempData.playerData.talent.shotWidthPointValue;
				GameData.gameData.playerData.talent.shotRemote = tempData.playerData.talent.shotRemote;
				GameData.gameData.playerData.talent.shotRemotePointValue = tempData.playerData.talent.shotRemotePointValue;

				// Sanctuaire
				GameData.gameData.playerData.talent.sanctuary = tempData.playerData.talent.sanctuary;

				// Jardin
				GameData.gameData.playerData.talent.garden = tempData.playerData.talent.garden;
				GameData.gameData.playerData.talent.leafDouble = tempData.playerData.talent.leafDouble;
				GameData.gameData.playerData.talent.leafDoublePointValue = tempData.playerData.talent.leafDoublePointValue;
				GameData.gameData.playerData.talent.leafBonus = tempData.playerData.talent.leafBonus;
				GameData.gameData.playerData.talent.leafBonusPointValue = tempData.playerData.talent.leafBonusPointValue;
				GameData.gameData.playerData.talent.leafLoss = tempData.playerData.talent.leafLoss;
				GameData.gameData.playerData.talent.leafLossPointValue = tempData.playerData.talent.leafLossPointValue;

				// Académie
				GameData.gameData.playerData.talent.academy = tempData.playerData.talent.academy;
				GameData.gameData.playerData.talent.buffPower = tempData.playerData.talent.buffPower;
				GameData.gameData.playerData.talent.buffPowerPointValue = tempData.playerData.talent.buffPowerPointValue;

				GameData.gameData.playerData.talent.flight = tempData.playerData.talent.flight;
				GameData.gameData.playerData.talent.flightPointValue = tempData.playerData.talent.flightPointValue;
				GameData.gameData.playerData.talent.flightDef = tempData.playerData.talent.flightDef;
				GameData.gameData.playerData.talent.flightDefPointValue = tempData.playerData.talent.flightDefPointValue;
				GameData.gameData.playerData.talent.flightAtk = tempData.playerData.talent.flightAtk;
				GameData.gameData.playerData.talent.flightAtkPointValue = tempData.playerData.talent.flightAtkPointValue;
				GameData.gameData.playerData.talent.flightSkill = tempData.playerData.talent.flightSkill;
				GameData.gameData.playerData.talent.flightSkillPointValue = tempData.playerData.talent.flightSkillPointValue;

				GameData.gameData.playerData.talent.tornado = tempData.playerData.talent.tornado;
				GameData.gameData.playerData.talent.tornadoPointValue = tempData.playerData.talent.tornadoPointValue;
				GameData.gameData.playerData.talent.tornadoDef = tempData.playerData.talent.tornadoDef;
				GameData.gameData.playerData.talent.tornadoDefPointValue = tempData.playerData.talent.tornadoDefPointValue;
				GameData.gameData.playerData.talent.tornadoAtk = tempData.playerData.talent.tornadoAtk;
				GameData.gameData.playerData.talent.tornadoAtkPointValue = tempData.playerData.talent.tornadoAtkPointValue;
				GameData.gameData.playerData.talent.tornadoSkill = tempData.playerData.talent.tornadoSkill;
				GameData.gameData.playerData.talent.tornadoSkillPointValue = tempData.playerData.talent.tornadoSkillPointValue;

				GameData.gameData.playerData.talent.shield = tempData.playerData.talent.shield;
				GameData.gameData.playerData.talent.shieldPointValue = tempData.playerData.talent.shieldPointValue;
				GameData.gameData.playerData.talent.shieldDef = tempData.playerData.talent.shieldDef;
				GameData.gameData.playerData.talent.shieldDefPointValue = tempData.playerData.talent.shieldDefPointValue;
				GameData.gameData.playerData.talent.shieldAtk = tempData.playerData.talent.shieldAtk;
				GameData.gameData.playerData.talent.shieldAtkPointValue = tempData.playerData.talent.shieldAtkPointValue;
				GameData.gameData.playerData.talent.shieldSkill = tempData.playerData.talent.shieldSkill;
				GameData.gameData.playerData.talent.shieldSkillPointValue = tempData.playerData.talent.shieldSkillPointValue;

				GameData.gameData.playerData.talent.leaf = tempData.playerData.talent.leaf;
				GameData.gameData.playerData.talent.leafPointValue = tempData.playerData.talent.leafPointValue;
				GameData.gameData.playerData.talent.leafDef = tempData.playerData.talent.leafDef;
				GameData.gameData.playerData.talent.leafDefPointValue = tempData.playerData.talent.leafDefPointValue;
				GameData.gameData.playerData.talent.leafAtk = tempData.playerData.talent.leafAtk;
				GameData.gameData.playerData.talent.leafAtkPointValue = tempData.playerData.talent.leafAtkPointValue;
				GameData.gameData.playerData.talent.leafSkill = tempData.playerData.talent.leafSkill;
				GameData.gameData.playerData.talent.leafSkillPointValue = tempData.playerData.talent.leafSkillPointValue;

				GameData.gameData.playerData.talent.heal = tempData.playerData.talent.heal;
				GameData.gameData.playerData.talent.healPointValue = tempData.playerData.talent.healPointValue;
				GameData.gameData.playerData.talent.healDef = tempData.playerData.talent.healDef;
				GameData.gameData.playerData.talent.healDefPointValue = tempData.playerData.talent.healDefPointValue;
				GameData.gameData.playerData.talent.healAtk = tempData.playerData.talent.healAtk;
				GameData.gameData.playerData.talent.healAtkPointValue = tempData.playerData.talent.healAtkPointValue;
				GameData.gameData.playerData.talent.healSkill = tempData.playerData.talent.healSkill;
				GameData.gameData.playerData.talent.healSkillPointValue = tempData.playerData.talent.healSkillPointValue;

				GameData.gameData.playerData.talent.lastWish = tempData.playerData.talent.lastWish;
				GameData.gameData.playerData.talent.lastWishPointValue = tempData.playerData.talent.lastWishPointValue;
				GameData.gameData.playerData.talent.lastWishDef = tempData.playerData.talent.lastWishDef;
				GameData.gameData.playerData.talent.lastWishDefPointValue = tempData.playerData.talent.lastWishDefPointValue;
				GameData.gameData.playerData.talent.lastWishAtk = tempData.playerData.talent.lastWishAtk;
				GameData.gameData.playerData.talent.lastWishAtkPointValue = tempData.playerData.talent.lastWishAtkPointValue;
				GameData.gameData.playerData.talent.lastWishSkill = tempData.playerData.talent.lastWishSkill;
				GameData.gameData.playerData.talent.lastWishSkillPointValue = tempData.playerData.talent.lastWishSkillPointValue;

				GameData.gameData.playerData.talent.cloud = tempData.playerData.talent.cloud;
				GameData.gameData.playerData.talent.cloudPointValue = tempData.playerData.talent.cloudPointValue;
				GameData.gameData.playerData.talent.cloudDef = tempData.playerData.talent.cloudDef;
				GameData.gameData.playerData.talent.cloudDefPointValue = tempData.playerData.talent.cloudDefPointValue;
				GameData.gameData.playerData.talent.cloudAtk = tempData.playerData.talent.cloudAtk;
				GameData.gameData.playerData.talent.cloudAtkPointValue = tempData.playerData.talent.cloudAtkPointValue;
				GameData.gameData.playerData.talent.cloudSkill = tempData.playerData.talent.cloudSkill;
				GameData.gameData.playerData.talent.cloudSkillPointValue = tempData.playerData.talent.cloudSkillPointValue;

				// Alchimie
				GameData.gameData.playerData.talent.alchemy = tempData.playerData.talent.alchemy;
				GameData.gameData.playerData.talent.jumpPower = tempData.playerData.talent.jumpPower;
				GameData.gameData.playerData.talent.jumpPowerPointValue = tempData.playerData.talent.jumpPowerPointValue;
				GameData.gameData.playerData.talent.vampirisme = tempData.playerData.talent.vampirisme;
				GameData.gameData.playerData.talent.vampirismePointValue = tempData.playerData.talent.vampirismePointValue;
				GameData.gameData.playerData.talent.regeneration = tempData.playerData.talent.regeneration;
				GameData.gameData.playerData.talent.regenerationPointValue = tempData.playerData.talent.regenerationPointValue;

				// Horlogerie
				GameData.gameData.playerData.talent.horology = tempData.playerData.talent.horology;
				GameData.gameData.playerData.talent.buffDelay = tempData.playerData.talent.buffDelay;
				GameData.gameData.playerData.talent.buffDelayPointValue = tempData.playerData.talent.buffDelayPointValue;
				GameData.gameData.playerData.talent.buffLength = tempData.playerData.talent.buffLength;
				GameData.gameData.playerData.talent.buffLengthPointValue = tempData.playerData.talent.buffLengthPointValue;
				GameData.gameData.playerData.talent.speedBonus = tempData.playerData.talent.speedBonus;
				GameData.gameData.playerData.talent.speedBonusPointValue = tempData.playerData.talent.speedBonusPointValue;
				GameData.gameData.playerData.talent.bossLengthBonus = tempData.playerData.talent.bossLengthBonus;
				GameData.gameData.playerData.talent.bossLengthBonusPointValue = tempData.playerData.talent.bossLengthBonusPointValue;
				GameData.gameData.playerData.talent.attackDelay = tempData.playerData.talent.attackDelay;
				GameData.gameData.playerData.talent.attackDelayPointValue = tempData.playerData.talent.attackDelayPointValue;
				GameData.gameData.playerData.talent.attackSpeed = tempData.playerData.talent.attackSpeed;
				GameData.gameData.playerData.talent.attackSpeedPointValue = tempData.playerData.talent.attackSpeedPointValue;

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

		// Sauvegardes spécifiques au jeu
		if (LevelManager.levelManager != null) {
			GameData.gameData.playerData.experience += ScoreManager.GetExperience ();
			GameData.gameData.playerData.level = LevelFromExp (GameData.gameData.playerData.experience);
			GameData.gameData.playerData.distanceTotal += LevelManager.levelManager.GetDistanceTraveled ();
			GameData.gameData.playerData.leaf = ScoreManager.GetFinalScore ();
			GameData.gameData.playerData.maxLeaf += ScoreManager.GetLeaf ();

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
