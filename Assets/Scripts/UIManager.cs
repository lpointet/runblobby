using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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

	[Header("End Menu")]
	public GameObject endUI;
	private SFXMenu sfxSound;

	public Text distanceEnd;
	public Text moneyEnd;
	public Text experience;
	public Text playerLevel;

	public AudioClip endFailure;
	public AudioClip endVictory;
	private AudioSource endAudio;

	// Déplacement des noms des ennemis
	private Vector2 namePosition;
	private Vector2 surnamePosition;
	private Vector2 positionNameBeforeEndMove;
	private Vector2 positionSurnameBeforeEndMove;
	private float textTimeToMid;
	private float textTimeMid;
	private float textTimeToEnd;
	private float textTimeTotal;

    void Awake() {
		if (uiManager == null)
			uiManager = GameObject.FindGameObjectWithTag ("LevelCanvas").GetComponent<UIManager> ();

        // Compteur
        defaultTextColor = meterText.color;
        scaleInitial = meterText.rectTransform.localScale.x;
        warningTextColor = new Color( 1, 0.588f, 0.588f );
        warningTextColorBis = warningTextColor / 2;
        warningTextColorBis.r = 1f; // rouge à fond
    }

	void Start() {
		pauseUI.SetActive (false);
		cdAnim = cdObject.GetComponent<Animator> ();

		endUI.SetActive (false);

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
	}
	
	void Update() {
		enemyEnCours = LevelManager.levelManager.GetEnemyEnCours();

        PauseManager();

        EnemySpawnManager();

        // Compteur
        MeterTextManager();
    }
    
    void OnGUI() {
        EnemyManager();
    }

    private void PauseManager() { 
		if (Input.GetButtonDown ("Pause") && !endUI.activeInHierarchy) {
			paused = !paused;

			if (paused) {
				TogglePauseMenu (true);
				UpdateValueScore ();
			} else {
				TogglePauseMenu (false);
			}
		}
    }

    private void EnemySpawnManager() {
        // Variable changée par la classe StartEnemyBlock sur le OnTriggerEnter2D, marque le début du compte à rebours pour le boss
		if( LevelManager.levelManager.IsEnemyToSpawn() ) {
			// On affiche la barre de vie vide, pour pouvoir la remplir le temps du spawn (= timer d'apparition)
            if( !enemyGUIActive ) {
                ToggleEnemyGUI( true );
            }

            // On remplit la barre de vie en fonction du temps de spawn 100% = ennemi apparait
            lerpingTimeEnemyBar += Time.deltaTime / LevelManager.levelManager.enemySpawnDelay;
			fillHealthBar.fillAmount = Mathf.Lerp (0, 1, lerpingTimeEnemyBar);

			if(fillHealthBar.fillAmount == 1) {
				lerpingTimeEnemyBar = 0;
                LevelManager.levelManager.SetEnemyToSpawn( false ); // On sort de la boucle d'apparition
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
			} else if (textTimeTotal <= 0) {
				enemyName.enabled = false;
				enemySurname.enabled = false;
			}
			textTimeTotal -= Time.deltaTime;
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

	private void TogglePauseMenu(bool active) {
		pauseUI.SetActive (active);
		standardUI.SetActive (!active);

		Time.timeScale = active ? 0 : 1;
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
	}

	public void ResumeGame() {
		cdObject.SetActive (true);
		cdAnim.SetBool ("powerOn", true); // Animation de compte à rebours
		pauseUI.SetActive(false);
		standardUI.SetActive (true);
		paused = false;
		// A la fin de l'animation, le timeScale redevient 1
		// S'il faut passer un paramètre de timeScale (si jamais il n'est pas à 1), il faudra passer un "faux" paramètre d'animator
	}

	public void Rejouer_Click() {
		//endUI.SetActive (false);
		SceneManager.LoadScene (SceneManager.GetActiveScene().buildIndex);
	}
	
	public void Home_Click() {
		SceneManager.LoadScene (0);
	}
	
	public void Quit() {
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE
		Application.Quit ();
#endif
	}
}