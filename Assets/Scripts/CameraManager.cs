using UnityEngine;
using System.Collections;
using System;

[RequireComponent (typeof (Camera))]
public class CameraManager : MonoBehaviour {

	public PlayerController player;
	public bool isFollowing;

	public int unitsInWidth;
	public int widthMin;

	public float xOffsetPourcentage;
	private float xOffset;
	public float yOffsetPourcentage;
	private float yOffset;

	private Transform backKiller;
	private Transform fallingKiller;
	
	private Collider2D backKillerCollider;
	private Collider2D fallingKillerCollider;

	void Awake() {
		player = FindObjectOfType<PlayerController> ();

		backKiller = transform.Find ("BackKiller");
		fallingKiller = transform.Find ("FallingKiller");
		
		backKillerCollider = backKiller.GetComponent<Collider2D>();
		fallingKillerCollider = fallingKiller.GetComponent<Collider2D>();
	}

	void Start () {

		Camera.main.orthographicSize = Screen.height / 32.0f / 2.0f;
		FixeResolution ();
		// on place on offset en pourcentage par rapport à ce qui est calculé
		xOffset = (Camera.main.orthographicSize * Camera.main.aspect) * xOffsetPourcentage;
		yOffset = Camera.main.orthographicSize * yOffsetPourcentage;
		transform.position = new Vector3(player.transform.position.x + xOffset, player.transform.position.y + yOffset, transform.position.z);
	}

	void Update () {
		#if UNITY_EDITOR
		//FixeResolution();
		#endif

		if (isFollowing) {
			transform.position = new Vector3 (player.transform.position.x + xOffset, player.transform.position.y + yOffset, transform.position.z);
		}
	}

	private void FixeResolution() {
		// calcul de la taille ortho (vertical en units / 2) à partir de la longueur demandée
		if (unitsInWidth < widthMin)
			unitsInWidth = widthMin;
		//Camera.main.orthographicSize = unitsInWidth / (2f * Camera.main.aspect);

		// placement des zones qui dépendent de l'écran (donc de la caméra)
		backKiller.position = transform.position + Vector3.left * (Camera.main.orthographicSize * Camera.main.aspect + backKillerCollider.bounds.size.x / 3f);
		fallingKiller.position = transform.position + Vector3.down * (Camera.main.orthographicSize + fallingKillerCollider.bounds.size.y / 3f);
	}
}
