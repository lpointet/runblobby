using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

// Un écran de gain d'item/mode à la fin d'une partie
[System.Serializable]
public class GainScreen {
	public string presentationText;
	public string gainText;
	public Sprite gainSprite;

	public GainScreen (string presentation, string gain, Sprite sprite) {
		presentationText = presentation;
		gainText = gain;
		gainSprite = sprite;
	}
}

// Une ligne de dialogue : le numéro de portrait, la position (gauche/droite), le texte
[System.Serializable]
public struct DialogEntry {
	public enum PortraitPosition { left, right };

	public int numCharacter;
	public PortraitPosition portraitPosition;
	[TextArea(3,10)] public string textLine;
}

// Choix possible d'affichage du CombatLog
public enum LogType { damage, heal, pickup, special };

public class UIManager : MonoBehaviour {

	public static UIManager uiManager;

	[Header("UI standard")]
	public GameObject standardUI;

	// Compteur
	public Text meterText;
	private Color defaultTextColor; // Couleur par défaut du meterText
	private float scaleInitial; // Echelle du texte au début
	public Color warningTextColor; // Couleur en alternance lors d'un ennemi
	public Color warningTextColorBis;
	private float scaleFonctionDistance; // Echelle du texte pendant l'ennemi
	private float enemyTimeToKill;

	// Distance parcourue
	public Slider meterTravelled;
	
	private Enemy enemyEnCours = null;
	private bool enemyGUIActive = false;
	public Text enemyName;
	public Text enemySurname;
	public Text enemyBarName;

	// Barre de vie
	private float lerpingTimeEnemyBar = 0f;
	public Slider enemyHealthBar;
	private float previousHP = 0;

	// Munition
	public Slider ammunition;

	[Header("Combat Log")]
	public Text combatText;
	public Color damageColor;
	public Color healColor;
	public Color pickupColor;
	public Color specialColor;

	[Header("Pause Menu")]
	public GameObject pauseUI;
	public Text distancePause;
	public Slider distanceSliderPause;
	public Text moneyPause;
	public Slider moneySliderPause;

	public GameObject cdObject;
	private Animator cdAnim;
	
	private bool paused = false;
	private float initialTimeScale;

	[Header("End Menu")]
	public GameObject endUI;

	public Text distanceEnd;
	public Slider distanceSliderEnd;
	public Text moneyEnd;
	public Slider moneySliderEnd;
	public Text moneyBonus;
	public Text experience;
	public Slider experienceSliderEnd;
	public Text gainLevel;
	public Text tCurrentPlayerLevel;
	private List<Button> bEndUI = new List<Button> (); // Liste des boutons de l'interface finale, directement enfants de endUI
	public GameObject bNextLevel; // Uniquement le bouton pour le niveau suivant
	public ParticleSystem pFirework;
	public Text tTitreEndGame;

	public AudioClip endFailure;
	public AudioClip endVictory;
	private AudioSource endAudio;

	public GameObject newItemUI;
	public Image newItem;
	public Image newItemBorder;
	public Text tnewItemPresentation;
	public Text tnewItemObject;
	public Button bArmory;
	private List<GainScreen> listGainScreen = new List<GainScreen> (); // Liste des nouveaux écrans à afficher
	private bool changeGainObject = false; // Permet de savoir si on a changé un item dans les écrans de gains (en cas de multi gains)

	private float timeUpdateValue;
	private float delaySliderFull = 1.5f;

	[Header("Dialog Menu")]
	public GameObject dialogUI;

	public Animator rightPortrait;
	public Animator leftPortrait;
	public Text dialogText;

	[Header("Générique")]
	public GameObject wQuit;
	public Button bQuitYes;
	public Button bQuitNo;
	public Text tQuitContent;

	private SFXMenu sfxSound;

	// Déplacement des noms des ennemis
	private Vector2 namePosition;
	private Vector2 surnamePosition;
	private Vector2 positionNameBeforeEndMove;
	private Vector2 positionSurnameBeforeEndMove;
	private float textTimeToMid;
	private float textTimeMid;
	private float textTimeToEnd;
	private float textTimeTotal;

	public float GetTimeScale() {
		return initialTimeScale;
	}

