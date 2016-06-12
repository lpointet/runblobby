using UnityEngine;

[RequireComponent (typeof (Camera))]
public class CameraManager : MonoBehaviour {

    public static CameraManager cameraManager;

	public bool isFollowing;

	public int unitsInWidth;
	public int widthMin;
	public float maxJumpHeight = 6.5f; // 6.5 est la valeur qui permet de sauter avec la plateforme le plus haut possible

	public float xOffsetPourcentage;
	private float xOffset;
	public float yOffset { get; private set; }

	public Transform backKiller { get; private set; }
	public Transform fallingKiller { get; private set; }
	private Collider2D backKillerCollider;
	private Collider2D fallingKillerCollider;

	public GameObject bgContainer;

	public float camRightEnd { get; private set; }

    void Awake() {
        if (cameraManager == null)
            cameraManager = GetComponent<CameraManager>();

		backKiller = transform.Find ("BackKiller");
		fallingKiller = transform.Find ("FallingKiller");
		
		backKillerCollider = backKiller.GetComponent<Collider2D>();
		fallingKillerCollider = fallingKiller.GetComponent<Collider2D>();
	}

	void Start () {
		FixeResolution ();

		// On place on offset en pourcentage par rapport à ce qui est calculé
		xOffset = (Camera.main.orthographicSize * Camera.main.aspect) * xOffsetPourcentage;
		//yOffset = Camera.main.orthographicSize * yOffsetPourcentage;
		// Le yOffset doit placer le héros à 3 unités (taille max des block en hauteur) au-dessus du bas de l'écran (sachant qu'il commence déjà à -1)
		yOffset = Camera.main.orthographicSize - 3 - LevelManager.levelManager.GetHeightStartBlock();
		transform.position = new Vector3(0 + xOffset, 0 + yOffset, transform.position.z);
		
		// On ajuste les fonds d'écran de sorte qu'ils rentrent dans la caméra en HAUTEUR et en LARGEUR
		SpriteRenderer firstBG = bgContainer.transform.GetChild (0).GetComponent<SpriteRenderer>();
		float yScale = Camera.main.orthographicSize * 2 / firstBG.bounds.size.y;
		float xScale = Camera.main.orthographicSize * Camera.main.aspect * 2 / firstBG.bounds.size.x;
		bgContainer.transform.localScale = new Vector3 (xScale, yScale, bgContainer.transform.localScale.z);

        camRightEnd = xOffset + Camera.main.orthographicSize * Camera.main.aspect;
    }

	void Update () {
		#if UNITY_EDITOR
		FixeResolution();
		#endif

		if (isFollowing) {
			transform.position = new Vector3 (LevelManager.GetPlayer().transform.position.x + xOffset, LevelManager.GetPlayer().transform.position.y + yOffset, transform.position.z);
		}
	}

	private void FixeResolution() {
		//float sizeCameraPixelPerfect;
		float sizeCameraMinWidth;

		// calcul de la taille ortho (vertical en units / 2) à partir de la longueur demandée
		if (unitsInWidth < widthMin)
			unitsInWidth = widthMin;
		sizeCameraMinWidth = unitsInWidth / (2f * Camera.main.aspect);
		//sizeCameraPixelPerfect = Screen.height / 64.0f / 2.0f; // 64 et non 32 pour éviter que ce soit trop petit sur un smartphone

		// On ne doit pas être plus bas qu'une certaine orthographicSize, sinon la hauteur ne sera plus suffisante pour les sauts les plus hauts
		// 6.5 est la valeur qui permet de sauter avec la plateforme le plus haut possible
		if (sizeCameraMinWidth < maxJumpHeight)
			sizeCameraMinWidth = maxJumpHeight;

		// On ajuste l'ortho selon la logique : il vaut mieux déformer les pixels mais avoir une bonne vision devant soi
		//Camera.main.orthographicSize = sizeCameraMinWidth > sizeCameraPixelPerfect ? sizeCameraMinWidth : sizeCameraPixelPerfect;
		Camera.main.orthographicSize = sizeCameraMinWidth;

		// placement des zones qui dépendent de l'écran (donc de la caméra)
		backKiller.position = transform.position + Vector3.left * (Camera.main.orthographicSize * Camera.main.aspect + backKillerCollider.bounds.size.x / 3f);
		fallingKiller.position = transform.position + Vector3.down * (Camera.main.orthographicSize + fallingKillerCollider.bounds.size.y / 2f);
	}
}
