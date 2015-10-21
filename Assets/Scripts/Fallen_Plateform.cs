using UnityEngine;

public class Fallen_Plateform : MonoBehaviour {

	private PlayerController player;

	private Transform pointFallen;
	public Transform fallenObject; // Objet qui doit tomber, caché au début mais existant
	public float delay; // Délai de chute de la plateforme
	public float chuteMultiplier = 2f;

	private bool blockAppeared = false;

	void Start () {
		player = LevelManager.getPlayer ();
		pointFallen = transform;
	}

	void Update () {
		if (blockAppeared) {
			fallenObject.Translate(Vector3.down * Time.deltaTime * player.GetMoveSpeed() * chuteMultiplier);

			if (fallenObject.position.y <= pointFallen.position.y) {
				blockAppeared = false; // on arrete le mouvement de chute
				fallenObject.position = new Vector2(pointFallen.position.x, pointFallen.position.y);
			}
		}
	}

	void OnBecameVisible() {
		// On le place au-dessus du point d'impact et hors de l'écran (en prenant en compte l'yOffset)
		float heightOverScreen = 2 * Camera.main.orthographicSize - Camera.main.GetComponent<CameraManager>().yOffset + Mathf.Abs(pointFallen.position.y);
		fallenObject.position = new Vector2(pointFallen.position.x, heightOverScreen);

		// On attend la distance que l'on souhaite adaptée à la vitesse du joueur
		Invoke ("TomberObjet", delay * (player.GetInitialMoveSpeed () / player.GetMoveSpeed ()));
	}

	private void TomberObjet() {
		blockAppeared = true;
	}
}
