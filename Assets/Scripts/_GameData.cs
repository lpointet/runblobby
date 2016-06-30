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
	public int leaf = 300;

	/* STATISTIQUES GLOBALES DU JOUEUR */
	public int experience = 0;
	public int level = 1;
	public long distanceTotal = 0; // distance totale parcourue, tout confondu
	public int enemyKilled = 0; // nombre d'ennemis tués
	public int numberOfDeath = 0; // nombre de morts

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

	// Armurerie
	public int armory = 0;
	public int healthPoint = 0; // Points de vie supplémentaire
	public int defense = 0; // Dégâts reçus = ATTAQUE (ennemie) * (10 / (10 + DEFENSE)) (minimum 1) (voir PENETRATION)
	public int sendBack = 0; // Renvoi = RENVOI / 100
	public int dodge = 0; // Esquive = ESQUIVE / 100
	public int reflection = 0; // Réflexion = REFLEXION / 100
	public int invulnerabilityLength = 0; // Invulnérabilité après coup = INVULNERABILITE

	// Arena
	public int arena = 0;
	public int criticalHit = 0; // Chance critique = (10 + CRITIQUE) / 100
	public int criticalPower = 0; // Coup critique = ((150 + P. CRITIQUE) / 100 )* ATTAQUE
	public int attack = 0; // Dégâts infligés  = ATTAQUE * (10 / (10 + DEFENSE (ennemie))
	public int sharp = 0; // DEFENSE (ennemie) = DEFENSE (ennemie) * (1 - PENETRATION / 100) // TODO calculer pour que la penetration n'ait pas à connaitre la defense ennemie ?
	public int machineGun = 0; // Munition = MUNITION
	public int shotDouble = 0;
	public int shotWidth = 0; // Scale des missiles = LARGEUR
	public int shotRemote = 0; // Calcul trajectoire = distance (ennemie) / TELEGUIDE

	// Sanctuaire
	public int sanctuary = 0;

	// Jardin
	public int garden = 0;
	public int leafDouble = 0;
	public int leafBonus = 0;

	// Académie
	public int academy = 0;
	public int buffPower = 0;

	public int flight = 1;
	public int flightDef = 0;
	public int flightAtk = 0;
	public int flightSkill = 0;

	public int tornado = 1;
	public int tornadoDef = 0;
	public int tornadoAtk = 0;
	public int tornadoSkill = 0;

	public int shield = 1;
	public int shieldDef = 0;
	public int shieldAtk = 0;
	public int shieldSkill = 0;

	public int leaf = 0;
	public int leafDef = 0;
	public int leafAtk = 0;
	public int leafSkill = 0;

	public int heal = 0;
	public int healDef = 0;
	public int healAtk = 0;
	public int healSkill = 0;

	public int lastWish = 0;
	public int lastWishDef = 0;
	public int lastWishAtk = 0;
	public int lastWishSkill = 0;

	public int cloud = 0;
	public int cloudDef = 0;
	public int cloudAtk = 0;
	public int cloudSkill = 0;

	// Alchimie
	public int alchemy = 0;
	public int jumpPower = 0;
	public int vampirisme = 0;
	public int regeneration = 0;

	// Horlogerie
	public int horology = 0;
	public int buffDelay = 0;
	public int buffLength = 0;
	public int speedBonus = 0;
	public int bossLengthBonus = 0;
	public int attackDelay = 0;
	public int attackSpeed = 0;
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