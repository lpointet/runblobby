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
			DontDestroyOnLoad (current);
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

	/* STATISTIQUES GLOBALES DU JOUEUR */
	public int experience = 0;
	public int level = 1;
	public long distanceTotal = 0; // distance totale parcourue, tout confondu
	public int enemyKilled = 0; // nombre d'ennemis tués

	/* STATISTIQUES DES LEVELS */
	public List<LevelData> levelData = new List<LevelData>();

	public PlayerData(string nom = "") {
		name = nom;

		levelData = _GameData.current.levelList;
	}
}

[Serializable]
public class LevelData {
	public int levelNumber; // Premier level = 1...
	public string levelName;
	//public Sprite background;
	public ArcadeData[] arcadeData; // Tableau des données selon la difficulté en mode Arcade : 0 = normal, 1 = hard, 2 = hell
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