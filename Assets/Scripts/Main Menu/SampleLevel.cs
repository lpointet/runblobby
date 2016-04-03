using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SampleLevel : MonoBehaviour {

	private LevelScrollList scrollList;

	public Image background;
	public Text levelName;
	public Slider progress;
	public Text distance;
	public GameObject deadBoss;
	public int sceneNumber;

	public Button storyMode;
	public Button arcadeMode;

	private MainMenuManager menuManager;

	void Awake() {
		menuManager = FindObjectOfType<MainMenuManager> ();
	}

	void Start() {
		scrollList = GetComponentInParent<LevelScrollList> ();
	}

	public void Level_Click() {
		scrollList.ResetLevel ();
		// Si le boss a été tué, on débloque le mode Arcade
		// On affiche donc les deux boutons et on cache le reste
		if (deadBoss.activeInHierarchy) {
			SwitchMode(true);
		} else {
			SetLevelGameData (sceneNumber, levelName.text);
			menuManager.LoadLevel (sceneNumber);
		}
	}

	public void Story_Click() {
		SetLevelGameData (sceneNumber, levelName.text);
		menuManager.LoadLevel (sceneNumber);
	}

	public void Arcade_Click() {
		SetLevelGameData (sceneNumber, levelName.text, 0, false);
		menuManager.LoadLevel (sceneNumber);
	}

	public void SwitchMode(bool buttonActif) {
		storyMode.gameObject.SetActive (buttonActif);
		arcadeMode.gameObject.SetActive (buttonActif);
		
		//GetComponent<Image>().enabled = !buttonActif;
		levelName.gameObject.SetActive(!buttonActif);
		progress.gameObject.SetActive (!buttonActif);
		deadBoss.SetActive(!buttonActif);
	}

	private void SetLevelGameData(int level, string name, int difficulty = 0, bool story = true) {
		_GameData.currentLevel = level;
		_GameData.currentDifficulty = difficulty; // TODO difficulté : v2
		_GameData.isStory = story;
		_GameData.currentLevelName = name;
	}
}
