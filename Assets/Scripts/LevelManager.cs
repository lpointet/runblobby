﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour {

	public static LevelManager levelManager;
	private Camera kamera;
	public static PlayerController player;
	private Transform background;

	public Text meterText;
	private Color defaultTextColor; // Couleur par défaut du meterText
	private Color warningTextColor; // Couleur en alternance lors d'un ennemi
	private Color warningTextColorBis;
	private float scaleFonctionDistance; // Echelle du texte pendant l'ennemi

	// Mort, Respawn
	public GameObject deathEffect;
	public GameObject currentCheckPoint;
	public float respawnDelay;

	// Création du monde et déplacement
	private float cameraStartPosition;
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

	// Partie Ennemi intermédiaire
	public int[] listPhase;		// Valeur relative à parcourir avant de rencontrer un ennemi
	private int currentPhase;	// Phase en cours
	private bool blockPhase;	// Phase avec des blocks ou avec un ennemi
	private bool premierBlock = false;	// Instantier le premier bloc ennemi
	public Enemy[] enemyMiddle;	// Liste des ennemis
	private Enemy enemyEnCours;
	private float enemyDistanceToKill;
	public float spawnEnemyDelay;	// Délai avant apparition de l'ennemi suite à la création du premier bloc
	public int[][] probabiliteBlock; // Probabilités d'apparition de chaque block par phase
	// Fin partie ennemi intermédiaire

	void Awake() {
		if (levelManager == null)
			levelManager = GameObject.FindGameObjectWithTag ("GameMaster").GetComponent<LevelManager>();

		player = FindObjectOfType<PlayerController> ();
		kamera = Camera.main;
		
		GameObject obj = GameObject.FindGameObjectWithTag("BackgroundContainer");
		if(null == obj) {
			Debug.LogError("Conteneur de fond introuvable, ajoute le tag 'BackgroundContainer' !");
		}
		background = obj.transform;
	}

	void Start () {
		distanceTraveled = 0;
		currentPhase = 0;
		blockPhase = true;
		enemyDistanceToKill = 0;

		defaultTextColor = meterText.color;
		warningTextColor = new Color (1, 0.588f, 0.588f);
		warningTextColorBis = warningTextColor / 2;
		warningTextColorBis.r = 1f;

		// Autant de probabilité que de phases (voir listPhase)
		probabiliteBlock = new int[listPhase.Length][];
		probabiliteBlock[0] = new int[5] {70, 30, 0, 0, 0};
		probabiliteBlock[1] = new int[5] {0, 40, 50, 10, 0};
		probabiliteBlock[2] = new int[5] {0, 0, 20, 60, 20};
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
		sizeLastBlock = blockList[0].GetComponent<BlockManager> ().widthSize;
		sizeFirstBlock = sizeLastBlock;
		
		GameObject obj = PoolingManager.current.Spawn("Background");
		obj.transform.parent = background;
		obj.SetActive(true);
	}

	void Update () {
		// Distance parcourue depuis le dernier update
		localDistance = player.moveSpeed * Time.deltaTime;

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
		if (currentPhase < listPhase.Length && distanceTraveled > listPhase[currentPhase]) {
			blockPhase = false;

			// On créé le premier bloc qui n'est pas un bloc du milieu
			if(!premierBlock) {
				PositionBlock(Instantiate(blockEnemy[0]));
				premierBlock = true;

				StartCoroutine(SpawnEnemyCo(enemyMiddle[currentPhase]));
			}

			// Faire clignoter le texte
			meterText.color = Color.Lerp(defaultTextColor, Color.clear, Mathf.Abs(Mathf.Sin(Time.frameCount/15f)));
			
			if(enemyEnCours != null) {
				// Si on est en phase "ennemie" et qu'on a dépassé la distance allouée pour le tuer, on meurt
				enemyDistanceToKill -= localDistance;

				if( enemyDistanceToKill <= 0 ) {
					LevelManager.Kill( player );
					RespawnPlayer();

					// Arrêter l'éditeur Unity pour empêcher la mort infinie
					// TODO : A remplacer par autre chose
					UnityEditor.EditorApplication.isPlaying = false;
				}

				meterText.text = Mathf.RoundToInt (enemyDistanceToKill) + "m"; // Mise à jour de la distance restante pour tuer le boss
				meterText.color = Color.Lerp (warningTextColor, warningTextColorBis, Mathf.Sin (2f * enemyDistanceToKill)); // Variation entre deux couleurs

				// Fonction type f(x) = ax² + b, avec a = (scaleMaxAtteint-1) / distanceMaxPossible² et b = 1
				scaleFonctionDistance = (2 / Mathf.Pow (enemyEnCours.distanceToKill, 2)) * Mathf.Pow (enemyEnCours.distanceToKill - enemyDistanceToKill, 2) + 1;
				meterText.transform.localScale = new Vector2(scaleFonctionDistance, scaleFonctionDistance);

				// On créé le dernier bloc qui n'est pas un bloc du milieu
				// Quand l'ennemi est mort
				if( enemyEnCours.isDead ) {
					enemyEnCours = null;
					PositionBlock(Instantiate(blockEnemy[1]));

					currentPhase++;
					premierBlock = false;
				}
			}
		}
		// Si on n'est pas dans une phase "ennemie", on est dans une phase "block"
		else {
			blockPhase = true;
			// On actualise la distance parcourue si le joueur n'est pas mort
			if (!player.isDead)
				distanceTraveled += localDistance;

			meterText.text = Mathf.RoundToInt (distanceTraveled) + "m"; // Mise à jour de la distance parcourue affichée
			meterText.color = defaultTextColor;
			meterText.transform.localScale = Vector2.one;
		}



		// Suppression du premier bloc dès qu'il disparait de la caméra
		if (blockList [0].transform.position.x + sizeFirstBlock < cameraStartPosition) {
			blockList [0].SetActive (false);
			blockList.RemoveAt (0);
			
			sizeFirstBlock = blockList [0].GetComponent<BlockManager> ().widthSize;
		}
		
		// Création du prochain bloc si le dernier bloc en cours approche de la fin de la caméra
		if (blockList [blockList.Count - 1].transform.position.x + sizeLastBlock < cameraEndPosition) {
			PositionBlock (GetNewBlock (blockPhase));
		}

		// Si le joueur n'est pas mort, on bouge le monde
		if (!player.isDead) {
			MoveWorld ();
		}
	}

	private GameObject GetNewBlock(bool _blockPhase) {
		if (_blockPhase) {
			//test random à mettre sous fonction
			string randomBlock = PoolingManager.current.RandomNameOfPool ("Block", RandomDifficulty(currentPhase)); // Random Block de difficulté 1
			// fin
			return PoolingManager.current.Spawn (randomBlock);
		}
		else
			return PoolingManager.current.Spawn ("BasiqueGround");
	}

	private string RandomDifficulty(int phase) {
		int[] probabilite; // liste des probabilités d'appeler le choixDifficulte
		string[] listeDifficulte = {"difficulty_1", "difficulty_2", "difficulty_3", "difficulty_4", "difficulty_5"};
		int sum = 0;

		// On retourne la difficulté la plus facile si jamais on envoie une valeur de phase incorrecte
		if (phase >= listPhase.Length)
			return "difficulty_1";

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
		
		obj.transform.position = lastBlock.transform.position + Vector3.right * lastBlock.GetComponent<BlockManager>().widthSize;
		obj.transform.rotation = lastBlock.transform.rotation;
		LevelManager.SetActiveRecursively(obj, true); // Normalement SetActive(true);
		
		sizeLastBlock = obj.GetComponent<BlockManager>().widthSize;
		
		blockList.Add (obj); // On ajoute à la liste le bloc
	}

	private void MoveWorld() {
		foreach(GameObject block in blockList) {
			block.transform.Translate (Vector3.left * Time.deltaTime * player.moveSpeed);
		}
	}

	public static PlayerController getPlayer() {
		return player;
	}

	public static void Kill(Character character) {
		character.healthPoint = 0;
		character.isDead = true;

		character.OnKill();
	}

	private IEnumerator SpawnEnemyCo(Enemy enemy) {
		yield return new WaitForSeconds (spawnEnemyDelay);
		Vector2 enemyTransform = new Vector2 (player.transform.position.x + 10, player.transform.position.y + 2);
		enemyEnCours = Instantiate (enemy, enemyTransform, player.transform.rotation) as Enemy;
		enemyDistanceToKill = enemyEnCours.distanceToKill;
	}

	public void RespawnPlayer(){
		StartCoroutine ("RespawnPlayerCo");
	}

	private IEnumerator RespawnPlayerCo() {
		Instantiate (deathEffect, player.transform.position, player.transform.rotation);
		player.GetComponent<Renderer> ().enabled = false;
		//player.enabled = false;
		player.gameObject.SetActive (false);
		yield return new WaitForSeconds (respawnDelay);
		player.gameObject.SetActive (true);
		//player.enabled = true;
		player.GetComponent<Rigidbody2D> ().velocity = Vector2.zero;
		player.transform.position = currentCheckPoint.transform.position;
		player.FullHealth ();
		player.isDead = false;
		player.GetComponent<Renderer> ().enabled = true;
	}
	
	// Fonction pour activer/désactiver tous les GameObjects dans un GameObject
	public static void SetActiveRecursively(GameObject rootObject, bool active)
	{
		rootObject.SetActive(active);
		
		foreach (Transform childTransform in rootObject.transform)
		{
			if (!childTransform.gameObject.activeInHierarchy)
				SetActiveRecursively(childTransform.gameObject, active);
		}
	}
}
