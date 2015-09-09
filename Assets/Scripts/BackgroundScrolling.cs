using UnityEngine;
using System.Collections;

public class BackgroundScrolling : MonoBehaviour {

	public float scrollSpeed;
	private float ratioVitesse;
	private Material myMaterial;

	private PlayerController player;

	void Start() {
		myMaterial = GetComponent<Renderer> ().material;
		player = LevelManager.getPlayer ();

		// Adapater l'échelle à la taille de l'écran pour qu'on voit tout à chaque fois
		//transform.localScale = new Vector2 (Camera.main.orthographicSize * Camera.main.aspect * 2, Camera.main.orthographicSize * 2);

		// La vitesse dépend du joueur et de ce ratio = la taille que doit parcourir l'objet et sa distance au joueur (z)
		ratioVitesse = transform.localScale.x * transform.position.z;

		//transform.localScale *= Screen.height / 1440f;
	}

	void Update () {
		if (!player.IsDead ()) {
			scrollSpeed = player.GetMoveSpeed () / ratioVitesse;

			float x = Mathf.Repeat (Time.time * scrollSpeed, 1);
			Vector2 offset = new Vector2 (x, myMaterial.mainTextureOffset.y);
			myMaterial.mainTextureOffset = offset;
		}
	}

}
