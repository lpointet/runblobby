using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class UIManager : MonoBehaviour {

	public static UIManager uiManager;

	[Header("UI standard")]
	public GameObject standardUI;

	// Compteur
	public Text meterText;
	private Color defaultTextColor; // Couleur par défaut du meterText
	private float scaleInitial; // Echelle du texte au début
	private Color warningTextColor; // Couleur en alternance lors d'un ennemi
	private Color warningTextColorBis;
	private float scaleFonctionDistance; // Echelle du texte pendant l'ennemi
	private float enemyDistanceToKill;

	// Distance parcourue
	public Slider meterTravelled;
	
	private Enemy enemyEnCours = null;
	private bool enemyGUIActive = false;
	public Text enemyName;
	public Text enemySurname;

	// Barre de vie
	public GameObject healthBar;
	public Image fillHealthBar;
	private float lerpingTimeEnemyBar = 0f;

	[Header("Pause Menu")]
	public GameObject pauseUI;
	public Text distancePause;
	public Text moneyPause;

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
	public Text playerLevel;

	public AudioClip endFailure;
	public AudioClip endVictory;
	private AudioSource endAudio;

	private float timeUpdateValue;
	private float delaySliderFull = 1.5f;

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
        warningTextColor = new Color( 1, 0.588f, 0.588f );
        warningTextColorBis = warningTextColor / 2;
        warningTextColorBis.r = 1f; // rouge à fond
    }

	void Start() {
		cdAnim = cdObject.GetComponent<Animator> ();

		pauseUI.SetActive (false);
		endUI.SetActive (false);
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
			StartCoroutine (PopText (playerLevel, "+" + diffLvl));
		}

        // Compteur
        MeterTextManager();
		if (LevelManager.levelManager.IsStory ())
			MeterBarManager ();

		EnemySpawnManager();
    
        EnemyManager();
    }

    public void PauseManager() { 
		if (!endUI.activeInHierarchy) {
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
			lerpingTimeEnemyBar += Time.unscaledDeltaTime / LevelManager.levelManager.enemySpawnDelay;
			fillHealthBar.fillAmount = Mathf.Lerp (0, 1, lerpingTimeEnemyBar);

			if (fillHealthBar.fillAmount == 1) {
				lerpingTimeEnemyBar = 0;
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
			textTimeTotal -= Time.unscaledDeltaTime;
		} else { // Si on n'est pas dans le compte-à-rebours, on cache les textes
			enemyName.gameObject.SetActive (false);
			enemySurname.gameObject.SetActive (false);
		}
    }

	private void MoveIntroEnemyText(Text movingText, float currentTime, float posBegin, float posEnd) {
		float posCurrent = Mathf.Lerp (posBegin, posEnd, currentTime);
		movingText.rectTransform.anchoredPosition = new Vector2 (posCurrent, movingText.rectTransform.anchoredPosition.y);
	}

    private void EnemyManager() {
        if( null != enemyEnCours ) {
            if( !enemyGUIActive ) {
                ToggleEnemyGUI( true );
            }
           fillHealthBar.fillAmount = enemyEnCours.GetHealthPoint() / (float)enemyEnCours.GetHealthPointMax();
        }
        else if( !LevelManager.levelManager.IsEnemyToSpawn() && enemyGUIActive ) {
            ToggleEnemyGUI( false );
        }
    }

    private void MeterTextManager() {
        if( null != enemyEnCours ) {
            enemyDistanceToKill = LevelManager.levelManager.GetEnemyDistanceToKill();
            meterText.text = Mathf.RoundToInt( enemyDistanceToKill ) + "m"; // Mise à jour de la distance restante pour tuer le boss
            meterText.color = Color.Lerp( warningTextColor, warningTextColorBis, Mathf.Sin( 2f * enemyDistanceToKill ) ); // Variation entre deux couleurs

            // Fonction type f(x) = ax² + b, avec a = (scaleMaxAtteint-1) / distanceMaxPossible² et b = 1
            scaleFonctionDistance = ( 2 / Mathf.Pow( enemyEnCours.GetDistanceToKill(), 2 ) ) * _StaticFunction.MathPower( enemyEnCours.GetDistanceToKill() - enemyDistanceToKill, 2 ) + 1;
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
        foreach (Transform obj in healthBar.transform)
            obj.gameObject.SetActive( active );
		enemyName.gameObject.SetActive (active);
		enemySurname.gameObject.SetActive (active);
    }

	public void ToggleEndMenu(bool active) {
		endUI.SetActive (active);
		standardUI.SetActive (!active);

		if (LevelManager.GetPlayer ().IsDead ()) {
			endUI.GetComponent<AudioSource> ().PlayOneShot (endFailure);
		} else {
			endUI.GetComponent<AudioSource> ().PlayOneShot (endVictory);
		}

		UpdateValueScore ();
	}

	private void UpdateValueScore() {
		// Partie Pause
		distancePause.text = Mathf.RoundToInt (LevelManager.levelManager.GetDistanceTraveled ()).ToString ();
		moneyPause.text = ScoreManager.GetScore ().ToString ();
		// Partie End Game
		distanceEnd.text = Mathf.RoundToInt (LevelManager.levelManager.GetDistanceTraveled ()).ToString ();
		moneyEnd.text = ScoreManager.GetScore ().ToString ();
		experience.text = ScoreManager.GetExperience ().ToString ();

		if (endUI.activeInHierarchy)
			StartCoroutine (UpdateSlider ());
	}

	private IEnumerator UpdateSlider() {
		distanceSliderEnd.maxValue = meterTravelled.maxValue;

		int currentPlayerLevel = _StaticFunction.LevelFromExp (GameData.gameData.playerData.experience);
		int nextPlayerLevel = currentPlayerLevel + 1;

		int currentPlayerExpInLevel = GameData.gameData.playerData.experience - _StaticFunction.ExpFromLevel (currentPlayerLevel);
		int futurPlayerExp = GameData.gameData.playerData.experience + ScoreManager.GetExperience ();

		int diffLevel = _StaticFunction.LevelFromExp (futurPlayerExp) - currentPlayerLevel;

		bool newLevel = true; // Savoir si l'on va passer un niveau complet ou non (servira en cas de multi level)
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
			if (experienceSliderEnd.value >= xpMaxThisLevel) {
				newLevel = true;
				currentLevelGain++;
				currentPlayerExpInLevel = 0;
				timeUpdateMultiLevel = 0;

				StartCoroutine (PopText (playerLevel, "+" + currentLevelGain));Debug.Log ("pouet");
			}

			// TODO ajouter un son filling

			timeUpdateValue += Time.unscaledDeltaTime;
			timeUpdateMultiLevel += Time.unscaledDeltaTime * (diffLevel + 1);
			yield return null;
		}
	}

	private IEnumerator PopText(Text text, string content) {
		float timeScalePop = 0;
		float delayPop = 0.2f;
		float popScale = 0;
		float initialScale = text.rectTransform.localScale.x;

		playerLevel.text = content;

		text.GetComponent<AudioSource> ().Play ();

		while (timeScalePop < delayPop) {
			popScale = Mathf.Lerp (initialScale, initialScale * 4f, timeScalePop);
			text.rectTransform.localScale = new Vector2 (popScale, popScale);

			timeScalePop += Time.unscaledDeltaTime;
			yield return null;
		}

		text.rectTransform.localScale = new Vector2 (initialScale, initialScale);
	}

	public void ResumeGame() {
		sfxSound.ButtonYesClick ();
		cdObject.SetActive (true);
		cdAnim.SetBool ("powerOn", true); // Animation de compte à rebours
		pauseUI.SetActive (false);
		standardUI.SetActive (true);
		paused = false;
	}

	public void Rejouer_Click() {
		SceneManager.LoadScene (SceneManager.GetActiveScene().buildIndex);
	}
	
	public void Home_Click() {
		sfxSound.ButtonNoClick ();
		SceneManager.LoadScene (0);
	}

	public void List_Click() {
		sfxSound.ButtonYesClick ();
		_GameData.current.SetListLevel (true); // On demande à charger le menu du jeu (liste des levels)
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
		#elif UNITY_STANDALONE
		Application.Quit ();
		#endif
	}

	public void Quit_No_Click() {
		sfxSound.ButtonNoClick ();

		wQuit.SetActive (false);
	}
}