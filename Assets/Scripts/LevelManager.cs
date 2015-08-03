using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour {

	public static LevelManager levelManager;
	private Camera kamera;
	public static PlayerController player;
	private Transform background;

	public Text meterText;

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

	// Augmentation de la vitesse par palier
	private float distanceTraveled;

	public int[] listPhase;		// Valeur relative à parcourir avant de rencontrer un ennemi
	private int currentPhase;	// Phase en cours
	private bool blockPhase;
	private bool premierBlock = false;
	public Enemy[] enemyMiddle;
	public float spawnEnemyDelay;

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
		/* Augmente la vitesse à chaque passage de x units (dans listStep)
		distanceTraveled += player.stats.moveSpeed;
		if (currentStep < listStep.Length) {
			if (distanceTraveled > listStep [currentStep]) {
				player.stats.moveSpeed += augmentSpeed;
				currentStep++;
			}
		}*/
		
		// Augmentation de la vitesse progressive
		//player.stats.moveSpeed = player.stats.initialMoveSpeed + Mathf.Log (distanceTraveled) / Mathf.Log(2);
		//player.stats.moveSpeed = player.stats.initialMoveSpeed + player.stats.initialMoveSpeed * Time.time / 60f;	

		// On actualise la distance parcourue si le joueur n'est pas mort
		if (!player.stats.isDead)
			distanceTraveled += player.stats.moveSpeed * Time.deltaTime;





		// Définir dans quelle phase on se situe
		if (distanceTraveled > listPhase[currentPhase] && currentPhase < listPhase.Length) {
			blockPhase = false;

			// On créé le premier bloc qui n'est pas un bloc du milieu
			if(!premierBlock) {
				PositionBlock(Instantiate(blockEnemy[0]));
				premierBlock = true;

				StartCoroutine(SpawnEnemyCo(enemyMiddle[0]));
			}

			// On créé le dernier bloc qui n'est pas un bloc du milieu
			// Quand l'ennemi est mort
			if(enemyMiddle[0].stats.isDead) {
				PositionBlock(Instantiate(blockEnemy[1]));
				Debug.Log ("coucou");
				currentPhase++;
				premierBlock = false;
			}

			Debug.Log (enemyMiddle[0].stats.isDead);
		}
		// Si on n'est pas dans une phase "ennemie", on est dans une phase "block"
		else {
			blockPhase = true;
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
		if (!player.stats.isDead) {
			MoveWorld ();
		}

		meterText.text = Mathf.RoundToInt (distanceTraveled) + "m"; // Mise à jour de la distance parcourue affichée
	}

	private GameObject GetNewBlock(bool _blockPhase) {
		if (_blockPhase) {
			//test random à mettre sous fonction
			string randomBlock = PoolingManager.current.RandomNameOfPool ("Block");
			// fin
			return PoolingManager.current.Spawn (randomBlock);
		}
		else
			return PoolingManager.current.Spawn ("BasiqueGround");
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
			block.transform.Translate (Vector3.left * Time.deltaTime * player.stats.moveSpeed);
		}
	}

	public static PlayerController getPlayer() {
		return player;
	}

	public static void KillPlayer(PlayerController player) {
		Destroy (player.gameObject);
	}

	private IEnumerator SpawnEnemyCo(Enemy enemy) {
		yield return new WaitForSeconds (spawnEnemyDelay);
		Instantiate (enemy, player.transform.position + Vector3.right * 10, player.transform.rotation);
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
		player.stats.isDead = false;
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
