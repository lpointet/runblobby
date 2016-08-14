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

	public GameObject wError;
	/* Fin menu d'accueil */
	/**********************/

	/***************************************/
	/* Ecran des talents/équipements/level */
	[Header("Player Menu")]
	public GameObject wPlayerMenu;

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

	/**************************/
	/* Ecran des statistiques */
	[Header("Stats Menu")]
	public GameObject wStatistique;

	public RectTransform pLeft;
	public RectTransform pRight;
	public GameObject statItemPrefab;
	/* Fin de l'écran des statistiques */
	/***********************************/

	/**************************/
	/* Ecran des statistiques */
	[Header("Talent Menu")]
	public GameObject wTalent;

	public GameObject wDescription;
	public Text tTitreTalent;
	public Text tDetailTalent;
	public Text tGainTalent;
	public Text tCurrentTalent;
	public Text tLevelTalent;
	public Text tCostTalent;
	public Text tTotalLeaf;
	private Color initialLeafColor;
	public Color warningLeafColor;
	private float shakeTime;

	public static TalentButton[] listTalent { get; private set; }		// Contient la liste des tous les talents existants

	public GameObject wArmoryTalent;
	public GameObject wArenaTalent;
	public GameObject wSanctuaryTalent;
	public GameObject wGardenTalent;
	public GameObject wAcademyTalent;
	public GameObject wAlchemyTalent;
	public GameObject wHorologyTalent;
	/* Fin de l'écran des statistiques */
	/***********************************/

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
		DeactiveButton (bOption);

		IsTactileAndMutli ();

		/* On charge l'écran de "jeu" */
		if (_GameData.loadListLevel) {
			ActivatePlayerMenu ();
			Level_Click ();
			_GameData.loadListLevel = false;
		}

		listTalent = wTalent.GetComponentsInChildren<TalentButton> (); // TODO supprimer après les tests sur les talents
		initialLeafColor = tTotalLeaf.color; // TODO aussi

		// Force le timeScale au cas où on vient du menu
		Time.timeScale = 3;
	}

	void Update () {
		if (wQuit.activeInHierarchy)
			bQuit.Select ();

		if (wTutoriel.activeInHierarchy)
			bTutoriel.Select ();

		if (wLevel.activeInHierarchy)
			bLevel.Select ();

		if (wTalent.activeInHierarchy)
			bTalent.Select ();
	}

	// Contrôle que l'écran est tactile / mutlitouch
	private void IsTactileAndMutli() {
		// Si l'appareil n'est pas tactile, on affiche un message d'excuse et on ferme le jeu au bout d'un certain temps
		/*if (!Input.touchSupported) { // TODO réactiver plus tard
			Debug.LogError ("Touch not supported.");
			string errorMessage = "Touch is not supported on your device...\n" +
				"You can't play, sorry!\n\n" +
				"Bye bye!";
			StartCoroutine (WaitBeforeCloseGame (errorMessage, 5f));
		}*/
		// Si l'appareil ne supporte pas le multi-touch, on affiche un message expliquant que le jeu ne sera pas aussi agréable à jouer, mais quand même jouable
		if (!Input.multiTouchEnabled) {
			Debug.LogWarning ("Multitouch not supported.");
			string errorMessage = "Multitouch is not supported on your device.\n\n" +
				"You can still play (and have a lot of fun!), but controls won't be as smooth as they should have been. Shame, right?";
			MainMenuManager.mainMenuManager.Error_Display (errorMessage);
		}
	}

	private IEnumerator WaitBeforeCloseGame(string currentContent, float delay) {
		int remainingSecond;

		while (delay > 0) {
			delay -= Time.unscaledDeltaTime;
			remainingSecond = Mathf.CeilToInt (delay);

			MainMenuManager.mainMenuManager.Error_Display (currentContent + "\n\n" + remainingSecond.ToString());

			yield return null;
		}

		#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
		#else
		Application.Quit ();
		#endif
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
			wLevel.SetActive (false);
			wTalent.SetActive (false);

			if (menu == bLevel || menu == null)
				wLevel.SetActive (true);
			if (menu == bTalent)
				wTalent.SetActive (true);

			// TODO equipement
		}

		wTutoriel.SetActive (false);
		wStatistique.SetActive (false);
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

	public void Error_Display(string content) {
		wError.SetActive (true);
		Text errorText = wError.GetComponentInChildren<Text> ();

		errorText.text = content;
	}

	public void Error_Click() {
		wError.SetActive (false);
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

		// Si on lance le tuto depuis New Game, on active le bouton "Next" de la troisième page
		if (!isTutoFromOption) {
			bNextTuto.gameObject.SetActive (true);
		} else {
			bNextTuto.gameObject.SetActive (false);
		}
	}

	public void NextTuto_Click() {
		if (currentPage == listPageTuto.Length - 1) {
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
		listPageTuto [currentPage].SetActive (false);
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
	private void ActivatePlayerMenu() {
		CleanMenu(); // On nettoie l'écran actuel

		wMainMenu.SetActive (false);
		wPlayerMenu.SetActive (true);
		// TODO boutons à activer avec leurs fonctions développées
		DeactiveButton (bEquipment);

		// Level
		listLevel = GetComponentsInChildren<LevelItem> ();
		// Talent
		initialLeafColor = tTotalLeaf.color;
		listTalent = wTalent.GetComponentsInChildren<TalentButton> ();

		CleanMenu(); // Et on nettoie l'écran du "jeu"
	}

	public void Player_Back() {
		sfxSound.ButtonNoClick ();

		wMainMenu.SetActive (true);
		wPlayerMenu.SetActive (false);
		CleanMenu();
	}

	public void Level_Click() {
		ActiveMenu(bLevel);
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

	public void LevelSelection_Click(int level) {listLevel = GetComponentsInChildren<LevelItem> (); // TODO delete
		foreach (LevelItem item in listLevel) {
			// Activation du level cliqué
			if (item.GetLevelNumber () == level) {
				// Seulement s'il n'est pas déjà actif
				if (!item.IsSelected ())
					item.SelectLevelItem ();
			}
			else
				item.DeselectLevelItem ();
		}
	}

	public void Talent_Click() {
		ActiveMenu(bTalent);

		// On cache le panneau de description tant qu'aucun talent n'est sélectionné
		wDescription.SetActive (false);

		UpdateTalent ();
	}

	public void Equipment_Click() {
		ActiveMenu(bEquipment);
	}
	/********************************************/
	/********** FIN PARTIE MENU JOUEUR **********/
	/********************************************/

	/********************************************/
	/************ PARTIE STATISTIQUE ************/
	/********************************************/
	public void Stat_Click() {
		if (pLeft.childCount == 0) {
			// On ramène toutes les données nécessaires
			CreateStatItem ("Distance since first birth", GameData.gameData.playerData.distanceTotal, pLeft);
			CreateStatItem ("Enemies slaughtered", GameData.gameData.playerData.enemyKilled, pLeft);
			CreateStatItem ("Deaths *cry*", GameData.gameData.playerData.numberOfDeath, pLeft);
		}

		wStatistique.SetActive (true);
	}

	public void Close_Stat () {
		wStatistique.SetActive (false);
	}

	private void CreateStatItem(string name, float value, RectTransform parent) {
		GameObject obj = Instantiate(statItemPrefab);

		Text[] textStat = obj.GetComponentsInChildren<Text> ();

		textStat [0].text = name;
		textStat [1].text = value.ToString();

		obj.transform.SetParent (pLeft, false);
	}
	/********************************************/
	/********** FIN PARTIE STATISTIQUE **********/
	/********************************************/

	/***************************************/
	/************ PARTIE TALENT ************/
	/***************************************/
	public void UpdateTalent () {
		// Parcours de l'ensemble des talents pour activer les nouvelles parties
		// Appelé à chaque ajout de point dans un talent
		// ARMURERIE
		if (GameData.gameData.playerData.talent.armory <= 0) {
			// Test si inactif
			if (GameData.gameData.playerData.talent.backPack >= 1) {
				wArmoryTalent.SetActive (true);
			} else
				wArmoryTalent.SetActive (false);
		} else
			wArmoryTalent.SetActive (true);

		// ARENA
		if (GameData.gameData.playerData.talent.arena <= 0) {
			// Test si inactif
			if (GameData.gameData.playerData.talent.armory >= 20) {
				wArenaTalent.SetActive (true);
			} else
				wArenaTalent.SetActive (false);
		} else
			wArenaTalent.SetActive (true);

		// GARDEN
		if (GameData.gameData.playerData.talent.garden <= 0) {
			// Test si inactif
			if (GameData.gameData.playerData.talent.backPack >= 1) {
				wGardenTalent.SetActive (true);
			} else
				wGardenTalent.SetActive (false);
		} else
			wGardenTalent.SetActive (true);

		// ACADEMY
		if (GameData.gameData.playerData.talent.academy <= 0) {
			// Test si inactif
			if (GameData.gameData.playerData.talent.backPack >= 1) {
				wAcademyTalent.SetActive (true);
			} else
				wAcademyTalent.SetActive (false);
		} else
			wAcademyTalent.SetActive (true);

		// ALCHEMY
		if (GameData.gameData.playerData.talent.alchemy <= 0) {
			// Test si inactif
			if (GameData.gameData.playerData.talent.backPack >= 1) {
				wAlchemyTalent.SetActive (true);
			} else
				wAlchemyTalent.SetActive (false);
		} else
			wAlchemyTalent.SetActive (true);

		// HOROLOGY
		if (GameData.gameData.playerData.talent.horology <= 0) {
			// Test si inactif
			if (GameData.gameData.playerData.talent.flight >= 1 &&
				GameData.gameData.playerData.talent.tornado >= 1 &&
				GameData.gameData.playerData.talent.shield >= 1 &&
				GameData.gameData.playerData.talent.leaf >= 1 &&
				GameData.gameData.playerData.talent.heal >= 1 &&
				GameData.gameData.playerData.talent.lastWish >= 1 &&
				GameData.gameData.playerData.talent.cloud >= 1) {
				wHorologyTalent.SetActive (true);
			} else
				wHorologyTalent.SetActive (false);
		} else
			wHorologyTalent.SetActive (true);

		// Parcours de l'ensemble des talents pour activer ceux qui sont débloqués
		// Appelé à chaque ajout de point dans un talent
		for (int i = 0; i < listTalent.Length; i++) {
			if (!listTalent [i].IsActivated () && listTalent [i].IsAvailable ()) {
				listTalent [i].ActivateTalent ();
			}
		}
	}

	public void DisplayTalent (Transform talent, string title, string detail, string mathText, float gainPerPoint, int currentValue, int valueMax, int leafCost, bool bought = false) {
		// On affiche le panneau
		wDescription.SetActive (true);

		tTitreTalent.text = title;
		tDetailTalent.text = detail;
		tGainTalent.text = mathText.Replace ("{value}", gainPerPoint.ToString ());
		tCurrentTalent.text = mathText.Replace ("{value}", (gainPerPoint * currentValue).ToString());
		tLevelTalent.text = string.Format ("{0}/{1}", currentValue, valueMax);
		tCostTalent.text = leafCost.ToString();

		// On écrase des valeurs dans le cas où l'amélioration est complète
		if (currentValue == valueMax) {
			tGainTalent.text = "-";
			tCostTalent.text = "-";
		}

		// Si on achète, on affiche une décrémentation du compteur de feuilles
		if (bought)
			StartCoroutine (BuyTalent ());
		else
			tTotalLeaf.text = GameData.gameData.playerData.leaf.ToString ();
		
		// Test pour choisir la couleur du total de feuille
		int valueTotalLeaf = 0;
		if (!int.TryParse (tTotalLeaf.text, out valueTotalLeaf) || valueTotalLeaf < leafCost)
			tTotalLeaf.color = warningLeafColor;
		else
			tTotalLeaf.color = initialLeafColor;
	}

	public void DeselectAllTalent () {
		// On désélectionne tous les talents
		for (int i = 0; i < listTalent.Length; i++) {
			listTalent [i].DeselectTalent ();
		}
		// On enlève l'affichage de l'écran de description
		wDescription.SetActive(false);
	}

	public IEnumerator ShakeLeaves () {
		// On actualise la durée si on relance la fonction
		shakeTime = 2f;
		float currentRandom;

		if (shakeTime > 0) {
			while (shakeTime > 0) {
				currentRandom = 1.5f + shakeTime;
				tTotalLeaf.rectTransform.anchoredPosition = new Vector2 (Random.Range (-currentRandom, currentRandom), Random.Range (-currentRandom, currentRandom));

				shakeTime -= Time.deltaTime;
				yield return null;
			}
		}

		// Paramètres initiaux
		tTotalLeaf.rectTransform.anchoredPosition = Vector2.zero;
	}

	public IEnumerator BuyTalent () {
		float countTime = 1f;
		float currentTimer = 0;

		int currentCount;
		int finalCount = GameData.gameData.playerData.leaf;

		if (int.TryParse(tTotalLeaf.text, out currentCount)) {
			while (currentTimer < countTime) {
				tTotalLeaf.text = Mathf.RoundToInt (Mathf.Lerp (currentCount, finalCount, currentTimer / countTime)).ToString ();

				currentTimer += Time.deltaTime;
				yield return null;
			}
		}

		tTotalLeaf.text = finalCount.ToString ();
	}
	/***************************************/
	/********** FIN PARTIE TALENT **********/
	/***************************************/
}