    void Awake() {
		if (uiManager == null)
			uiManager = this;

		sfxSound = GetComponentInChildren<SFXMenu> ();

		foreach (Button button in endUI.GetComponentsInChildren<Button> (true)) {
			if (button.transform.parent == endUI.transform)
				bEndUI.Add (button);
		}

        // Compteur
        defaultTextColor = meterText.color;
        scaleInitial = meterText.rectTransform.localScale.x;
    }

	void Start() {
		cdAnim = cdObject.GetComponent<Animator> ();

		pauseUI.SetActive (false);
		endUI.SetActive (false);
		dialogUI.SetActive (false);
		wQuit.SetActive (false);

		ToggleEnemyGUI (false);

		// Position des textes : ceci est la position de départ, le milieu étant (0, 0) et la fin -position de départ
		namePosition = new Vector2((Screen.width + enemyName.rectTransform.sizeDelta.x) / 2f, enemyName.rectTransform.anchoredPosition.y);
		surnamePosition = new Vector2(-(Screen.width + enemySurname.rectTransform.sizeDelta.x) / 2f, enemySurname.rectTransform.anchoredPosition.y);
		enemyName.rectTransform.anchoredPosition = namePosition;
		enemySurname.rectTransform.anchoredPosition = surnamePosition;

		textTimeTotal = LevelManager.levelManager.enemySpawnDelay;
		textTimeToMid = 0.2f * textTimeTotal;
		textTimeToEnd = 0.2f * textTimeTotal;
		textTimeMid = 0.6f * textTimeTotal;

		// Compteur distance (à n'afficher qu'en mode histoire)
		if (LevelManager.levelManager.IsStory ()) {
			meterTravelled.maxValue = LevelManager.levelManager.listPhase [LevelManager.levelManager.listPhase.Length - 1];
			meterTravelled.gameObject.SetActive (true);
		} else {
			meterTravelled.gameObject.SetActive (false);
		}
	}

	void LateUpdate() {
		enemyEnCours = LevelManager.levelManager.GetEnemyEnCours();

		if (endUI.activeInHierarchy && Input.GetMouseButton(0) && timeUpdateValue < delaySliderFull) {
			timeUpdateValue = delaySliderFull - 0.001f; // Pour permettre de rentrer une dernière fois dans la boucle while de UpdateSlider()

			int diffLvl = _StaticFunction.LevelFromExp(GameData.gameData.playerData.experience + ScoreManager.GetExperience ()) - _StaticFunction.LevelFromExp(GameData.gameData.playerData.experience);
			StartCoroutine (PopText (gainLevel, diffLvl));
		}

		if (TimeManager.paused)
			return;

		// Compteur
		MeterTextManager();
		if (LevelManager.levelManager.IsStory ())
			MeterBarManager ();

		EnemySpawnManager();

		EnemyManager();
	}

    public void PauseManager() { 
		if (!endUI.activeInHierarchy && !dialogUI.activeInHierarchy && !cdObject.activeInHierarchy && !LevelManager.player.IsDead()) {
			paused = !paused;

			if (paused) {
				initialTimeScale = Time.timeScale;
				TogglePauseMenu (true);
				UpdateValueScore ();
			}
		}
    }

