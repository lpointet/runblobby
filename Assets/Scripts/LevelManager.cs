using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Tiled2Unity;

public class LevelManager : MonoBehaviour {

	public static LevelManager levelManager;
	private Camera kamera;
	private static PlayerController player;

	private AudioSource sourceSound;
	private float soundVolumeInit;

	private bool startLevel = false;
	private bool endLevel = false;

	// Mort, Respawn
	public GameObject currentCheckPoint;
	public float respawnDelay;

	[Header("Reset divers")]
	public Material moneyMat;
	public Color BaseColor;

	// Création du monde et déplacement
	[Header("Création du monde")]
	private int currentLevel;
	private int currentDifficulty;
	private bool isStory;

	public GameObject blockStart;
	private float heightStartBlock = 0.25f;
	public GameObject blockEnd;
	public GameObject[] blockEnemy;
	private List<GameObject> blockList;
	private int[][] probabiliteBlock; // Probabilités d'apparition de chaque block par phase
	private string[] listeDifficulte; // Liste des difficultés possibles pour les blocs

	private float sizeLastBlock;
	private float sizeFirstBlock;

	[HideInInspector] public float cameraStartPosition;
	[HideInInspector] public float cameraEndPosition;
	// Fin création du monde et déplacement

	// Distance parcourue
	private float distanceTraveled; // pendant la phase bloc
	private float localDistance; // variation permanente de la distance
	private float distanceSinceLastBonus; // distance depuis l'apparition du dernier bonus

	//* Partie Ennemi intermédiaire
	[Header("Ennemis/Boss")]
	public int[] listPhase;		// Valeur relative à parcourir avant de rencontrer un ennemi
	private int currentPhase;	// Phase en cours
	private bool blockPhase;	// Phase avec des blocs ou avec un ennemi
	public LayerMask layerNotGround; // Permet de supprimer les pièces que les ennemis "perdent", ou autres obstacles invoqués (bombes...)
	private bool premierBlock = false;	// Instantier le premier bloc ennemi
	public Enemy[] enemyMiddle;	// Liste des ennemis
	private Enemy enemyEnCours;
	private bool enemyToSpawn = false;	// Bool modifiable pour savoir à quel moment il faut invoquer l'ennemi
    private bool enemySpawnLaunched = false; // Bool pour savoir si l'appel du spawn a déjà été fait ou pas
	public float enemySpawnDelay;
	private float enemyDistanceToKill;
	public FlyPickup flyEndBoss;	// Pour faire voler à l'infini durant le dernier boss
	//* Fin partie ennemi intermédiaire

	public static PlayerController GetPlayer() {
		return player;
	}

	public bool IsBlockPhase() {
		return blockPhase;
	}

    public bool IsEnemyToSpawn() {
        return enemyToSpawn;
    }
    
    public void SetEnemyToSpawn( bool value ) {
        enemyToSpawn = value;
        if( value ) {
            enemySpawnLaunched = false;
        }
    }

    public Enemy GetEnemyEnCours() {
        return enemyEnCours;
    }

    public float GetEnemyDistanceToKill() {
        return enemyDistanceToKill;
    }

    public void SetEnemyDistanceToKill( float value ) {
        enemyDistanceToKill = value;
    }

    public int GetDistanceTraveled() {
		return Mathf.RoundToInt(distanceTraveled);
    }

	public int GetCurrentLevel() {
		return currentLevel;
	}

	public void SetCurrentLevel(int value) {
		currentLevel = value;
	}

	public int GetCurrentDifficulty() {
		return currentDifficulty;
	}

	public void SetCurrentDifficulty(int value) {
		currentDifficulty = value;
	}

	public bool IsStory() {
		return isStory;
	}

	public void SetStoryMode(bool value) {
		isStory = value;
	}

	public float GetLocalDistance() {
		return localDistance;
	}

	public void ResetBonusDistance() {
		distanceSinceLastBonus = 0;
	}

	public float GetDistanceSinceLastBonus() {
		return distanceSinceLastBonus;
	}

	public float GetHeightStartBlock() {
		return heightStartBlock;
	}

	public void StartLevel() {
		startLevel = true;
	}

	public bool IsLevelStarted() {
		return startLevel;
	}

