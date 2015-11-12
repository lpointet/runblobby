/*using UnityEngine;

public class Parallaxing : MonoBehaviour {

	private Transform[] backgrounds;			// Array des backgrounds et foregrounds
	private float[] parallaxScales;			// Proportion du mouvement pour bouger les éléments du background
	public float smoothing = 1f;			// La douceur du parallaxe, doit etre > 0

	private PlayerController player;

	void Awake() {
		player = LevelManager.GetPlayer ();
		backgrounds = LevelManager.levelManager.GetBackgrounds();
	}

	void Start () {
		parallaxScales = new float[backgrounds.Length]; // attribué à chaque background son parallaxScales
		for(int i = 0; i < backgrounds.Length; i++) {
			parallaxScales[i] = backgrounds[i].position.z * -1;
		}
	}

	void Update () {
		if (!player.IsDead()) {
			for (int i = 0; i < backgrounds.Length; i++) {
				// la parallaxe est l'opposé du mouvement du joueur (scale < 0)
				float parallax = (Time.deltaTime * player.GetMoveSpeed()) * parallaxScales [i];

				// créé la position future du background (sa position x + parallax)
				Vector3 backgroundTargetPos = new Vector3 (backgrounds [i].position.x + parallax, backgrounds [i].position.y, backgrounds [i].position.z);

				// fade entre la position courante et la future
				backgrounds [i].position = Vector3.Lerp (backgrounds [i].position, backgroundTargetPos, smoothing * Time.deltaTime);
			}
		}
	}
}*/
