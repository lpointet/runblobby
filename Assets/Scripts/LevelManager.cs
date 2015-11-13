using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using Tiled2Unity;

public class LevelManager : MonoBehaviour {

	public static LevelManager levelManager;
	private Camera kamera;
	public static PlayerController player;
	//public Transform[] backgrounds;			// Array des backgrounds et foregrounds

	private AudioSource sourceSound;

	// Mort, Respawn
	//public GameObject deathEffect;
	public GameObject currentCheckPoint;
	public float respawnDelay;

	// Création du monde et déplacement
	public float cameraStartPosition;
	private float cameraEndPosition;

	public GameObject blockStart;
	public GameObject blockEnd;
	public GameObject[] blockEnemy;
	private List<GameObject> blockList;

	private float sizeLastBlock;
	private float sizeFirstBlock;
	// Fin création du monde et déplacement

	// Distance parcourue
	private float distanceTraveled; // pendant la phase bloc
	private float localDistance; // variation permanente de la distance
	private float distanceSinceLastBonus; // distance depuis l'apparition du dernier bonus

	//* Partie Ennemi intermédiaire
	public int[] listPhase;		// Valeur relative à parcourir avant de rencontrer un ennemi
	private int currentPhase;	// Phase en cours
	private bool blockPhase;	// Phase avec des blocs ou avec un ennemi
	private bool premierBlock = false;	// Instantier le premier bloc ennemi
	public Enemy[] enemyMiddle;	// Liste des ennemis
	private Enemy enemyEnCours;
	private bool enemyToSpawn = false;	// Bool modifiable pour savoir à quel moment il faut invoquer l'ennemi
    private bool enemySpawnLaunched = false; // Bool pour savoir si l'appel du spawn a déjà été fait ou pas
	public float enemySpawnDelay;
	private float enemyDistanceToKill;
	public int[][] probabiliteBlock; // Probabilités d'apparition de chaque block par phase
	private string[] listeDifficulte; // Liste des difficultés possibles
	//* Fin partie ennemi intermédiaire

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

    public float GetDistanceTraveled() {
        return distanceTraveled;
    }

    void Awake() {
		if (levelManager == null)
			levelManager = GameObject.FindGameObjectWithTag ("GameMaster").GetComponent<LevelManager> ();

		player = FindObjectOfType<PlayerController> ();
		kamera = Camera.main;
		sourceSound = GetComponent<AudioSource> ();
	}

	void Start () {
		Time.timeScale = 1;

		distanceTraveled = 0;
		distanceSinceLastBonus = 0;
		currentPhase = 0;
		blockPhase = true;
        SetEnemyDistanceToKill( 0 );

		listeDifficulte = new string[5] {"difficulty_1", "difficulty_2", "difficulty_3", "difficulty_4", "difficulty_5"};
		// Autant de probabilité que de phases (voir listPhase)
		probabiliteBlock = new int[listPhase.Length][];
		probabiliteBlock[0] = new int[5] {70, 30,  0,  0,  0};
		probabiliteBlock[1] = new int[5] { 0, 40, 50, 10,  0};
		probabiliteBlock[2] = new int[5] { 0, 0,  20, 60, 20};
		// On met à 0 au cas où on dépasse les 3 phases et qu'on oublie
		for (int i = 3; i < listPhase.Length; i++) {
			probabiliteBlock [i] = new int[5] {0, 0, 0, 0, 0};
		}

		cameraStartPosition = kamera.transform.position.x - kamera.orthographicSize * kamera.aspect - 1;
		cameraEndPosition = kamera.transform.position.x + kamera.orthographicSize * kamera.aspect + 1;

        // Initialisation avec le Block Start
        blockList = new List<GameObject> {blockStart};
		// ICI : Instantiate si jamais c'est un prefab
		blockList[0].transform.position = player.transform.position + Vector3.down; // Juste sous le joueur
		sizeLastBlock = blockList[0].GetComponent<TiledMap> ().NumTilesWide;
		sizeFirstBlock = sizeLastBlock;

        /*GameObject obj;
		for(int i = 0; i < backgrounds.Length; i++) {
			obj = PoolingManager.current.Spawn( backgrounds[i].GetComponent<PoolingScript>().poolName );
			obj.transform.parent = backgrounds[i].transform;
			obj.SetActive(true);
		}*/

        // On commence le niveau dans une phase "block" et non une phase "ennemi", le joueur ne peut donc pas tirer
        player.SetFireAbility( false );
	}

