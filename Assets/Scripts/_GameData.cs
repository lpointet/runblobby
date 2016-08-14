using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System;

public class _GameData : MonoBehaviour {

	public static _GameData current;

	public List<LevelData> levelList;
	public static string saveFile;

	/* Permet de gérer le chargement de la liste des levels (passage d'arguments) */
	public static bool loadListLevel = false;

	/* Passage d'informations au LevelManager depuis le Menu */
	public static int currentLevel = 1;
	public static int currentDifficulty = 0;
	public static bool isStory = true;
	public static string currentLevelName;

	void Awake() {
		if (current == null) {
			current = this;
			//DontDestroyOnLoad (current); // TODO probablement à remettre quand on enlève le _GameData des levels
		} else
			DestroyObject (this);

		saveFile = Application.persistentDataPath + "/param.dat";

		GameData.gameData = new GameData ();
		if (!_StaticFunction.Load ())
			Debug.Log ("Erreur de chargement du fichier de sauvegarde.");
	}
}

[Serializable]
public class GameData {

	public static GameData gameData;

	public bool existingGame = false; // Modifier à true à la sauvegarde

	/* PARAMETRES DU JEU */
	public float musicVolume = 2;
	public float sfxVolume = 3;

	public int firstLevel = 1; // Correspond au numéro de scène du premier level, permet de corriger le numéro du level dans certaines fonctions (chargement level depuis le menu)
	public int lastLevel = 1; // Numéro de scène du dernier level (corriger dans le constructeur, ceci est un fallback)

	/* PARAMETRES DU JOUEUR */
	public PlayerData playerData;

	public GameData() {
		lastLevel = SceneManager.sceneCountInBuildSettings;

		playerData = new PlayerData ();
	}
}

[Serializable]
public class PlayerData {

	public string name; // Servira si un jour on doit différencier des joueurs

	/* ETAT COURANT DU JEU */
	public bool isStory = true;
	public int currentLevel = 1;
	public int currentDifficulty = 0;
	public int leaf = 300; // TODO mettre à 0

	/* STATISTIQUES GLOBALES DU JOUEUR */
	public int experience = 0;
	public int level = 1;
	public long distanceTotal = 0; // distance totale parcourue, tout confondu
	public int enemyKilled = 0; // nombre d'ennemis tués
	public int numberOfDeath = 0; // nombre de morts
	public int maxLeaf = 0; // nombre de feuilles récupérées

	/* TALENTS */
	public Talent talent = new Talent();

	/* STATISTIQUES DES LEVELS */
	public List<LevelData> levelData = new List<LevelData>();

	public PlayerData(string nom = "") {
		name = nom;

		levelData = _GameData.current.levelList;
	}
}

[Serializable]
public class Talent {
	// Sac à dos
	public int backPack = 0;
	public float backPackPointValue = 10;

	// Armurerie
	public int armory = 0;
	public int healthPoint = 0;
	public float healthPointPointValue = 1; // Points de vie supplémentaire
	public int defense = 0;
	public float defensePointValue = 1; // Dégâts reçus = ATTAQUE (ennemie) * (10 / (10 + DEFENSE)) (minimum 1) (voir PENETRATION)
	public int sendBack = 0;
	public float sendBackPointValue = 1; // Renvoi = RENVOI / 100
	public int dodge = 0;
	public float dodgePointValue = 1; // Esquive = ESQUIVE / 100
	public int reflection = 0;
	public float reflectionPointValue = 1; // Réflexion = REFLEXION / 100
	public int invulnerabilityTime = 0;
	public float invulnerabilityTimePointValue = 0.1f; // Invulnérabilité après coup = INVULNERABILITE

	// Arena
	public int arena = 0;
	public int criticalHit = 0;
	public float criticalHitPointValue = 1; // Chance critique = (10 + CRITIQUE) / 100
	public int criticalPower = 0;
	public float criticalPowerPointValue = 5; // Coup critique = ((150 + P. CRITIQUE) / 100 )* ATTAQUE
	public int attack = 0;
	public float attackPointValue = 1; // Dégâts infligés  = ATTAQUE * (10 / (10 + DEFENSE (ennemie))
	public int sharp = 0;
	public float sharpPointValue = 2; // DEFENSE (ennemie) = DEFENSE (ennemie) * (1 - PENETRATION / 100)
	public int machineGun = 0;
	public float machineGunPointValue = 1; // Munition = MUNITION
	public int shotDouble = 0;
	public float shotDoublePointValue = 5; // Double tir = DOUBLE TIR / 100 de second tir gratuit
	public int shotWidth = 0;
	public float shotWidthPointValue = 5; // Scale des missiles = LARGEUR
	public int shotRemote = 0;
	public float shotRemotePointValue = 1; // Calcul trajectoire = distance (ennemie) / TELEGUIDE

