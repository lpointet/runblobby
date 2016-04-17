using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Collections;

public class MainMenuManager : MonoBehaviour {

	public static MainMenuManager mainMenuManager;
	private SFXMenu sfxSound;

	/******************/
	/* Menu d'accueil */
	[Header("Main Menu")]
	public GameObject wMainMenu;

    public Button bContinue;

    public Button bNewGame;
	public GameObject wNewGame;

    public Button bOptions;
	public GameObject wOptionPanel;
	public Text tMusic;
	public Text tMusicValue;
	public Slider sMusic;
	public AudioMixer aMusicMixer;
	public Text tSfx;
	public Text tSfxValue;
	public AudioMixer aSfxMixer;
	public Slider sSfx;

    public Button bQuit;
	public GameObject wQuit;
	/* Fin menu d'accueil */
	/**********************/

	/***************************************/
	/* Ecran des talents/équipements/level */
	[Header("Player Menu")]
	public GameObject wPlayerMenu;

	public Button bLevel;
	public GameObject levelScrollView;

	public Button bTalent;

	public Button bEquipment;
	/* Fin de l'écran des talents/équipements/level */
	/************************************************/

	/*********************/
	/* Ecran des crédits */
	[Header("Crédit Menu")]
	public GameObject wCreditMenu;

	public Button bCreditBack;
	public GameObject creditScrollView;
	/* Fin de l'écran des crédits */
	/******************************/

	/* Ecran de chargement */
	[Header("Loading Screen")]
	public GameObject loadingScreen;
	public Slider loadingBar;
	/* Fin écran de chargement */

	private Color32 colorOptionValue = _StaticFunction.ToColor (0x4DACF9);
	private Color32 colorOptionNulle = _StaticFunction.ToColor(0xF94D4D);
	private Color32 colorMenuNormal = _StaticFunction.ToColor (0xFFFFFF);
	private Color32 colorMenuSelect = _StaticFunction.ToColor (0x4DACF9);

    void Awake() {
		if (mainMenuManager == null)
			mainMenuManager = GameObject.FindGameObjectWithTag ("GameMaster").GetComponent<MainMenuManager> ();

		sfxSound = GetComponentInChildren<SFXMenu> ();
    }

	void Start() {
		sMusic.value = GameData.gameData.musicVolume;
		sSfx.value = GameData.gameData.sfxVolume;

		// On initialise les valeurs des textes et du son
		tMusicValue.text = sMusic.value.ToString ();
		AjusterVolume(aMusicMixer, "musicVolume", sMusic);

		tSfxValue.text = sSfx.value.ToString ();
		AjusterVolume(aSfxMixer, "sfxVolume", sSfx, -21, 3);

		ClearMenu();

		/* On charge l'écran de "jeu" */
		if (_GameData.loadListLevel) {
			ChangeMainScreen ();
			Level_Click ();
			_GameData.loadListLevel = false;
		}
	}

    public void Everywhere_Click() {
        ClearMenu();
    }

    public void Continue_Click() {
		ChangeMainScreen ();
		Level_Click ();
    }

	public void NewGame_Click() {
		// Demander confirmation avant de charger
		if (GameData.gameData.existingGame)
			wNewGame.SetActive (true);
		else {
			// Lancement du premier niveau si on a valider le message de confirmation
			_GameData.currentLevel = 1;
			_GameData.currentDifficulty = 0; // TODO difficulté : v2
			_GameData.isStory = true;
			_GameData.currentLevelName = GameData.gameData.playerData.levelData [0].levelName;

			LoadLevel (1);
		}
    }

	public void NewGame_No_Click() {
		sfxSound.ButtonNoClick ();

		wNewGame.SetActive (false);
	}

	public void NewGame_Yes_Click() {
		sfxSound.ButtonYesClick ();

		_StaticFunction.Erase ();

		// Lancement du premier niveau si on a valider le message de confirmation
		_GameData.currentLevel = 1;
		_GameData.currentDifficulty = 0; // TODO difficulté : v2
		_GameData.isStory = true;
		_GameData.currentLevelName = GameData.gameData.playerData.levelData [0].levelName;

		LoadLevel (1);
	}

    public void Option_Click() {
		SetMenuActive(bOptions);
    }

	public void SliderMusic_Change() {
		tMusicValue.text = sMusic.value.ToString ();
		GameData.gameData.musicVolume = sMusic.value;

		AjusterVolume(aMusicMixer, "musicVolume", sMusic);

		if (sMusic.value > 0)
			tMusicValue.color = colorOptionValue;
		else {
			tMusicValue.color = colorOptionNulle;
			MuteSound(aMusicMixer, "musicVolume");
		}
	}

