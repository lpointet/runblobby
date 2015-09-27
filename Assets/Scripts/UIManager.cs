using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour {

	public GameObject PauseUI;
	public GameObject cdObject;
	private Animator cdAnim;
	
	private bool paused = false;
    private GameObject healthBar;
    private Image fillHealthBar;
    private float lerpingTimeEnemyBar = 0f;
    private Enemy EnemyEnCours = null;
    private bool EnemyGUIActive = false;

    void Awake() {
        healthBar = GameObject.Find("HPBarEnemy");
        fillHealthBar = healthBar.GetComponent<RectTransform>().FindChild("HPBarEnemyFill").GetComponent<Image>();
    }

	void Start() {
		PauseUI.SetActive (false);
		cdAnim = cdObject.GetComponent<Animator> ();
	}
	
	void Update() {
        //* MENU PAUSE
        PauseManager();
        //* FIN MENU PAUSE

        EnemySpawnManager();
    }
    
    void OnGUI() {
        EnemyManager();
    }

    private void PauseManager() { 
		if (Input.GetButtonDown ("Pause")) {
			paused = !paused;

			if (paused) {
				PauseUI.SetActive (true);
				Time.timeScale = 0;
			} else {
				PauseUI.SetActive(false);
				Time.timeScale = 1;
			}
		}
    }

    private void EnemySpawnManager() {
        // Variable changée par la classe StartEnemyBlock sur le OnTriggerEnter2D, marque le début du compte à rebours pour le boss
		if( LevelManager.levelManager.IsEnemyToSpawn() ) {
			// On affiche la barre de vie vide, pour pouvoir la remplir le temps du spawn (= timer d'apparition)
            if( !EnemyGUIActive ) {
                ToggleEnemyGUI( true );
            }

            // On remplit la barre de vie en fonction du temps de spawn 100% = ennemi apparait
            lerpingTimeEnemyBar += Time.deltaTime / LevelManager.levelManager.enemySpawnDelay;
			fillHealthBar.fillAmount = Mathf.Lerp (0, 1, lerpingTimeEnemyBar);

			if(fillHealthBar.fillAmount == 1) {
				lerpingTimeEnemyBar = 0;
                LevelManager.levelManager.SetEnemyToSpawn( false ); // On sort de la boucle d'apparition
			}
		}
    }

    private void EnemyManager() {
        EnemyEnCours = LevelManager.levelManager.GetEnemyEnCours();
        if( null != EnemyEnCours ) {
            if( !EnemyGUIActive ) {
                ToggleEnemyGUI( true );
            }
           fillHealthBar.fillAmount = EnemyEnCours.GetHealthPoint() / (float)EnemyEnCours.GetHealthPointMax();
        }
        else if( !LevelManager.levelManager.IsEnemyToSpawn() && EnemyGUIActive ) {
            ToggleEnemyGUI( false );
        }
    }

    private void ToggleEnemyGUI( bool active ) {
        EnemyGUIActive = active;
        foreach (Transform obj in healthBar.transform)
            obj.gameObject.SetActive( active );
    }

	public void ResumeGame() {
		cdObject.SetActive (true);
		cdAnim.SetBool ("powerOn", true); // Animation de compte à rebours
		PauseUI.SetActive(false);
		paused = false;
		// A la fin de l'animation, le timeScale redevient 1
		// S'il faut passer un paramètre de timeScale (si jamais il n'est pas à 1), il faudra passer un "faux" paramètre d'animator
	}
	
	public void MainMenu() {
		
	}
	
	public void Quit() {
		Application.Quit ();
	}
}
