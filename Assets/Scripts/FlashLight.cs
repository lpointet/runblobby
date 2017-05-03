using System.Collections;
using UnityEngine;

public class FlashLight : MonoBehaviour {

	private Transform myTransform;
	private Material myMaterial; // Permet d'ajuster la luminosité en réglant la valeur du cutout par l'alpha

	private float screenWidth;
	private float screenHeight;
	private float xOffset; // Décalage du joueur en x
	private float yOffset; // Décalage du joueur en y

	private float flashLightOffset = -5.75f; // Décalage selon x de la lumière (par rapport à la construction de l'image
	private Vector2 currentFlashLightOffset;

	/* CLIGNOTTEMENT DE LA LUMIERE */
	[Header("Winking")]
	[SerializeField] private float minAlphaCutoff; // Le cutoff ne sera jamais inférieur
	[SerializeField] private float maxAlphaCutoff; // Le cutoff ne sera jamais supérieur
	[Range(0.0f, 0.5f)] [SerializeField] private float winkRange; // Range de clignottement
	[SerializeField] private int winkSpeed; // Vitesse de clignottement
	private float winkCutoff; // Variable "temporaire" pour conserver le cutoff "courant"
	private float currentCutoff; // Valeur courante du cutoff, qui diminue au cours du temps - Ne peut jamais être inférieure à startCutoff
	[SerializeField] private float cutoffDecay; // Diminution par seconde du cutoff
	private float timeToDark; // Temps avant le prochain assombrissement

	/* LUCIOLE DU HEROES */
	[Header("Hero Firefly")]
	[SerializeField] private SpriteRenderer firefly;
	private Transform fireflyTransform;
	private Vector2 currentFireflyPosition;

	/* LUCIOLE DANS LA CAVE */
	[Header("Cave Firefly")]
	[SerializeField] private Firefly flyingFirefly;
	private float brightTimeToLive;

	private bool playerWasGrounded;
	private bool blackScreen = false; // Permet de savoir lorsque l'on veut un écran noir 

	void Awake () {
		fireflyTransform = firefly.transform;
		myMaterial = GetComponent<Renderer> ().sharedMaterial;
		currentCutoff = maxAlphaCutoff - winkRange;
		winkCutoff = currentCutoff;
	}

	void Start () {
		// Chargement différent selon la difficulté et le mode
		if (LevelManager.levelManager.IsStory ()) {
			switch (LevelManager.levelManager.GetCurrentDifficulty ()) {
			// Normal
			case 0:
				gameObject.SetActive (false);
				return;
			// Hard
			case 1:
				
				break;
			// Hell
			case 2:
				
				break;
			}
		}

		screenWidth = Camera.main.orthographicSize * Camera.main.aspect * 2;
		screenHeight = Camera.main.orthographicSize * 2;

		// Agrandissement de la texture à la taille de l'écran
		transform.localScale = new Vector3 (screenWidth, screenHeight, 1);
		// Rétrécissement inverse de la taille de la luciole
		fireflyTransform.localScale = new Vector3 (1 / screenWidth, 1 / screenHeight, 1);

		// Décalage pour prendre en compte la position du joueur (z = -1 pour qu'il soit devant tout, sauf l'UI)
		transform.position = new Vector3 (CameraManager.cameraManager.xOffset, CameraManager.cameraManager.yOffset, -1);

		// Décalage du material pour "centrer" la torche sur le joueur
		// Décalage de la luciole pour la centrer sur le joueur
		MovingFirefly ();

		brightTimeToLive = 1; // La première luciole dure 10 sec

		myMaterial.SetFloat ("_Cutoff", maxAlphaCutoff - winkRange);
	}