    private void EnemySpawnManager() {
        // Variable changée par la classe StartEnemyBlock sur le OnTriggerEnter2D, marque le début du compte à rebours pour le boss
		if (LevelManager.levelManager.IsEnemyToSpawn ()) {
			// On affiche la barre de vie vide, pour pouvoir la remplir le temps du spawn (= timer d'apparition)
			if (!enemyGUIActive) {
				ToggleEnemyGUI (true);
				enemyBarName.gameObject.SetActive (false);
			}

			// On remplit la barre de vie en fonction du temps de spawn 100% = ennemi apparait
			lerpingTimeEnemyBar += TimeManager.deltaTime / LevelManager.levelManager.enemySpawnDelay;
			enemyHealthBar.value = Mathf.Lerp (0, 1, lerpingTimeEnemyBar);

			// Quand l'ennemi est là, on "force" des valeurs
			if (enemyEnCours != null) {
				enemyHealthBar.value = 1;
				previousHP = 1; // Pour éviter que la première frame dans EnemyManager() montre un ratio d'HP = 0
				LevelManager.levelManager.SetEnemyToSpawn (false); // On sort de la boucle d'apparition
			}

			// On gère le déplacement des textes
			if (textTimeTotal > textTimeMid + textTimeToEnd) {
				float lerpTime = (LevelManager.levelManager.enemySpawnDelay - textTimeTotal) / textTimeToMid;
				MoveIntroEnemyText (enemyName, lerpTime, namePosition.x, 0);
				MoveIntroEnemyText (enemySurname, lerpTime, surnamePosition.x, 0);
			} else if (textTimeTotal > textTimeToEnd) {
				enemyName.rectTransform.anchoredPosition = new Vector2 (enemyName.rectTransform.anchoredPosition.x - 1f, enemyName.rectTransform.anchoredPosition.y);
				enemySurname.rectTransform.anchoredPosition = new Vector2 (enemySurname.rectTransform.anchoredPosition.x + 1f, enemySurname.rectTransform.anchoredPosition.y);
				positionNameBeforeEndMove = enemyName.rectTransform.anchoredPosition;
				positionSurnameBeforeEndMove = enemySurname.rectTransform.anchoredPosition;
			} else if (textTimeTotal > 0) {
				float lerpTime = (LevelManager.levelManager.enemySpawnDelay - textTimeMid - textTimeToEnd - textTimeTotal) / textTimeToEnd;
				MoveIntroEnemyText (enemyName, lerpTime, positionNameBeforeEndMove.x, -namePosition.x);
				MoveIntroEnemyText (enemySurname, lerpTime, positionSurnameBeforeEndMove.x, -surnamePosition.x);
				// On affiche le nom du boss
				enemyBarName.text = enemyName.text;
				enemyBarName.gameObject.SetActive (true);
			}
			textTimeTotal -= TimeManager.deltaTime;
		} else { // Si on n'est pas dans le compte-à-rebours, on cache les textes d'apparition
			enemyName.gameObject.SetActive (false);
			enemySurname.gameObject.SetActive (false);
			textTimeTotal = LevelManager.levelManager.enemySpawnDelay;
		}
    }

	private void MoveIntroEnemyText(Text movingText, float currentTime, float posBegin, float posEnd) {
		float posCurrent = Mathf.Lerp (posBegin, posEnd, currentTime);
		movingText.rectTransform.anchoredPosition = new Vector2 (posCurrent, movingText.rectTransform.anchoredPosition.y);
	}

	private void EnemyManager() {
		if( null != enemyEnCours ) {
			if (!enemyGUIActive) {
				ToggleEnemyGUI( true );
			}

			float realRatioHP = enemyEnCours.healthPoint / (float)enemyEnCours.healthPointMax;

			// Si la valeur courante de la barre est plus haute que la valeur réelle des PV, on la fait descendre
			if (enemyHealthBar.value > realRatioHP) {
				lerpingTimeEnemyBar += TimeManager.deltaTime / 1f;
				enemyHealthBar.value = Mathf.Lerp (previousHP, realRatioHP, lerpingTimeEnemyBar);
			}
			// Sinon, on reboot le timer pour la prochaine fois
			else {
				previousHP = realRatioHP;
				enemyHealthBar.value = realRatioHP;
				lerpingTimeEnemyBar = 0;
			}
        }
        else if( !LevelManager.levelManager.IsEnemyToSpawn() && enemyGUIActive ) {
			enemyHealthBar.value = 0;
			ToggleEnemyGUI( false );
        }
    }

    private void MeterTextManager() {
        if( null != enemyEnCours ) {
			enemyTimeToKill = LevelManager.levelManager.GetEnemyTimeToKill();
			meterText.text = Mathf.RoundToInt (enemyTimeToKill).ToString ("0s"); // Mise à jour du temps restant pour tuer le boss
			meterText.color = Color.Lerp( warningTextColor, warningTextColorBis, Mathf.Sin( 7f * enemyTimeToKill ) ); // Variation entre deux couleurs

            // Fonction type f(x) = ax² + b, avec a = (scaleMaxAtteint-1) / distanceMaxPossible² et b = 1
			scaleFonctionDistance = ( 2 / Mathf.Pow( enemyEnCours.timeToKill, 2 ) ) * _StaticFunction.MathPower( enemyEnCours.timeToKill - enemyTimeToKill, 2 ) + 1;
			meterText.transform.parent.transform.localScale = new Vector2( scaleFonctionDistance, scaleFonctionDistance ) * scaleInitial;
        }
        else if( !LevelManager.player.IsDead() && !LevelManager.levelManager.IsEnemyToSpawn() ) {
			meterText.text = Mathf.RoundToInt( LevelManager.levelManager.GetDistanceTraveled() ).ToString("0m");// Mise à jour de la distance parcourue affichée
            meterText.color = defaultTextColor;
			meterText.transform.parent.transform.localScale = new Vector2( scaleInitial, scaleInitial );
        }
    }

