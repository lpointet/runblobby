using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Collections;

public class MainMenuManager : MonoBehaviour {

	public static MainMenuManager mainMenuManager;
	private SFXMenu sfxSound;

	public Color deactivatedColor;

	/******************/
	/* Menu d'accueil */
	[Header("Main Menu")]
	public GameObject wMainMenu;

    public Button bContinue;

    public Button bNewGame;
	public GameObject wNewGame;

	public Button bMusic;
	private Slider sMusic;
	public AudioMixer aMusicMixer;

	public Button bSfx;
	public AudioMixer aSfxMixer;
	private Slider sSfx;

	public Button bAchievement;

	public Button bStats;

	public Button bOption;

	public Button bTutoriel;

    public Button bQuit;
	public GameObject wQuit;
	/* Fin menu d'accueil */
	/**********************/

	/***************************************/
	/* Ecran des talents/équipements/level */
	[Header("Player Menu")]
	public GameObject wPlayerMenu;

	public Text tTitle;

	public Button bLevel;
	public GameObject wLevel;
	private LevelItem[] listLevel;

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

	/***********************/
	/* Ecran de chargement */
	[Header("Loading Screen")]
	public GameObject loadingScreen;
	public Slider loadingBar;
	/* Fin écran de chargement */
	/***************************/

	/*********************/
	/* Ecran du tutoriel */
	[Header("Tutoriel Menu")]
	public GameObject wTutoriel;

	public Button bNextTuto;
	public Button bPreviousTuto;
	public Button bSkipTuto;

	private Vector2 dragTutoStart;
	private Vector2 dragTutoEnd;

	public GameObject[] listPageTuto;
	public RectTransform menuTutoButton;
	public GameObject pageButton;
	private GameObject[] listPageButton;
	private Vector3 initialScale;
	private Vector3 pageButtonActifScale;
	private int currentPage = 0;
	private bool isTutoFromOption = true; // Permet de savoir si on a lancé le tutoriel depuis le menu ou si c'est celui qui se lance avec un New Game
	/* Fin de l'écran du tutoriel */
	/******************************/

    void Awake() {
		if (mainMenuManager == null)
			mainMenuManager = GameObject.FindGameObjectWithTag ("GameMaster").GetComponent<MainMenuManager> ();

		sMusic = bMusic.GetComponentInChildren<Slider> ();
		sSfx = bSfx.GetComponentInChildren<Slider> ();

		sfxSound = GetComponentInChildren<SFXMenu> ();
    }

	void Start() {
		sMusic.value = GameData.gameData.musicVolume;
		sSfx.value = GameData.gameData.sfxVolume;

		sMusic.gameObject.SetActive (false);
		sSfx.gameObject.SetActive (false);

		// On initialise les valeurs du son
		AjusterVolume(aMusicMixer, "musicVolume", sMusic);
		AjusterVolume(aSfxMixer, "sfxVolume", sSfx, -21, 3);

		// On prépare le menu
		CleanMenu();

		// TODO boutons à activer avec leurs fonctions développées
		DeactiveButton (bAchievement);
		DeactiveButton (bStats);
		DeactiveButton (bOption);

		/* On charge l'écran de "jeu" */
		if (_GameData.loadListLevel) {
			ActivatePlayerMenu ();
			Level_Click ();
			_GameData.loadListLevel = false;
		}
	}

	void Update () {
		if (wQuit.activeInHierarchy)
			bQuit.Select ();

		if (wTutoriel.activeInHierarchy)
			bTutoriel.Select ();

		if (wLevel.activeInHierarchy)
			bLevel.Select ();
	}

	// Effet qui doit apparaître pour tous les boutons : sons, effets...
	private void ActiveMenu(Button menu) {
		CleanMenu (menu);
		menu.Select ();
		sfxSound.ButtonYesClick ();
	}

	// Pour remettre le menu dans un état "neutre" (sauf le bouton éventuellement en paramètre)
	private void CleanMenu(Button menu = null) {
		if (wMainMenu.activeInHierarchy) {
			// Les parties qui ne sont actives que pour un menu particulier sont éteintes ici
			if (menu != bMusic)
				sMusic.gameObject.SetActive (false);
			if (menu != bSfx)
				sSfx.gameObject.SetActive (false);

			// Spécification du bouton "Continue" qui ne doit être que "New" si aucune partie n'est là
			if (!GameData.gameData.existingGame) {
				DeactiveButton (bNewGame);
			}
		} else if (wPlayerMenu.activeInHierarchy) {
			bLevel.Select ();
			wLevel.SetActive (true);
		}

		wTutoriel.SetActive (false);
	}