	// Sanctuaire
	public int sanctuary = 0;

	// Jardin
	public int garden = 0;
	public int leafDouble = 0;
	public float leafDoublePointValue = 5;
	public int leafBonus = 0;
	public float leafBonusPointValue = 5;
	public int leafLoss = 0;
	public float leafLossPointValue = 5;

	// Académie
	public int academy = 0;
	public int buffPower = 0;
	public float buffPowerPointValue = 5;

	public int flight = 1;
	public float flightPointValue = 1;
	public int flightDef = 0;
	public float flightDefPointValue = 0.1f;
	public int flightAtk = 0;
	public float flightAtkPointValue = 1;
	public int flightSkill = 0;
	public float flightSkillPointValue = 1;

	public int tornado = 1;
	public float tornadoPointValue = 1;
	public int tornadoDef = 0;
	public float tornadoDefPointValue = 5;
	public int tornadoAtk = 0;
	public float tornadoAtkPointValue = 1;
	public int tornadoSkill = 0;
	public float tornadoSkillPointValue = 1;

	public int shield = 1;
	public float shieldPointValue = 1;
	public int shieldDef = 0;
	public float shieldDefPointValue = 1;
	public int shieldAtk = 0;
	public float shieldAtkPointValue = -1;
	public int shieldSkill = 0;
	public float shieldSkillPointValue = 1;

	public int leaf = 0;
	public float leafPointValue = 1;
	public int leafDef = 0;
	public float leafDefPointValue = 10;
	public int leafAtk = 0;
	public float leafAtkPointValue = 1;
	public int leafSkill = 0;
	public float leafSkillPointValue = 1;

	public int heal = 0;
	public float healPointValue = 1;
	public int healDef = 0;
	public float healDefPointValue = 1;
	public int healAtk = 0;
	public float healAtkPointValue = -1;
	public int healSkill = 0;
	public float healSkillPointValue = 1;

	public int lastWish = 0;
	public float lastWishPointValue = 1;
	public int lastWishDef = 0;
	public float lastWishDefPointValue = 2;
	public int lastWishAtk = 0;
	public float lastWishAtkPointValue = 1;  // Weapon.cs : AddOwnerParameters() : augmente l'ATTAQUE de l'arme de 1
	public int lastWishSkill = 0;
	public float lastWishSkillPointValue = 1;

	public int cloud = 0;
	public float cloudPointValue = 1;
	public int cloudDef = 0;
	public float cloudDefPointValue = 1;
	public int cloudAtk = 0;
	public float cloudAtkPointValue = 10;
	public int cloudSkill = 0;
	public float cloudSkillPointValue = 1;

	// Alchimie
	public int alchemy = 0;
	public int jumpPower = 0;
	public float jumpPowerPointValue = 5;
	public int vampirisme = 0;
	public float vampirismePointValue = 1;
	public int regeneration = 0;
	public float regenerationPointValue = 1;

	// Horlogerie
	public int horology = 0;
	public int buffDelay = 0;
	public float buffDelayPointValue = 2;
	public int buffLength = 0; 
	public float buffLengthPointValue = 0.5f;
	public int speedBonus = 0;
	public float speedBonusPointValue = -2;
	public int bossLengthBonus = 0;
	public float bossLengthBonusPointValue = 1;
	public int attackDelay = 0;
	public float attackDelayPointValue = 1;
	public int attackSpeed = 0;
	public float attackSpeedPointValue = 5;
}

[Serializable]
public class LevelData {
	public int levelNumber; // Premier level = 1...
	public string levelName;
	public ArcadeData arcadeData; // Données en mode Arcade
	public StoryData[] storyData; // Tableau des données selon la difficulté en mode Story : 0 = normal, 1 = hard, 2 = hell
}

[Serializable]
public class ArcadeData {
	public int scoreRecord; // score maximum obtenu
	public int distanceRecord; // distance maximum parcourue
}

[Serializable]
public class StoryData {
	public int scoreRecord; // score maximum obtenu
	public int distanceRecord; // distance maximum parcourue
	public int distanceMax; // distance maximum du niveau
	public bool isBossDead; // true = boss mort
}