	private void MeterBarManager() {
		meterTravelled.value = LevelManager.levelManager.GetDistanceTraveled ();
	}

	private void TogglePauseMenu(bool active) {
		pauseUI.SetActive (active);
		standardUI.SetActive (!active);

		Time.timeScale = active ? 0 : initialTimeScale;
	}

    private void ToggleEnemyGUI( bool active ) {
        enemyGUIActive = active;

		enemyName.gameObject.SetActive (active);
		enemySurname.gameObject.SetActive (active);
		enemyBarName.gameObject.SetActive (active);

		enemyHealthBar.gameObject.SetActive (active);
    }

	public void ToggleAmmoGUI (bool active) {
		ammunition.gameObject.SetActive (active);
	}

	public void ToggleEndMenu (bool active) {
		endUI.SetActive (active);
		standardUI.SetActive (!active);

		// On cache les boutons temporairement (ils doivent être visibles si on clique ou à la fin de l'update des sliders)
		foreach (Button button in bEndUI) {
			button.gameObject.SetActive (false);
		}

		// On cache le menu du nouvel objet obtenu
		newItemUI.SetActive (false);

		if (LevelManager.player.IsDead ()) {
			endUI.GetComponent<AudioSource> ().PlayOneShot (endFailure);
			tTitreEndGame.text = "YOU LOSE...";
			pFirework.gameObject.SetActive (false);
		} else {
			endUI.GetComponent<AudioSource> ().PlayOneShot (endVictory);
			tTitreEndGame.text = "YOU WIN!";
			pFirework.gameObject.SetActive (true);
		}

		UpdateValueScore ();
	}

	public IEnumerator RunDialog (RuntimeAnimatorController[] protagonist, DialogEntry[] dialogLine, System.Action<bool> callback) {
		dialogUI.SetActive (true);
		standardUI.SetActive (false);
		int currentLine = 0;

		Image leftSprite = leftPortrait.GetComponent<Image> ();
		Image rightSprite = rightPortrait.GetComponent<Image> ();
		Color leftColor = leftSprite.color;
		Color rightColor = rightSprite.color;

		// Initialisation
		leftPortrait.runtimeAnimatorController = protagonist [dialogLine [currentLine].numCharacter];
		rightPortrait.runtimeAnimatorController = protagonist [dialogLine [currentLine].numCharacter];

		// Tant qu'on n'a pas atteint la fin du dialogue, on défile
		while (currentLine < dialogLine.Length) {
			dialogText.text = dialogLine [currentLine].textLine;

			if (dialogLine [currentLine].portraitPosition == DialogEntry.PortraitPosition.left) {
				rightColor.a = 0.25f;
				rightSprite.color = rightColor;
				leftColor.a = 1;
				leftSprite.color = leftColor;
				leftPortrait.runtimeAnimatorController = protagonist [dialogLine [currentLine].numCharacter];
				leftPortrait.enabled = true;
				rightPortrait.enabled = false;
			} else if (dialogLine [currentLine].portraitPosition == DialogEntry.PortraitPosition.right) {
				rightColor.a = 1;
				rightSprite.color = rightColor;
				leftColor.a = 0.25f;
				leftSprite.color = leftColor;
				rightPortrait.runtimeAnimatorController = protagonist [dialogLine [currentLine].numCharacter];
				rightPortrait.enabled = true;
				leftPortrait.enabled = false;
			}

			// Si on appuie sur une touche, on affiche l'entrée suivante
			if (Input.GetMouseButtonDown (0))
				currentLine++;

			yield return null;
		}

		dialogUI.SetActive (false);
		standardUI.SetActive (true);
		callback (true);
	}

