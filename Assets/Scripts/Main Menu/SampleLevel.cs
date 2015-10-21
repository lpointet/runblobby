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
			menuManager.LoadLevel (sceneNumber);
		}
	}

	public void Story_Click() {
		menuManager.LoadLevel (sceneNumber);
	}

	public void Arcade_Click() {
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
}