	void Update () {
		if (LevelManager.player.IsDead () || TimeManager.paused)
			return;
		
		// Décalage du material pour "centrer" la torche sur le joueur
		// Décalage de la luciole pour la centrer sur le joueur
		MovingFirefly ();

		// Clignottement de l'écran d'ajustement pour imiter une luciole
		Winking ();

		// Assombrissement progressif - Si la durée de vie de la luciole s'efface
		if (brightTimeToLive < 0 && TimeManager.time > timeToDark)
			DescentToDarkness (cutoffDecay);
		brightTimeToLive -= TimeManager.deltaTime;

		// Contrôle pour savoir si le joueur a atteri
		if (!playerWasGrounded && LevelManager.player.IsGrounded () && !LevelManager.player.IsFlying ()) {
			Darkening (0.05f);
			StartCoroutine (BlackScreen (0.25f));
		}
		playerWasGrounded = LevelManager.player.IsGrounded ();
	}

	private void MovingFirefly () {
		xOffset = (CameraManager.cameraManager.xOffset - LevelManager.player.transform.position.x) / screenWidth;
		xOffset += 0.025f * Mathf.Sin (1 + 1.5f * TimeManager.time);
		yOffset = (CameraManager.cameraManager.yOffset - LevelManager.player.transform.position.y) / screenHeight;
		yOffset += 0.025f * Mathf.Sin (2 * TimeManager.time);

		currentFireflyPosition = new Vector2 (xOffset, yOffset);
		xOffset += flashLightOffset / screenWidth;
		currentFlashLightOffset = new Vector2 (xOffset, yOffset);

		myMaterial.mainTextureOffset = currentFlashLightOffset;
		fireflyTransform.localPosition = -currentFireflyPosition;

		// Placement de la luciole devant ou derrière le joueur
		if (Mathf.Cos (1 + 1.5f * TimeManager.time) < 0)
			firefly.sortingLayerName = "Player";
		else
			firefly.sortingLayerName = "Foreground";
	}
		
	private void Winking () {
		// On stoppe le clignottement lorsque l'on souhaite avoir un écran noir
		if (blackScreen)
			return;
		
		// Niveau du cutoff
		winkCutoff = currentCutoff + winkRange * Mathf.Sin (winkSpeed * TimeManager.time);
		myMaterial.SetFloat ("_Cutoff", winkCutoff);
	}

	private IEnumerator BlackScreen (float time) {
		float currentTime = 0;
		float startCutoff = myMaterial.GetFloat ("_Cutoff");
		float endCutoff = currentCutoff + winkRange * Mathf.Sin (winkSpeed * (time + TimeManager.time));

		blackScreen = true;

		while (currentTime < time) {
			// Premier quart, on éteint la lumière
			if (currentTime < time / 4.0f) {
				winkCutoff = Mathf.Lerp (startCutoff, 0.5f, currentTime / 4.0f / time);
			} else {
				winkCutoff = Mathf.Lerp (0.5f, endCutoff, 3.0f * currentTime / 4.0f / time);
			}
			myMaterial.SetFloat ("_Cutoff", winkCutoff);

			currentTime += TimeManager.deltaTime;
			yield return null;
		}

		blackScreen = false;
	}

	private void DescentToDarkness (float cutoffValue) {
		timeToDark = TimeManager.time + 1.0f;

		Darkening (cutoffValue);
	}

	public void Darkening (float cutoffValue) {
		// Le cutoff ne peut jamais devenir totalement noir - Minimum = minAlphaCutoff + winkRange
		// Ajustement en fonction de la valeur courante du cutoff - Plus c'est "haut", moins il faut varier
		cutoffValue = cutoffValue * (1.0f - myMaterial.GetFloat ("_Cutoff"));
		currentCutoff = Mathf.Max (currentCutoff - cutoffValue, minAlphaCutoff + winkRange);
	}

	public void GetNewFirefly () {
		brightTimeToLive = flyingFirefly.brightLifetime * cutoffDecay;

		Lightning (10.0f);
	}

	public void Lightning (float cutoffValue) {
		// Le cutoff ne peut jamais être totalement blanc - Maxime = maxAlphaCutoff - winkRange
		// Ajustement en fonction de la valeur courante du cutoff - Plus c'est "haut", moins il faut varier
		cutoffValue = cutoffValue * (1.0f - myMaterial.GetFloat ("_Cutoff"));
		currentCutoff = Mathf.Min (currentCutoff + cutoffValue, maxAlphaCutoff - winkRange);
	}
}