    void Awake() {
		if (levelManager == null)
			levelManager = FindObjectOfType<LevelManager>();

		player = FindObjectOfType<PlayerController> ();
		kamera = Camera.main;
		sourceSound = GetComponent<AudioSource> ();
	}

	void Start () {
		// Reset divers
		ScoreManager.Reset ();

		distanceTraveled = 0;
		distanceSinceLastBonus = 0;
		currentPhase = 0;
		blockPhase = true;
        SetEnemyDistanceToKill( 0 );

		SetCurrentLevel (_GameData.currentLevel); // Information mise à jour par le UIManager > SampleLevel
		SetStoryMode (_GameData.isStory);
		SetCurrentDifficulty (_GameData.currentDifficulty);

		// Ajustement de l'apparition du boss de fin (par rapport à la distance maximum du level)
		listPhase [listPhase.Length - 1] = GameData.gameData.playerData.levelData[GetCurrentLevel ()].storyData[GetCurrentDifficulty ()].distanceMax;

		moneyMat.SetColor("_BaseColor", BaseColor);
		moneyMat.SetColor("_TargetColor", BaseColor);

		// On commence le niveau dans une phase "block" et non une phase "ennemi", le joueur ne peut donc pas tirer
		GetPlayer().SetFireAbility( false );

		soundVolumeInit = sourceSound.volume;
		PlayBackgroundMusic ();
		// Fin reset divers

		listeDifficulte = new string[5] {"difficulty_1", "difficulty_2", "difficulty_3", "difficulty_4", "difficulty_5"};
		// Autant de probabilité que de phases (voir listPhase)
		probabiliteBlock = new int[listPhase.Length][];
		probabiliteBlock[0] = new int[5] {70, 30,  0,  0,  0};
		probabiliteBlock[1] = new int[5] { 0, 40, 50, 10,  0};
		probabiliteBlock[2] = new int[5] { 0, 0,  20, 60, 20};
		// On met à 20 au cas où on dépasse les 3 phases et qu'on oublie
		for (int i = 3; i < listPhase.Length; i++) {
			probabiliteBlock [i] = new int[5] {20, 20, 20, 20, 20};
		}

		cameraStartPosition = kamera.transform.position.x - kamera.orthographicSize * kamera.aspect - 3;
		cameraEndPosition = kamera.transform.position.x + kamera.orthographicSize * kamera.aspect + 5;

        // Initialisation avec le Block Start
        blockList = new List<GameObject> {blockStart};
		// ICI : Instantiate si jamais c'est un prefab
		blockList[0].transform.position = new Vector3(-1, -1, 0) * heightStartBlock; // Juste sous le joueur, un peu en avant
		sizeLastBlock = blockList[0].GetComponent<TiledMap> ().NumTilesWide;
		sizeFirstBlock = sizeLastBlock;
		//layerCoins = LayerMask.NameToLayer ("Coins");

		StartLevel (); // TODO SUPPRIMER CETTE LIGNE + ACTIVER STARTLEVEL
	}