	private void UpdateValueScore() {
		// Partie Pause
		distancePause.text = Mathf.RoundToInt (LevelManager.levelManager.GetDistanceTraveled ()).ToString ();
		moneyPause.text = ScoreManager.GetScore ().ToString ();
		// Partie End Game
		distanceEnd.text = Mathf.RoundToInt (LevelManager.levelManager.GetDistanceTraveled ()).ToString ();
		moneyEnd.text = ScoreManager.GetScore ().ToString ();
		experience.text = ScoreManager.GetExperience ().ToString ();

		if (pauseUI.activeInHierarchy)
			UpdatePauseSlider ();
		
		if (endUI.activeInHierarchy)
			StartCoroutine (UpdateSlider ());
	}

	private void UpdatePauseSlider () {
		distanceSliderPause.maxValue = meterTravelled.maxValue;
		distanceSliderPause.value = meterTravelled.value;

		moneySliderPause.value = ScoreManager.GetRatioLeaf ();
	}

	private IEnumerator UpdateSlider() {
		distanceSliderEnd.maxValue = meterTravelled.maxValue;

		int currentPlayerLevel = _StaticFunction.LevelFromExp (GameData.gameData.playerData.experience);
		int nextPlayerLevel = currentPlayerLevel + 1;
		// Affichage du niveau courant
		tCurrentPlayerLevel.text = currentPlayerLevel.ToString();

		int currentPlayerExpInLevel = GameData.gameData.playerData.experience - _StaticFunction.ExpFromLevel (currentPlayerLevel);
		int futurPlayerExp = GameData.gameData.playerData.experience + ScoreManager.GetExperience ();

		int diffLevel = _StaticFunction.LevelFromExp (futurPlayerExp) - currentPlayerLevel;

		bool newLevel = true; // Savoir si l'on va passer un niveau complet ou non (sert en cas de multi level)
		int currentLevelGain = 0; // Gain en level courant
		int xpMaxThisLevel = 0; // XP à atteindre sur ce level

		float timeUpdateMultiLevel = 0; // Défini le temps passé sur chaque barre en cas de gain de level
		
		while (timeUpdateValue < delaySliderFull) {
			distanceSliderEnd.value = Mathf.Lerp (0, meterTravelled.value, timeUpdateValue / delaySliderFull);
			moneySliderEnd.value = Mathf.Lerp (0, ScoreManager.GetRatioLeaf (), timeUpdateValue / delaySliderFull);

			if (newLevel) {
				experienceSliderEnd.maxValue = _StaticFunction.ExpFromLevel (nextPlayerLevel + currentLevelGain) - _StaticFunction.ExpFromLevel (currentPlayerLevel + currentLevelGain);

				// Si le prochain level à atteindre est le dernier qu'on atteindra, la limite haute n'est pas l'xp requise pour passer de niveau, mais l'xp totale restante
				if (currentLevelGain == diffLevel) {
					xpMaxThisLevel = futurPlayerExp - _StaticFunction.ExpFromLevel (currentPlayerLevel + diffLevel);
				} else {
					xpMaxThisLevel = Mathf.RoundToInt(experienceSliderEnd.maxValue);
				}
					
				newLevel = false; // On reset éventuellement ce point lorsqu'on atteint la fin d'une barre
			}

			experienceSliderEnd.value = Mathf.Lerp (currentPlayerExpInLevel, xpMaxThisLevel, timeUpdateMultiLevel / delaySliderFull);

			// Lorsque l'on gagne un level
			if (currentLevelGain != diffLevel && experienceSliderEnd.value >= xpMaxThisLevel) {
				newLevel = true;
				currentLevelGain++;
				currentPlayerExpInLevel = 0;
				timeUpdateMultiLevel = 0;

				StartCoroutine (PopText (gainLevel, currentLevelGain));

				// Mise à jour du niveau courant
				int newPlayerLevel = currentPlayerLevel + currentLevelGain;
				tCurrentPlayerLevel.text = newPlayerLevel.ToString();
			}

			timeUpdateValue += TimeManager.deltaTime;
			timeUpdateMultiLevel += TimeManager.deltaTime * (diffLevel + 1);
			yield return null;
		}

		// Lorsque les sliders ont terminé leurs courses, on affiche le bonus de feuilles
		StartCoroutine (PopText (moneyBonus, ScoreManager.GetBonusScore ()));

		/****************************************************************************************************************/
		/* Affichage des potentiels gains d'objets et le lien vers l'armurerie ici, avant d'afficher les autres boutons */
		/****************************************************************************************************************/
		SearchGain (); // Remplit la listGainScreen

		// On évite totalement la boucle si la liste est vide
		if (listGainScreen.Count > 0) {
			while (!Input.GetMouseButtonUp (0) && listGainScreen.Count > 0) {
				// Ajout du nouvel objet/mode
				if (Input.GetMouseButtonUp (0)) {
					listGainScreen.RemoveAt (0); // On enlève celui qu'on a actuellement
					// On met le suivant s'il existe
					if (listGainScreen.Count > 0)
						NewGainScreen ();
				}
				// Affichage de l'écran et des objets gagnés
				if (!newItemUI.activeInHierarchy || changeGainObject) {
					newItemUI.SetActive (true);
					bArmory.gameObject.SetActive (false);

					StartCoroutine (PopObject (newItem.transform));
					StartCoroutine (PopObject (newItemBorder.transform));
				}
				// Affichage du bouton vers l'armurerie 1sec après l'affichage de l'objet
				if (!bArmory.isActiveAndEnabled) {
					yield return new WaitForSecondsRealtime (1);

					bArmory.gameObject.SetActive (true);
					StartCoroutine (PopObject (bArmory.transform));
				}

				changeGainObject = false;

				yield return null;
			}
		}
		// On cache le menu de gain d'objets
		newItemUI.SetActive (false);

		/*********************************************/
		/* Fin d'affichage des nouveaux objets/modes */
		/*********************************************/

		// Lorsque les sliders ont terminé leurs courses, on affiche les boutons
		foreach (Button button in bEndUI) {
			button.gameObject.SetActive (true);
			StartCoroutine (PopObject (button.transform));
		}
		if (LevelManager.player.IsDead ())
			bNextLevel.SetActive (false); // On cache ce bouton si le joueur est mort

		// On sauvegarde
		_StaticFunction.Save ();
	}

