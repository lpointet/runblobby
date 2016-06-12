using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

// Une ligne de dialogue : le numéro de portrait, la position (gauche/droite), le texte
[System.Serializable]
public struct DialogEntry {
	public enum PortraitPosition { left, right };

	public int numCharacter;
	public PortraitPosition portraitPosition;
	[TextArea(3,10)] public string textLine;
}

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

	// Barre de vie
	private float lerpingTimeEnemyBar = 0f;
	public Slider enemyHealthBar;
	private float previousHP = 0;

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
	public Text experience;
	public Slider experienceSliderEnd;
	public Text gainLevel;
	public Text tCurrentPlayerLevel;
	public GameObject bNextLevel;
	public ParticleSystem pFirework;
	public Text tTitreEndGame;

	public AudioClip endFailure;
	public AudioClip endVictory;
	private AudioSource endAudio;

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

	void Update() {
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
    
		EnemyManager();

		EnemySpawnManager();
    }

    public void PauseManager() { 
		if (!endUI.activeInHierarchy && !dialogUI.activeInHierarchy) {
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
			}

			// On remplit la barre de vie en fonction du temps de spawn 100% = ennemi apparait
			lerpingTimeEnemyBar += TimeManager.deltaTime / LevelManager.levelManager.enemySpawnDelay;
			enemyHealthBar.value = Mathf.Lerp (0, 1, lerpingTimeEnemyBar);

			if (enemyHealthBar.value == 1) {
				enemyHealthBar.value = 1;
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
			}
			textTimeTotal -= TimeManager.deltaTime;
		} else { // Si on n'est pas dans le compte-à-rebours, on cache les textes
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
				 // TODO pourquoi est-ce qu'on perd une frame ?
			}
			float realRatioHP = enemyEnCours.GetHealthPoint() / (float)enemyEnCours.GetHealthPointMax();

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
			meterText.text = Mathf.RoundToInt( enemyTimeToKill ) + "s"; // Mise à jour du temps restant pour tuer le boss
			meterText.color = Color.Lerp( warningTextColor, warningTextColorBis, Mathf.Sin( 7f * enemyTimeToKill ) ); // Variation entre deux couleurs

            // Fonction type f(x) = ax² + b, avec a = (scaleMaxAtteint-1) / distanceMaxPossible² et b = 1
			scaleFonctionDistance = ( 2 / Mathf.Pow( enemyEnCours.GetTimeToKill(), 2 ) ) * _StaticFunction.MathPower( enemyEnCours.GetTimeToKill() - enemyTimeToKill, 2 ) + 1;
            meterText.transform.localScale = new Vector2( scaleFonctionDistance, scaleFonctionDistance ) * scaleInitial;
        }
        else if( !LevelManager.GetPlayer().IsDead() && !LevelManager.levelManager.IsEnemyToSpawn() ) {
            meterText.text = Mathf.RoundToInt( LevelManager.levelManager.GetDistanceTraveled() ) + "m"; // Mise à jour de la distance parcourue affichée
            meterText.color = defaultTextColor;
            meterText.transform.localScale = new Vector2( scaleInitial, scaleInitial );
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

		enemyHealthBar.gameObject.SetActive (active);
    }

	public void ToggleEndMenu(bool active) {
		endUI.SetActive (active);
		standardUI.SetActive (!active);

		if (LevelManager.GetPlayer ().IsDead ()) {
			endUI.GetComponent<AudioSource> ().PlayOneShot (endFailure);
			bNextLevel.SetActive (false);
			tTitreEndGame.text = "YOU LOSE...";
			pFirework.gameObject.SetActive (false);
		} else {
			endUI.GetComponent<AudioSource> ().PlayOneShot (endVictory);
			bNextLevel.SetActive (true);
			tTitreEndGame.text = "YOU WIN!";
			pFirework.gameObject.SetActive (true);
		}

		UpdateValueScore ();
	}

	public IEnumerator RunDialog(RuntimeAnimatorController[] protagonist, DialogEntry[] dialogLine, System.Action<bool> callback) {
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
		//Debug.Log(_StaticFunction.LevelFromExp (GameData.gameData.playerData.experience) + " " + currentPlayerLevel);
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
	}

	private IEnumerator PopText(Text text, int valueGainLevel) {
		float timeScalePop = 0;
		float delayPop = 0.2f;
		float popScale = 0;
		float initialScale = text.rectTransform.localScale.x;

		if (valueGainLevel > 0) {
			gainLevel.gameObject.SetActive (true);
			gainLevel.text = "+" + valueGainLevel;
		}

		text.GetComponent<AudioSource> ().Play ();

		while (timeScalePop < delayPop) {
			popScale = Mathf.Lerp (initialScale, initialScale * 4f, timeScalePop);
			text.rectTransform.localScale = new Vector2 (popScale, popScale);

			timeScalePop += TimeManager.deltaTime;
			yield return null;
		}

		text.rectTransform.localScale = new Vector2 (initialScale, initialScale);
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
		//sfxSound.ButtonYesClick ();
		_GameData.loadListLevel = true; // On demande à charger le menu du jeu (liste des levels)
		SceneManager.LoadScene (0);
	}

	public void Quit_Click() {
		// Change le texte selon l'état du joueur : mort ou vivant ou pause
		string content;
		if (LevelManager.GetPlayer ().IsDead ()) {
			content = "Are you really sure you want to quit?\nYou just died, but don't be afraid. This kind of misadventure happens at least once in a life.";
		} else if (!paused) {
			content = "Are you really sure you want to quit?\nYou just succeeded, why would you leave?";
		} else {
			content = "Are you really sure you want to quit?\nYou can make a pause, and come back later!";
		}
		tQuitContent.text = content;

		sfxSound.ButtonYesClick ();
		wQuit.SetActive (true);
	}

	public void Quit_Yes_Click() {
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