	void Update () {
        // Empêcher que des choses se passent durant la pause
		if (Time.timeScale == 0 || player.IsDead ())
            return;

		PlayBackgroundMusic ();
		// Distance parcourue depuis le dernier update
		localDistance = player.GetMoveSpeed() * Time.deltaTime;

		/* Augmente la vitesse à chaque passage de x units (dans listStep)
		distanceTraveled += player.moveSpeed;
		if (currentStep < listStep.Length) {
			if (distanceTraveled > listStep [currentStep]) {
				player.moveSpeed += augmentSpeed;
				currentStep++;
			}
		}*/

		// Augmentation de la vitesse progressive
		//player.moveSpeed = player.initialMoveSpeed + Mathf.Log (distanceTraveled) / Mathf.Log(2);
		//player.moveSpeed = player.initialMoveSpeed + player.initialMoveSpeed * Time.time / 60f;	

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

                // Le joueur peut tirer
                player.SetFireAbility( true );
            }

			// Faire clignoter le texte avant que l'ennemi ne soit là
			//meterText.color = Color.Lerp(defaultTextColor, Color.clear, Mathf.Abs(Mathf.Sin(Time.frameCount / 15f)));
			
			if(enemyEnCours != null) {
                // Si on est en phase "ennemie" et qu'on a dépassé la distance allouée pour le tuer, on meurt
                SetEnemyDistanceToKill( GetEnemyDistanceToKill() - localDistance );

				if( GetEnemyDistanceToKill() <= 0 ) {
					LevelManager.Kill( player, true );

                    // Arrêter l'éditeur Unity pour empêcher la mort infinie
                    // TODO : A remplacer par autre chose
                    SetEnemyDistanceToKill( enemyEnCours.GetDistanceToKill() );
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
			//test random à mettre sous fonction
			string randomBlock = PoolingManager.current.RandomNameOfPool ("Block", RandomDifficulty(currentPhase)); // Random Block de difficulté adaptée à la currentPhase
			// fin
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
		if (obj == null) return;
		
		// On cherche le dernier élément (vu qu'on place tout par rapport à lui)
		GameObject lastBlock = blockList[blockList.Count-1];
		
		obj.transform.position = lastBlock.transform.position + Vector3.right * lastBlock.GetComponent<TiledMap > ().NumTilesWide;
		obj.transform.rotation = lastBlock.transform.rotation;
        _StaticFunction.SetActiveRecursively(obj, true); // Normalement SetActive(true);
		
		sizeLastBlock = obj.GetComponent<TiledMap>().NumTilesWide;
		
		blockList.Add (obj); // On ajoute à la liste le bloc
	}

	private void MoveWorld() {
		foreach(GameObject block in blockList) {
			block.transform.Translate (Vector3.left * Time.smoothDeltaTime * player.GetMoveSpeed());
		}
	}

	private IEnumerator SpawnEnemy(Enemy enemy) {
        yield return new WaitForSeconds( enemySpawnDelay );
		Vector2 enemyTransform = new Vector2 (player.transform.position.x + 10, player.transform.position.y + 2);
		enemyEnCours = Instantiate (enemy, enemyTransform, player.transform.rotation) as Enemy;
		enemyDistanceToKill = enemyEnCours.GetDistanceToKill();
	}

	/*public void KillPlayer(){
		StartCoroutine ("RespawnPlayerCo");
	}

	private IEnumerator RespawnPlayerCo() {
        //Instantiate (deathEffect, player.transform.position, player.transform.rotation);
        //player.GetComponent<Renderer> ().enabled = false;
		bool fireAbility = player.GetFireAbility();
        player.SetFireAbility( false );
        yield return new WaitForSeconds (respawnDelay);
		player.GetComponent<Rigidbody2D> ().velocity = Vector2.zero;
        player.transform.position = currentCheckPoint.transform.position;
		player.FullHealth ();
		player.Resurrect();
		player.SetFireAbility( fireAbility );
        //player.GetComponent<Renderer> ().enabled = true;
    }*/

	public static PlayerController GetPlayer() {
		return player;
	}

	/*public Transform[] GetBackgrounds() {
		return backgrounds;
	}*/

	public bool IsBlockPhase() {
		return blockPhase;
	}

	public static void Kill( Character character, bool ignoreLastWish = false ) {
		if( character == player ) {
			Pickup[] pickups = character.GetComponentsInChildren<Pickup>();
			LastWishPickup lastWish = player.GetLastWish();
			
			foreach( Pickup pickup in pickups ) {
				if( pickup != lastWish || lastWish.IsLaunched() ) {
					pickup.Disable();
				}
			}

			if( ignoreLastWish && lastWish != null ) {
				lastWish.Cancel();
			}
		}

		//character.SetHealthPoint( 0 );
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
