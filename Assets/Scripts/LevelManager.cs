using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Tiled2Unity;

public class LevelManager : MonoBehaviour {

	public static LevelManager levelManager;
	private Camera kamera;
	public static PlayerController player;

	private AudioSource sourceSound;
	private float soundVolumeInit;

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
	public GameObject blockEnd;
	public GameObject[] blockEnemy;
	private List<GameObject> blockList;
	private int[][] probabiliteBlock; // Probabilités d'apparition de chaque block par phase
	private string[] listeDifficulte; // Liste des difficultés possibles pour les blocs

	private float sizeLastBlock;
	private float sizeFirstBlock;

	[HideInInspector] public float cameraStartPosition;
	private float cameraEndPosition;
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
	//* Fin partie ennemi intermédiaire

	private float fps;

	public static PlayerController GetPlayer() {
		return player;
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

    void Awake() {
		if (levelManager == null)
			levelManager = GameObject.FindGameObjectWithTag ("GameMaster").GetComponent<LevelManager> ();

		player = FindObjectOfType<PlayerController> ();
		kamera = Camera.main;
		sourceSound = GetComponent<AudioSource> ();
	}

	void Start () {
		// Reset divers
		Time.timeScale = 1;
		ScoreManager.Reset ();

		distanceTraveled = 0;
		distanceSinceLastBonus = 0;
		currentPhase = 0;
		blockPhase = true;
        SetEnemyDistanceToKill( 0 );

		SetCurrentLevel (1); // TODO l'information doit venir du MainMenuManager
		SetStoryMode (true); // TODO idem
		SetCurrentDifficulty (0); // TODO idem

		moneyMat.SetColor("_BaseColor", BaseColor);
		moneyMat.SetColor("_TargetColor", BaseColor);
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

		cameraStartPosition = kamera.transform.position.x - kamera.orthographicSize * kamera.aspect - 1;
		cameraEndPosition = kamera.transform.position.x + kamera.orthographicSize * kamera.aspect + 1;

        // Initialisation avec le Block Start
        blockList = new List<GameObject> {blockStart};
		// ICI : Instantiate si jamais c'est un prefab
		blockList[0].transform.position = player.transform.position + Vector3.down; // Juste sous le joueur
		sizeLastBlock = blockList[0].GetComponent<TiledMap> ().NumTilesWide;
		sizeFirstBlock = sizeLastBlock;
		//layerCoins = LayerMask.NameToLayer ("Coins");

        // On commence le niveau dans une phase "block" et non une phase "ennemi", le joueur ne peut donc pas tirer
        player.SetFireAbility( false );

		soundVolumeInit = sourceSound.volume;
		PlayBackgroundMusic ();

		fps = 1.0f / Time.fixedDeltaTime;
	}

	void Update () {
		// Empêcher que des choses se passent durant la pause
		if (Time.timeScale == 0 || player.IsDead ()) {
			sourceSound.volume = 0.1f;
			return;
		}

		// Faire apparaître le menu de fin si on est en mode histoire et qu'on a tué le dernier boss
		if (currentPhase >= listPhase.Length && IsStory()) {
			FinDuMonde ();
		}

		sourceSound.volume = soundVolumeInit;
	
		// Distance parcourue depuis le dernier update
		//localDistance = player.GetMoveSpeed() * Time.smoothDeltaTime;
		localDistance = player.GetMoveSpeed () / fps; // TODO choisir un des deux, celui-ci a l'air plus fluide

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
            }
			
			if(enemyEnCours != null) {
				// Le joueur peut tirer
				player.SetFireAbility( true );

                // Si on est en phase "ennemie" et qu'on a dépassé la distance allouée pour le tuer, on meurt
                SetEnemyDistanceToKill( GetEnemyDistanceToKill() - localDistance );

				if( GetEnemyDistanceToKill() <= 0 ) {
					LevelManager.Kill( player, true );
				}

				// On créé le dernier bloc qui n'est pas un bloc du milieu
				// Quand l'ennemi est mort
				if( enemyEnCours.IsDead() ) {
					enemyEnCours = null;
					PositionBlock(Instantiate(blockEnemy[1]));

					currentPhase++;
					premierBlock = false;

                    // Le joueur ne peut plus tirer
                    player.SetFireAbility( false );
				}
			}
		}
		// Si on n'est pas dans une phase "ennemie", on est dans une phase "bloc"
		else {
			blockPhase = true;
		}

        // On actualise la distance parcourue si le joueur n'est pas mort, et que l'ennemi n'est pas là
        if (!player.IsDead () && !IsEnemyToSpawn() && enemyEnCours == null) {
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
        if (!player.IsDead() || player.HasLastWish()) {
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
		tempEnemy.gameObject.SetActive (false);
		UIManager.uiManager.enemyName.text = tempEnemy.GetName ();
		UIManager.uiManager.enemySurname.text = tempEnemy.GetSurName ();
        yield return new WaitForSeconds( enemySpawnDelay );
		//Vector2 enemyTransform = new Vector2 (player.transform.position.x + 14, player.transform.position.y);
		//enemyEnCours = Instantiate (enemy, enemyTransform, player.transform.rotation) as Enemy;
		enemyEnCours = tempEnemy;
		tempEnemy = null;
		enemyEnCours.transform.position = enemyEnCours.GetStartPosition ();
		enemyEnCours.transform.rotation = Quaternion.identity;
		enemyDistanceToKill = enemyEnCours.GetDistanceToKill();
		enemyEnCours.gameObject.SetActive (true);
	}

	public bool IsBlockPhase() {
		return blockPhase;
	}

	private void FinDuMonde () {
		//LevelManager.Kill( player, true ); // TODO adapter le code pour la fin du niveau en mode histoire
		UIManager.uiManager.ToggleEndMenu(true);
		CleanPickup (true);
	}

	public static void Kill( Character character, bool ignoreLastWish = false ) {
//		if (character.IsDead ()) // Ce qui te tue ne peut pas te rendre plus mort.
//			return;
		
		if( character == GetPlayer() ) {
			CleanPickup (ignoreLastWish);
		}

		character.Die();
		
		character.OnKill();
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

	private static void CleanPickup (bool ignoreLastWish = false) {
		Pickup[] pickups = GetPlayer().GetComponentsInChildren<Pickup>();
		LastWishPickup lastWish = GetPlayer().GetLastWish();

		foreach( Pickup pickup in pickups ) {
			if( pickup != lastWish || lastWish.IsLaunched() ) {
				pickup.Disable();
			}
		}

		if( ignoreLastWish && lastWish != null ) {
			lastWish.Cancel();
		}
	}

	private void PlayBackgroundMusic() {
		if (sourceSound.isPlaying)
			return;

		sourceSound.Play ();
	}

	public void ResetBonusDistance() {
		distanceSinceLastBonus = 0;
	}

	public float GetDistanceSinceLastBonus() {
		return distanceSinceLastBonus;
	}
}