	public void SliderSFX_Change() {
		tSfxValue.text = sSfx.value.ToString ();
		GameData.gameData.sfxVolume = sSfx.value;

		AjusterVolume(aSfxMixer, "sfxVolume", sSfx, -21, 3);

		if (sSfx.value > 0)
			tSfxValue.color = colorOptionValue;
		else {
			tSfxValue.color = colorOptionNulle;
			MuteSound(aSfxMixer, "sfxVolume");
		}

		sfxSound.ButtonYesClick ();
	}

    public void Quit_Click() {
		SetMenuActive(bQuit);

		wQuit.SetActive (true);
    }

	public void Quit_Yes_Click() {
		_StaticFunction.Save ();

#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE
		Application.Quit ();
#endif
	}

	public void Quit_No_Click() {
		sfxSound.ButtonNoClick ();

		wQuit.SetActive (false);
		ClearMenu ();
	}

	public void Credit_Click() {
		wCreditMenu.SetActive (true);
	}

	public void Credit_Back() {
		wCreditMenu.SetActive (false);
		wMainMenu.SetActive (true);
		ClearMenu ();
	}

	public void Player_Back() {
		sfxSound.ButtonNoClick ();

		ChangeMainScreen ();
	}

	public void Level_Click() {
		SetMenuActive(bLevel);

		levelScrollView.SetActive (true);
	}

	public void LevelUp_Click() {
		levelScrollView.GetComponent<LevelScrollList> ().MonterLevel();
	}

	public void LevelDown_Click() {
		levelScrollView.GetComponent<LevelScrollList> ().DescendreLevel();
	}

	private void SetMenuActive(Button menu, Button submenu = null) {
        ClearMenu();
		sfxSound.ButtonYesClick ();

		menu.GetComponentInChildren<Text> ().color = colorMenuSelect;

        /*if (submenu != null)
			submenu.GetComponentInChildren<Text> ().color = colorMenuSelect;*/

        // On active tout l'arbre descendant suivant le menu
       if (menu == bOptions) {
			wOptionPanel.SetActive (true);
			//tMusic.gameObject.SetActive (true);
			//tSfx.gameObject.SetActive (true);
		}
    }

    private void ClearMenu() {
		if (wMainMenu.activeInHierarchy) {
			bNewGame.GetComponentInChildren<Text> ().color = colorMenuNormal;
			bOptions.GetComponentInChildren<Text> ().color = colorMenuNormal;
			bQuit.GetComponentInChildren<Text> ().color = colorMenuNormal;

			wOptionPanel.SetActive (false);
			//tMusic.gameObject.SetActive (false);
			//tSfx.gameObject.SetActive (false);

			if (!GameData.gameData.existingGame)
				bContinue.gameObject.SetActive (false);
			else
				bContinue.GetComponentInChildren<Text> ().color = colorMenuNormal;
			
		} else if (wPlayerMenu.activeInHierarchy) {
			bLevel.GetComponentInChildren<Text> ().color = colorMenuNormal;

			levelScrollView.SetActive (false);
		}
    }

	private void ChangeMainScreen() {
		ClearMenu(); // On nettoie l'ancien
		if (wMainMenu.activeInHierarchy) {
			wMainMenu.SetActive (false);
			wPlayerMenu.SetActive (true);
		} else if (wPlayerMenu.activeInHierarchy) {
			wMainMenu.SetActive (true);
			wPlayerMenu.SetActive (false);
		}
		ClearMenu(); // Et le nouveau
	}

	public void LoadLevel(int level) {
		// On vérifie que le level est au moins dans ce qui est existant
		// TODO on peut rajouter un controle en précisant les scènes précises existantes
		if (level < SceneManager.sceneCountInBuildSettings && level > 0)
			StartCoroutine (LoadLevelWithBar (level));
	}

	IEnumerator LoadLevelWithBar(int level) {
		AsyncOperation asyncOp;
		asyncOp = SceneManager.LoadSceneAsync (level);

		loadingScreen.SetActive(true);

		while (!asyncOp.isDone) {
			loadingBar.value = asyncOp.progress;
			yield return null;
		}
	}

	private void AjusterVolume(AudioMixer audioSource, string valueName, Slider curseur, int valeurMin = -24, int valeurMax = 0) {
		audioSource.SetFloat (valueName, _StaticFunction.MappingScale(curseur.value, curseur.minValue, curseur.maxValue, valeurMin, valeurMax));
	}

	private void MuteSound(AudioMixer audiosource, string valueName) {
		audiosource.SetFloat (valueName, -80);
	}
}