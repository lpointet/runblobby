using System.Collections;
using UnityEngine;

public class MudScreen : MonoBehaviour {

	public static MudScreen mudScreen;

	private SpriteRenderer mySprite;
	private Transform myTransform;

	[SerializeField] private Color cleanColor;
	private Color uncleanColor;
	[SerializeField] private float cleaningSpeed; // En combien de temps on passe de clean à unclean
	private float currentMud = 0; // Entre 0 et 1

	void Awake () {
		if (mudScreen == null)
			mudScreen = GameObject.FindObjectOfType<MudScreen> ();
		
		myTransform = transform;
		mySprite = GetComponent<SpriteRenderer> ();
	}

	void Start () {
		mySprite.color = cleanColor;

		// Récupération de la couleur "sale"
		Color temp = cleanColor;
		temp.a = 0.99f; // On n'évite d'être totalement opaque dans tous les cas
		uncleanColor = temp;

		// Taille et position de l'écran
		myTransform.position = new Vector2 (CameraManager.cameraManager.xOffset, CameraManager.cameraManager.yOffset);

		myTransform.localScale = 2.0f * Camera.main.orthographicSize * new Vector2 (Camera.main.aspect, 1.0f);
	}

	void Update () {
		mySprite.color = Color.Lerp (cleanColor, uncleanColor, currentMud);

		if (currentMud > 0)
			currentMud -= TimeManager.deltaTime / cleaningSpeed;
	}

	public void AddMud (float value) {
		currentMud += value;

		if (currentMud > 1)
			currentMud = 1;
	}
}