	// Ajoute tous les écrans possibles de gain à la listGainScreen, en cas de victoire
	private void SearchGain () {
		listGainScreen.Clear (); // Nettoyage de la liste des écrans
		// Remplissage de la liste des écrans
		int levelNumber = LevelManager.levelManager.GetCurrentLevel ();
		int levelMode = LevelManager.levelManager.GetCurrentDifficulty ();
		string presentation;
		string gain;
		ItemDescription item;

		// Le joueur ne doit pas être mort, et en mode "story" (on ne peut que "perdre" en mode "Arcade")
		if (!LevelManager.player.IsDead () && LevelManager.levelManager.IsStory ()) {
			// On teste la présence d'une condition de victoire non déjà remplie
			// Ex : boss pas déjà mort, ratioRecord de vie et de feuilles inférieurs à leur seuils...
			/* Normal */
			if (levelMode == 0) {
				if (!GameData.gameData.playerData.levelData [levelNumber - GameData.gameData.firstLevel].storyData [levelMode].isBossDead) {
					// Ajouter écran de mode Arcade
					// Ajouter écran de mode Hard
					presentation = "You've unlocked two new modes!";
					gain = "Hard + Arcade";
					listGainScreen.Add (new GainScreen (presentation, gain, ListManager.current.level [levelNumber - GameData.gameData.firstLevel]));
				}
			}
			/* Hard ou Hell */
			else if (levelMode == 1 || levelMode == 2) {
				if (!GameData.gameData.playerData.levelData [levelNumber - GameData.gameData.firstLevel].storyData [levelMode].isBossDead) {
					if (levelMode == 1) {
						// Ajouter écran de mode Hell si Hard
						presentation = "You've unlocked a new mode!";
						gain = "Hell";
						listGainScreen.Add (new GainScreen (presentation, gain, ListManager.current.level [levelNumber - GameData.gameData.firstLevel]));
					}
					// Ajouter écran de nouvelle arme
					item = SearchItem (levelNumber, levelMode, ItemType.weapon);
					if (item != null) {
						presentation = "You've unlocked a wonderful";
						listGainScreen.Add (new GainScreen (presentation, item.itemName, item.itemImage));
					}
				}
				// Le joueur n'a pas déjà enregistré le fait d'avoir un ratio
				if (GameData.gameData.playerData.levelData [levelNumber - GameData.gameData.firstLevel].storyData [levelMode].healthRatioRecord < GameData.gameData.ratioHealthShield
					&& Mathf.Clamp01 (LevelManager.player.minHealthRatio) >= GameData.gameData.ratioHealthShield) {
					// Ajouter écran de nouveau bouclier
					item = SearchItem (levelNumber, levelMode, ItemType.shield);
					if (item != null) {
						presentation = "You've unlocked a wonderful";
						listGainScreen.Add (new GainScreen (presentation, item.itemName, item.itemImage));
					}
				}
				if (GameData.gameData.playerData.levelData [levelNumber - GameData.gameData.firstLevel].storyData [levelMode].scoreRatioRecord < GameData.gameData.ratioScoreHelm
					&& ScoreManager.GetRatioLeaf () >= GameData.gameData.ratioScoreHelm) {
					// Ajouter écran de nouveau casque
					item = SearchItem (levelNumber, levelMode, ItemType.helm);
					if (item != null) {
						presentation = "You've unlocked a wonderful";
						listGainScreen.Add (new GainScreen (presentation, item.itemName, item.itemImage));
					}
				}
			}
			// Mise à jour des paramètres de fin de niveau (mort du boss)
			GameData.gameData.playerData.levelData [levelNumber - GameData.gameData.firstLevel].storyData [levelMode].isBossDead = true;
		}
		// En mode Arcade on ne peut que perdre
		else if (!LevelManager.levelManager.IsStory ()) {
			if (GameData.gameData.playerData.levelData [levelNumber - GameData.gameData.firstLevel].arcadeData.distanceRecord < GameData.gameData.distanceLimitPerfume
				&& LevelManager.levelManager.GetDistanceTraveled () >= GameData.gameData.distanceLimitPerfume) {
				// Ajouter écran de nouveau parfum
				item = SearchItem (levelNumber, levelMode, ItemType.perfume);
				if (item != null) {
					presentation = "You've unlocked a wonderful";
					listGainScreen.Add (new GainScreen (presentation, item.itemName, item.itemImage));
				}
			}
		}
		// Ajout du premier objet
		if (listGainScreen.Count > 0) {
			NewGainScreen ();
		}
	}