	// Permet de désactiver un bouton
	private void DeactiveButton(Button deactivateButton) {
		deactivateButton.interactable = false;
		deactivateButton.GetComponent<Image> ().color = deactivatedColor;
		deactivateButton.GetComponent<Image> ().raycastTarget = false;
	}




	/********************************************/
	/********** PARTIE MENU PRINCIPALE **********/
	/********************************************/
    public void Everywhere_Click() {
		CleanMenu();
    }

    public void Continue_Click() {
		// Le bouton se comporte différemment selon qu'une partie existe déjà ou non
		if (!GameData.gameData.existingGame) {
			NewGame_Click ();
		} else {
			ActivatePlayerMenu ();
			Level_Click ();
		}
    }

	public void NewGame_Click() {
		ActiveMenu(bNewGame);
		// Demander confirmation avant d'effacer la sauvegarde courante
		if (GameData.gameData.existingGame)
			wNewGame.SetActive (true);
		else {
			isTutoFromOption = false;
			Tuto_Begin ();
		}
    }

	public void NewGame_No_Click() {
		sfxSound.ButtonNoClick ();
		wNewGame.SetActive (false);

		CleanMenu ();
	}

	public void NewGame_Yes_Click() {
		sfxSound.ButtonYesClick ();
		wNewGame.SetActive (false);

		_StaticFunction.Erase ();

		isTutoFromOption = false;
		Tuto_Begin ();
	}

	public void Music_Click() {
		ActiveMenu(bMusic);
		sMusic.gameObject.SetActive (true);
	}

	public void Sfx_Click() {
		ActiveMenu(bSfx);
		sSfx.gameObject.SetActive (true);
	}

	public void SliderMusic_Change() {
		bMusic.Select ();

		GameData.gameData.musicVolume = sMusic.value;
		AjusterVolume(aMusicMixer, "musicVolume", sMusic);

		if (sMusic.value <= 0)
			MuteSound (aMusicMixer, "musicVolume");
	}

	public void SliderSFX_Change() {
		bSfx.Select ();

		GameData.gameData.sfxVolume = sSfx.value;
		AjusterVolume(aSfxMixer, "sfxVolume", sSfx, -21, 3);

		if (sSfx.value <= 0)
			MuteSound (aSfxMixer, "sfxVolume");

		sfxSound.ButtonYesClick ();
	}

    public void Quit_Click() {
		ActiveMenu(bQuit);

		wQuit.SetActive (true);
    }

	public void Quit_Yes_Click() {
		_StaticFunction.Save ();

#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit ();
#endif
	}

	public void Quit_No_Click() {
		sfxSound.ButtonNoClick ();

		wQuit.SetActive (false);
		CleanMenu ();
	}

	public void Credit_Click() {
		wCreditMenu.SetActive (true);
	}

	public void Credit_Back() {
		sfxSound.ButtonNoClick ();

		wCreditMenu.SetActive (false);
		wMainMenu.SetActive (true);
		CleanMenu ();
	}

	public void Tuto_Begin() {
		ActiveMenu (bTutoriel);

		currentPage = 0;
		listPageButton = new GameObject[listPageTuto.Length];

		// On supprime tous les enfants s'il y en a
		if (menuTutoButton.childCount > 0) {
			for (int i = 0; i < menuTutoButton.childCount; i++) {
				Destroy (menuTutoButton.GetChild (i).gameObject);
			}
		}

		// Création des boutons d'indexation
		for (int i = 0; i < listPageTuto.Length; i++) {
			GameObject newPage = Instantiate (pageButton) as GameObject;
			newPage.transform.SetParent (menuTutoButton, false);
			listPageButton [i] = newPage;
		}

		pageButtonActifScale = 3f * Vector3.one;
		initialScale = listPageButton [currentPage].transform.localScale;

		listPageTuto [currentPage].SetActive (true);
		listPageButton [currentPage].transform.localScale = pageButtonActifScale;

		wTutoriel.SetActive (true);
	}