	void Update () {
		// On ne commence pas avant le début... hé oué !
		if (!IsLevelStarted ())
			return;

		// Empêcher que des choses se passent durant la pause
		// En plus de baisser le volume si l'écran de fin arrive
		if (TimeManager.paused || GetPlayer().IsDead () || endLevel) {
			sourceSound.volume = 0.1f;
			return;
		}

		// Faire apparaître le menu de fin si on est en mode histoire et qu'on a tué le dernier boss
		if (currentPhase >= listPhase.Length && IsStory() && !endLevel) {
			FinDuMonde ();
		}

		sourceSound.volume = soundVolumeInit;
	
		// Distance parcourue depuis le dernier update
		localDistance = GetPlayer().GetMoveSpeed() * Time.smoothDeltaTime;

		// Définir dans quelle phase on se situe
		if (currentPhase < listPhase.Length && GetDistanceTraveled() > listPhase[currentPhase]) {
			blockPhase = false;

			// On créé le premier bloc qui n'est pas un bloc du milieu
			if(!premierBlock) {
				PositionBlock(Instantiate(blockEnemy[0]));
				premierBlock = true;
			}

			// Variable changée par la classe StartEnemyBlock sur le OnTriggerEnter2D, marque le début du compte à rebours pour le boss
			if( IsEnemyToSpawn() && !enemySpawnLaunched ) {
                StartCoroutine( SpawnEnemy( enemyMiddle[currentPhase] ) );
                enemySpawnLaunched = true;

				// On fait voler le joueur si c'est le dernier ennemi
				if (currentPhase == listPhase.Length - 1) {
					CleanPickup( GetPlayer().GetLastWish() );
					// On créer un pickup de vol sur le joueur, en vol infini (1000s...)
					GetPlayer ().SetZeroGravFlying (true);
					flyEndBoss.lifeTime = 1000;
					flyEndBoss = Instantiate (flyEndBoss, GetPlayer ().transform.position, Quaternion.identity) as FlyPickup;
					flyEndBoss.gameObject.SetActive (true);
				}
            }
			
			if(enemyEnCours != null) {
				// Le joueur peut tirer
				GetPlayer().SetFireAbility( true );

                // Si on est en phase "ennemie" et qu'on a dépassé la distance allouée pour le tuer, on meurt
                SetEnemyDistanceToKill( GetEnemyDistanceToKill() - localDistance );

				if( GetEnemyDistanceToKill() <= 0 ) {
					LevelManager.Kill( GetPlayer() );
				}

				// On créé le dernier bloc qui n'est pas un bloc du milieu
				// Quand l'ennemi est mort
				if( enemyEnCours.IsDead() ) {
					enemyEnCours = null;
					PositionBlock(Instantiate(blockEnemy[1]));

					currentPhase++;
					premierBlock = false;

                    // Le joueur ne peut plus tirer
					GetPlayer().SetFireAbility( false );

					// On fait accélérer le joueur à chaque nouvelle phase bloc
					GetPlayer().SetMoveSpeed(GetPlayer().GetMoveSpeed() * 1.15f);
				}
			}
		}
		// Si on n'est pas dans une phase "ennemie", on est dans une phase "bloc"
		else {
			blockPhase = true;
		}

        // On actualise la distance parcourue si le joueur n'est pas mort, et que l'ennemi n'est pas là
		if (!GetPlayer().IsDead() && !IsEnemyToSpawn() && enemyEnCours == null) {
			distanceTraveled += localDistance;
			distanceSinceLastBonus += localDistance;
		}

		// Suppression du premier bloc dès qu'il disparait de la caméra
		if (blockList [0].transform.position.x + sizeFirstBlock < cameraStartPosition) {
			// On supprime les objets qui ne sont pas sur la couche "Ground" si on est sur les blocks du boss
			// Supprime les pièces, les bombes...
			if(blockList[0].name.Contains(blockEnemy[1].name)) {
				foreach (Transform t in blockList[0].GetComponentsInChildren(typeof(Transform), true)) {
					if ((1 << t.gameObject.layer & layerNotGround) != 0) {
						t.parent = null;
						t.gameObject.SetActive (false);
					}
				}
			}
			blockList [0].SetActive (false);
			blockList.RemoveAt (0);
			
			sizeFirstBlock = blockList [0].GetComponent<TiledMap> ().NumTilesWide;
		}
		
		// Création du prochain bloc si le dernier bloc en cours approche de la fin de la caméra
		if (blockList [blockList.Count - 1].transform.position.x + sizeLastBlock < cameraEndPosition) {
			PositionBlock (GetNewBlock (blockPhase));
		}

        // Si le joueur n'est pas mort, on bouge le monde
        if (!GetPlayer().IsDead() || GetPlayer().HasLastWish()) {
			MoveWorld ();
		}
	}

	private GameObject GetNewBlock(bool _blockPhase) {
		if (_blockPhase) {
			string randomBlock = PoolingManager.current.RandomPoolName ("Block", RandomDifficulty(currentPhase)); // Random Block de difficulté adaptée à la currentPhase
			return PoolingManager.current.Spawn (randomBlock);
		}
		else
			return PoolingManager.current.Spawn ("BasiqueGround");
	}

