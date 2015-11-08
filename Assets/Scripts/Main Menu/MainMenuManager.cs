using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Collections;

public class MainMenuManager : MonoBehaviour {

	public static MainMenuManager mainMenuManager;
	private bool existingGame = false;
	private SFXMenu sfxSound;

	/******************/
	/* Menu d'accueil */
	[Header("Main Menu")]
	public GameObject wMainMenu;

    public Button bContinue;

    public Button bNewGame;

    public Button bOptions;
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
	public Button bQuitYes;
	public Button bQuitNo;
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
        //tCentral.text = "";

		/*texteAffichable = "To travel deep down into the heart of a history with maaaany rebounds.\n\nLiterally.";
        AfficherTexte(texteAffichable);*/
		/*texteAffichable = "Runner's classic mode!\n\nRun.\nJump.\nDie.\n\nTry again.";
        AfficherTexte(texteAffichable);*/
    }

	void Start() {
		if (existingGame = _GameData.gameData.Load ()) {
			sMusic.value = _GameData.gameData.musicVolume;
			sSfx.value = _GameData.gameData.sfxVolume;
		}

		// On initialise les valeurs des textes et du son
		tMusicValue.text = sMusic.value.ToString ();
		AjusterVolume(aMusicMixer, "musicVolume", sMusic);

		tSfxValue.text = sSfx.value.ToString ();
		AjusterVolume(aSfxMixer, "sfxVolume", sSfx, -21, 3);

		ClearMenu();
	}

    public void Everywhere_Click() {
        ClearMenu();
    }

    public void Continue_Click() {
		SetMenuActive(bContinue);
    }

    public void NewGame_Click() {
		// TODO vérifier si une partie existe déjà
		// Demander confirmation avant de charger

		// TODO vrai lancement, on ne passe pas par l'autre menu lors d'une nouvelle partie
		//StartCoroutine (LoadLevelWithBar(1));
		//sfxSound.ButtonYesClick ();

		ChangeMainScreen ();
		Level_Click ();
    }

    public void Option_Click() {
		SetMenuActive(bOptions);
    }

	public void SliderMusic_Change() {
		tMusicValue.text = sMusic.value.ToString ();
		_GameData.gameData.musicVolume = sMusic.value;

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
		_GameData.gameData.sfxVolume = sSfx.value;

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
		_GameData.gameData.Save ();

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
			tMusic.gameObject.SetActive (true);
			tSfx.gameObject.SetActive (true);
		}
    }

    private void ClearMenu() {
		if (wMainMenu.activeInHierarchy) {
			bNewGame.GetComponentInChildren<Text> ().color = colorMenuNormal;
			bOptions.GetComponentInChildren<Text> ().color = colorMenuNormal;
			bQuit.GetComponentInChildren<Text> ().color = colorMenuNormal;

			tMusic.gameObject.SetActive (false);
			tSfx.gameObject.SetActive (false);

			if (!existingGame)
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

    /*private void AfficherTexte(string texte) {
        tCentral.text = texte;
        tCentral.gameObject.SetActive(true);

        bGo.gameObject.SetActive(true);
    }

    private void CacherTexte() {
        tCentral.gameObject.SetActive(false);
        bGo.gameObject.SetActive(false);
    }*/

	public void LoadLevel(int level) {
		// On vérifie que le level est au moins dans ce qui est existant
		// TODO on peut rajouter un controle en précisant les scènes précises existantes
		if (level < Application.levelCount && level > 0)
			StartCoroutine (LoadLevelWithBar (level));
	}

	IEnumerator LoadLevelWithBar(int level) {
		AsyncOperation asyncOp;
		loadingScreen.SetActive(true);
		asyncOp = Application.LoadLevelAsync (level);
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

    /* Ecrire les lettres l'une après l'autre
    * public float letterTyping = 0.01f;
    * Appel : StartCoroutine(TypeText(sAventure, tCentral));
    IEnumerator TypeText(string message, Text texte) {
        foreach (char letter in message.ToCharArray()) {
            texte.text += letter;
            yield return new WaitForSeconds(letterTyping);
        }
    }
    */
}
