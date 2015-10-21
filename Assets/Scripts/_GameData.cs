using UnityEngine;
using System.Collections;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class _GameData : MonoBehaviour {

	public static _GameData gameData;

	private string saveFile;

	/* Les valeurs entrées sont les valeurs par défaut s'il n'y a pas de sauvegarde */
	/* PARAMETRES DU JEU */
	public float musicVolume = 2;
	public float sfxVolume = 3;

	/* STATISTIQUES DU JOUEUR */
	public string playerName = "";
	public int currentLevel = 1;
	public bool isStory = true;

	void Awake () {
		if (gameData == null) {
			gameData = this;
			DontDestroyOnLoad (gameObject);
			Load ();
		} else if (gameData != this)
			DestroyObject (gameObject);
	}

	void Start() {
		saveFile = Application.persistentDataPath + "/param.dat";
	}

	public void Save() {
		BinaryFormatter bf = new BinaryFormatter();
		FileStream file = File.Create (saveFile);

		PlayerData data = new PlayerData ();
		data.musicVolume = musicVolume;
		data.sfxVolume = sfxVolume;

		bf.Serialize (file, data);
		file.Close ();

		Debug.Log ("Save successful.");
	}

	public bool Load() {
		if (File.Exists (saveFile)) {
			BinaryFormatter bf = new BinaryFormatter ();
			FileStream file = File.Open (saveFile, FileMode.Open);

			PlayerData data = bf.Deserialize (file) as PlayerData;
			file.Close ();

			musicVolume = data.musicVolume;
			sfxVolume = data.sfxVolume;

			Debug.Log ("Load successful.");
			return true;
		} else
			return false;
	}
}

[Serializable]
class PlayerData {
	/* PARAMETRES DU JEU */
	public float musicVolume;
	public float sfxVolume;
	
	/* STATISTIQUES DU JOUEUR */
}