	private string RandomDifficulty(int phase) {
		int[] probabilite; // liste des probabilités d'appeler le choixDifficulte
		int sum = 0;

		// On retourne la difficulté la plus facile si jamais on envoie une valeur de phase incorrecte
		if (phase >= listPhase.Length)
			return listeDifficulte[0];

		probabilite = probabiliteBlock[phase]; // Défini dans Start()

		// On calcule la somme des proportions pour choisir un nombre aléatoire là-dedans
		for(int i = 0; i < probabilite.Length; sum += probabilite[i++]);
		int random = Random.Range (0, sum); // Nombre entre 0 et sum exclus

		int k, choix;
		// On ajoute à k la valeur de la proportion si jamais k est inférieur à random, et on incrémente choix
		for(k = 0, choix = 0; k <= random; k += probabilite[choix++]);

		return listeDifficulte [choix-1];
	}

	private void PositionBlock(GameObject obj) {
		if (obj == null)
			return;

		// On cherche le dernier élément (vu qu'on place tout par rapport à lui)
		GameObject lastBlock = blockList[blockList.Count-1];
		
		obj.transform.position = lastBlock.transform.position + Vector3.right * sizeLastBlock;
		obj.transform.rotation = lastBlock.transform.rotation;
        _StaticFunction.SetActiveRecursively(obj, true); // Normalement SetActive(true);

		sizeLastBlock = obj.GetComponent<TiledMap>().NumTilesWide;
		
		blockList.Add (obj); // On ajoute à la liste le bloc
	}

	private void MoveWorld() {
		foreach(GameObject block in blockList) {
			block.transform.Translate (Vector2.left * localDistance);
		}
	}

	private IEnumerator SpawnEnemy(Enemy enemy) {
		Enemy tempEnemy = Instantiate(enemy) as Enemy; // Permet d'accéder à l'ennemi avant qu'il ne soit visible pour le joueur

		UIManager.uiManager.enemyName.text = tempEnemy.GetName ();
		UIManager.uiManager.enemySurname.text = tempEnemy.GetSurName ();

		yield return new WaitForSeconds( enemySpawnDelay * Time.timeScale );

		enemyEnCours = tempEnemy;
		tempEnemy = null;

		enemyDistanceToKill = enemyEnCours.GetDistanceToKill();
	}

	private void FinDuMonde () {
		if (IsStory ()) {
			GetPlayer ().OnVictory ();
			CleanPickup( GetPlayer().GetLastWish() );

			// On offre des points d'xp supplémentaires si c'est la première fois qu'il tue le boss
			if (!GameData.gameData.playerData.levelData [GetCurrentLevel ()].storyData [GetCurrentDifficulty ()].isBossDead) {
				ScoreManager.AddPoint (50 + 25 * GetCurrentLevel (), ScoreManager.Types.Experience); // TODO ajuster la fonction xp
			} else { // Si on a déjà tué le boss
				ScoreManager.AddPoint (10 * GetCurrentLevel (), ScoreManager.Types.Experience); // TODO ajuster la fonction xp
			}

			UIManager.uiManager.ToggleEndMenu (true);

			endLevel = true;
		}
	}

	public static void Kill( Character character ) {
		LastWishPickup lastWish = GetPlayer().GetLastWish();

		if (character == GetPlayer ()) {
			CleanPickup (lastWish);

			if (lastWish == null || lastWish.IsLaunched ()) {
				character.Die ();
				character.OnKill ();

			} else if (lastWish != null) {
				lastWish.Launch ();
			}
		} else {
			character.Die ();
			character.OnKill ();
		}
	}

	public static void MaybeKill( Transform transform ) {
		Enemy enemy = transform.GetComponent<Enemy>();
        Animator deathAnim = transform.GetComponent<Animator>();

        if ( null != enemy ) {
            Kill( enemy );
		}
		else {
            if (deathAnim)
                deathAnim.SetBool("dead", true);
            else
			    transform.gameObject.SetActive(false);
		}
	}

	public static void CleanPickup (LastWishPickup lastWish = null) {
		Pickup[] pickups = GetPlayer().GetComponentsInChildren<Pickup>();

		foreach( Pickup pickup in pickups ) {
			if( pickup != lastWish || lastWish.IsLaunched() ) {
				pickup.Disable();
			}
		}

		if( lastWish != null && lastWish.IsLaunched() ) {
			lastWish.Cancel();
		}
	}

	private void PlayBackgroundMusic() {
		if (sourceSound.isPlaying)
			return;

		sourceSound.Play ();
	}
}
