using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// Le mur de feu réduit de plus en plus la zone de l'écran visible par le joueur
// Lorsque le joueur récupère des poches d'eau, le mur recule un peu

[RequireComponent(typeof(AudioSource))]
public class Firewall : MonoBehaviour {

	private ParticleSystem fireParticle;
	private ParticleSystem.ShapeModule fireShape;

	private Transform myTransform;

	// Données sur l'apparition de la boule d'eau
	[SerializeField] private Waterball ballOfWater;
	[SerializeField] private float delayBetweenBalls;
	private float timeToCreate;
	private float waterMinX;
	private float waterMaxX;
	private float waterMinY;
	private float waterMaxY;

	// Données concernant l'avancée du mur
	private float startingPosition;
	private float moveSpeed;		// Vitesse de déplacement du mur par mètre d'avancée du joueur

	[Header("Hard Parameters")]
	[SerializeField] private float hardMoveSpeed;
	[SerializeField] private float hardDelayBetweenBalls;

	[Header("Hell Parameters")]
	[SerializeField] private float hellMoveSpeed;
	[SerializeField] private float hellDelayBetweenBalls;

	void Awake () {
		fireParticle = GetComponentInChildren<ParticleSystem> ();
		myTransform = transform;

		timeToCreate = TimeManager.time + Random.Range (1f, 3f);
	}

	void OnEnable () {
		
	}

	void Start () {
		// Chargement différent selon la difficulté
		switch (LevelManager.levelManager.GetCurrentDifficulty ()) {
		// Normal
		case 0:
			/*gameObject.SetActive (false);
			break;*/ // TODO remove
		// Hard
		case 1:
			gameObject.SetActive (true);
			// Paramètres
			moveSpeed = hardMoveSpeed;
			delayBetweenBalls = hardDelayBetweenBalls;

			LoadFireWall ();

			break;
		// Hell
		case 2:
			gameObject.SetActive (true);
			// Paramètres
			moveSpeed = hellMoveSpeed;
			delayBetweenBalls = hellDelayBetweenBalls;

			LoadFireWall ();

			break;
		}

		float ratioBorderScreen = 0.2f;
		float absolutHorizontalBorder = 2 * Camera.main.orthographicSize * Camera.main.aspect * ratioBorderScreen;
		float absolutVerticalBorder = 2 * Camera.main.orthographicSize * ratioBorderScreen;
		waterMinX = CameraManager.cameraLeftPosition + absolutHorizontalBorder;
		waterMaxX = CameraManager.cameraRightPosition - absolutHorizontalBorder;
		waterMinY = 2f; // On force pour laisser de la place au joueur de sauter sans le déranger avec des bulles
		waterMaxY = CameraManager.cameraUpPosition - absolutVerticalBorder;
	}

	private void LoadFireWall () {
		// Mise en place du mur de feu selon les proportions de l'écran
		// Taille
		myTransform.localScale = 2f * new Vector2 (Camera.main.orthographicSize * Camera.main.aspect, Camera.main.orthographicSize);
		// Position : centrage + décalage au bord de l'écran gauche (début)
		myTransform.position = new Vector2 (CameraManager.cameraManager.xOffset, CameraManager.cameraManager.yOffset) + Vector2.left * Camera.main.orthographicSize * Camera.main.aspect * 2;
		startingPosition = myTransform.position.x;

		// Ajustement de la hauteur du mur de feu en fonction de l'écran (légèrement plus grand pour couvrir le bas) + un peu de largeur
		ParticleSystem.ShapeModule shape;
		shape = fireParticle.shape;
		shape.box = Vector3.up * myTransform.localScale.y * 1.1f + Vector3.right;
		// Position des particules
		//fireParticle.transform.localPosition = 2 * Vector2.right;
	}

	void Update () {
		if (TimeManager.paused || LevelManager.player.IsDead () || LevelManager.IsEndingScene())
			return;
		
		myTransform.Translate (Vector2.right * moveSpeed * LevelManager.levelManager.GetLocalDistance ());

		// Apparition de la boule d'eau
		if (TimeManager.time > timeToCreate && !ballOfWater.isActiveAndEnabled) {
			CreateWaterBall ();
			timeToCreate = TimeManager.time + delayBetweenBalls;
		}
	}

	void OnTriggerEnter2D (Collider2D other){
		if (other.name == "Heros") {
			LevelManager.Kill( LevelManager.player );
		}
	}

	private void CreateWaterBall () {
		float posX = Random.Range (waterMinX, waterMaxX);
		float posY = Random.Range (waterMinY, waterMaxY);

		ballOfWater.transform.position = new Vector2 (posX, posY);

		ballOfWater.gameObject.SetActive (true);
	}

	public void SlowFireWall (float distance) {
		StartCoroutine (SlowFireWallCoroutine (distance));
	}

	private IEnumerator SlowFireWallCoroutine (float distance) {
		float delay = 0.25f;
		float currentTime = 0;

		while (currentTime < delay) {
			myTransform.Translate (distance * Vector2.left * TimeManager.deltaTime / delay);

			// On ne peut reculer plus que le début de l'écran
			if (myTransform.position.x < startingPosition)
				myTransform.position = new Vector2 (startingPosition, myTransform.position.y);

			currentTime += TimeManager.deltaTime;
			yield return null;
		}
	}
}
