using UnityEngine;

public class BackgroundScrolling : MonoBehaviour {

	public float scrollSpeed;

	public bool isVariable;
	public float baseCutoff = 0f;
	public float rangeSinCutoff = 0f;
	public float baseYOffset = 1f;
	public float rangeSinYOffset = 0f;
	public float speedYOffset = 1f;

	private float xOffset;
	private float ratioVitesse;
	private Material myMaterial;

	void Start() {
		myMaterial = GetComponent<Renderer> ().material;

		// Adapater l'échelle à la taille de l'écran pour qu'on voit tout à chaque fois
		//transform.localScale = new Vector2 (Camera.main.orthographicSize * Camera.main.aspect * 2, Camera.main.orthographicSize * 2);

		// La vitesse dépend du joueur et de ce ratio = la taille que doit parcourir l'objet et sa distance au joueur (z)
		ratioVitesse = transform.localScale.x * transform.position.z * scrollSpeed;
		xOffset = 0;
	}

	void Update () {
		if (!TimeManager.paused && !LevelManager.GetPlayer ().IsDead ()) {
			// Décallage permanent dans le sens inverse du joueur
			xOffset = Mathf.Repeat (xOffset + LevelManager.levelManager.GetLocalDistance() / ratioVitesse, 1);
			//float x = Mathf.Repeat (Time.time * scrollSpeed - decalage, 1);
			float y = myMaterial.mainTextureOffset.y;

			// Si on a décidé que c'était à taille variable, on modifie ici
			if (isVariable) {
				float cutoff = baseCutoff + rangeSinCutoff * Mathf.Sin (Time.time);
				y = baseYOffset + rangeSinYOffset * Mathf.Sin (speedYOffset * Time.time);

				myMaterial.SetFloat ("_Cutoff", cutoff);
			}

			Vector2 offset = new Vector2 (xOffset, y);
			myMaterial.mainTextureOffset = offset;
		}
	}

}