	public void NextTuto_Click() {
		if (currentPage == listPageTuto.Length - 1) {
			listPageTuto [currentPage].SetActive (false);
			SkipTuto_Click ();
		} else {
			listPageTuto [currentPage].SetActive (false);
			listPageTuto [++currentPage].SetActive (true);

			listPageButton [currentPage].transform.localScale = pageButtonActifScale;
			listPageButton [currentPage - 1].transform.localScale = initialScale;
		}
	}

	public void PreviousTuto_Click() {
		if (currentPage > 0) {
			listPageTuto [currentPage].SetActive (false);
			listPageTuto [--currentPage].SetActive (true);

			listPageButton [currentPage].transform.localScale = pageButtonActifScale;
			listPageButton [currentPage + 1].transform.localScale = initialScale;
		}
	}

	public void SkipTuto_Click() {
		wTutoriel.SetActive (false);

		// Si on lance le tuto depuis New Game
		if (!isTutoFromOption) {
			// Lancement du premier niveau avec les bons paramètres pour une nouvelle partie
			_GameData.currentLevel = 1;
			_GameData.currentDifficulty = 0; // TODO difficulté : v2
			_GameData.isStory = true;
			_GameData.currentLevelName = GameData.gameData.playerData.levelData [0].levelName;

			LoadLevel (1);
		}
	}

	public void BeginDragTuto() {
		dragTutoStart = Input.mousePosition;
	}

	public void EndDragTuto() {
		dragTutoEnd = Input.mousePosition;

		float directionX = dragTutoEnd.x - dragTutoStart.x;
		float directionY = dragTutoEnd.y - dragTutoStart.y;
		if (Mathf.Abs (directionX) < 125 || Mathf.Abs (directionX) < Mathf.Abs (directionY) )
			return;

		if (directionX < 0)
			NextTuto_Click ();
		else
			PreviousTuto_Click ();
	}

	private void ActivatePlayerMenu() {
		CleanMenu(); // On nettoie l'écran actuel

		wMainMenu.SetActive (false);
		wPlayerMenu.SetActive (true);
		// TODO boutons à activer avec leurs fonctions développées
		DeactiveButton (bTalent);
		DeactiveButton (bEquipment);

		listLevel = GetComponentsInChildren<LevelItem> ();

		CleanMenu(); // Et on nettoie l'écran du "jeu"
	}

	private void AjusterVolume(AudioMixer audioSource, string valueName, Slider curseur, int valeurMin = -24, int valeurMax = 0) {
		audioSource.SetFloat (valueName, _StaticFunction.MappingScale(curseur.value, curseur.minValue, curseur.maxValue, valeurMin, valeurMax));
	}

	private void MuteSound(AudioMixer audiosource, string valueName) {
		audiosource.SetFloat (valueName, -80);
	}
	/********************************************/
	/******** FIN PARTIE MENU PRINCIPALE ********/
	/********************************************/



	/********************************************/
	/************ PARTIE MENU JOUEUR ************/
	/********************************************/
	public void Player_Back() {
		sfxSound.ButtonNoClick ();

		wMainMenu.SetActive (true);
		wPlayerMenu.SetActive (false);
		CleanMenu();
	}

	public void Level_Click() {
		ActiveMenu(bLevel);
		tTitle.text = "LEVEL";

		wLevel.SetActive (true);
	}

	public void LevelUp_Click() {
		wLevel.GetComponentInChildren<LevelScrollList> ().MonterLevel();
	}

	public void LevelDown_Click() {
		wLevel.GetComponentInChildren<LevelScrollList> ().DescendreLevel();
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

	public void Talent_Click() {
		ActiveMenu(bTalent);
		tTitle.text = "SKILL";
	}

	public void Equipment_Click() {
		ActiveMenu(bEquipment);
		tTitle.text = "EQUIPMENT";
	}

	public void LevelSelection_Click(int level) {
		foreach (LevelItem item in listLevel) {
			// Activation du level cliqué
			if (item.GetLevelNumber () == level) {
				// Seulement s'il n'est pas déjà actif
				//if (!item.IsSelected ())
					item.SelectLevelItem ();
			}
			else
				item.DeselectLevelItem ();
		}
	}
	/********************************************/
	/********** FIN PARTIE MENU JOUEUR **********/
	/********************************************/
}