	private void NewGainScreen () {
		tnewItemPresentation.text = listGainScreen [0].presentationText;
		tnewItemObject.text = listGainScreen [0].gainText;
		newItem.sprite = listGainScreen [0].gainSprite;
		newItemBorder.sprite = listGainScreen [0].gainSprite;

		changeGainObject = true;
	}

	private ItemDescription SearchItem (int level, int mode, ItemType type) {
		foreach (ItemDescription item in ListManager.current.item) {
			if (item.itemLevel != level || item.itemMode != mode || item.itemType != type)
				continue;
			Debug.Log ("Gain item : " + item.itemName + " - Level demandé : " + level + " - Mode demandé : " + mode);
			return item;
		}
		return null;
	}

	private IEnumerator PopText (Text text, int number) {
		float timeScalePop = 0;
		float delayPop = 0.15f;
		float popScale = 0;
		float initialScale = text.rectTransform.localScale.x;

		if (number > 0) {
			text.gameObject.SetActive (true);
			text.text = number.ToString ("+0");
		}

		text.GetComponent<AudioSource> ().Play ();

		while (timeScalePop < delayPop) {
			popScale = Mathf.Lerp (initialScale / 2f, initialScale * 4f, timeScalePop);
			text.rectTransform.localScale = new Vector2 (popScale, popScale);

			timeScalePop += TimeManager.deltaTime;
			yield return null;
		}

		text.rectTransform.localScale = new Vector2 (initialScale, initialScale);
	}

	private IEnumerator PopObject (Transform transform) {
		float timeScalePop = 0;
		float delayPop = 0.15f;
		float popScale = 0;
		float initialScale = transform.localScale.x;

		while (timeScalePop < delayPop) {
			popScale = Mathf.Lerp (initialScale / 2f, initialScale * 4f, timeScalePop);
			transform.localScale = new Vector2 (popScale, popScale);

			timeScalePop += TimeManager.deltaTime;
			yield return null;
		}

		transform.localScale = new Vector2 (initialScale, initialScale);
	}

	public IEnumerator CombatText (Transform parentTransform, string combatLog, LogType type = LogType.special, System.Action<bool> callback = null) {
		GameObject obj = PoolingManager.current.Spawn ("CombatText");

		if (obj != null) {
			Text combatText = obj.GetComponent<Text> ();
			float floatingTime = 1f;
			float elapsedTime = 0;

			obj.SetActive (true);
			obj.transform.SetParent (transform, false);
			obj.transform.position = parentTransform.position;
			obj.transform.rotation = Quaternion.identity;
			combatText.text = combatLog;

			// Choix de la couleur
			Color startColor;
			switch (type) {
			case LogType.damage:
				startColor = damageColor;
				break;
			case LogType.heal:
				startColor = healColor;
				break;
			case LogType.pickup:
				startColor = pickupColor;
				break;
			case LogType.special:
			default:
				startColor = specialColor;
				break;
			}

			Color endColor = startColor;
			endColor.a = 0;

			combatText.color = startColor;

			// Le texte flotte au-dessus du character touché
			while (elapsedTime < floatingTime) {
				if(TimeManager.paused)
					combatText.color = endColor;
				else
					combatText.color = startColor;
					
				obj.transform.position = parentTransform.position + Vector3.up * (0.5f + elapsedTime);
				//combatText.color = Color.Lerp (startColor, endColor, elapsedTime);

				elapsedTime += TimeManager.deltaTime;
				yield return null;
			}

			combatText.color = endColor;
			obj.transform.SetParent (PoolingManager.pooledObjectParent, false);
			obj.SetActive (false);
		}

		// On prévient que l'action est terminée, si besoin
		if (callback != null)
			callback (true);
	}

	public void ResumeGame() {
		sfxSound.ButtonYesClick ();
		cdObject.SetActive (true);
		pauseUI.SetActive (false);
		standardUI.SetActive (true);
		paused = false;

		cdAnim.SetBool ("powerOn", true); // Animation de compte à rebours
	}

	public void Rejouer_Click() {
		sfxSound.ButtonYesClick ();
		if (paused)
			Time.timeScale = initialTimeScale;
		SceneManager.LoadScene (SceneManager.GetActiveScene().buildIndex);
	}

	public void Next_Click() {
		if (paused)
			Time.timeScale = initialTimeScale;
		// TODO Vérifier qu'on lance bien Arcade si Arcade, et la difficulté idem
		int nextLevel = SceneManager.GetActiveScene ().buildIndex + 1;
		if (nextLevel <= SceneManager.sceneCount)
			SceneManager.LoadScene (nextLevel);
		else
			List_Click ();	
	}
	
	public void Home_Click() {
		sfxSound.ButtonNoClick ();
		SceneManager.LoadScene (0);
	}

	public void List_Click() {
		_GameData.loadListLevel = true; // On demande à charger le menu du jeu (liste des levels)
		SceneManager.LoadScene (0);
	}

	public void Armory_Click() {
		_GameData.loadArmory = true; // On demande à charger l'armurerie (liste des équipements)
		SceneManager.LoadScene (0);
	}

	public void Quit_Click() {
		// Change le texte selon l'état du joueur : mort ou vivant ou pause
		string content;
		if (LevelManager.player.IsDead ()) {
			content = "Are you really sure you want to quit?\nYou just died, but don't be afraid. This kind of misadventure happens at least once in a life.";
		} else if (!paused) {
			content = "Are you really sure you want to quit?\nYou just succeeded, why would you leave?";
		} else {
			content = "Are you really sure you want to quit?\nYou can make a pause, and come back later!\nNothing will be save, you know? :(";
		}
		tQuitContent.text = content;

		sfxSound.ButtonYesClick ();
		wQuit.SetActive (true);
	}

	public void Quit_Yes_Click() {
		if (!paused)
			_StaticFunction.Save ();
		
		sfxSound.ButtonYesClick ();

		#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
		#else
		Application.Quit ();
		#endif
	}

	public void Quit_No_Click() {
		sfxSound.ButtonNoClick ();

		wQuit.SetActive (false);
	}
}