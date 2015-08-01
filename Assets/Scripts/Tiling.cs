using UnityEngine;
using System.Collections;

[RequireComponent (typeof(SpriteRenderer))]

public class Tiling : MonoBehaviour {

	public int offsetX = 2;					// offset pour éviter les erreurs

	public bool hasRightBuddy = false;		// pour éviter des calculs inutiles s'ils existent déjà
	public bool hasLeftBuddy = false;

	public bool reverseScale = false;		// utilisé si l'objet n'est pas tilable

	private float spriteWidth = 0f;			// la largeur de l'élément
	private Camera kamera;
	private Transform myTransform;

	void Awake() {
		kamera = Camera.main;
		myTransform = transform;
	}

	void Start () {
		SpriteRenderer sRenderer = GetComponent<SpriteRenderer> ();
		spriteWidth = sRenderer.sprite.bounds.size.x * Mathf.Abs(myTransform.localScale.x);
	}

	void Update () {
		if (!hasLeftBuddy || !hasRightBuddy) { // on évite les calculs si ça existe déjà
			// calculer la vision de la caméra (moitié) de ce que la caméra voit dans les coordonnées réelles
			float camHorizontalExtend = kamera.orthographicSize * Screen.width/Screen.height;
			// calculer la position x où la caméra peut voir le bord du sprite
			float edgeVisiblePositionRight = (myTransform.position.x + spriteWidth/2) - camHorizontalExtend;
			float edgeVisiblePositionLeft = (myTransform.position.x - spriteWidth/2) + camHorizontalExtend;
			// controle si on peut voir l'élément et on appelle de quoi le construire sinon
			if (kamera.transform.position.x >= edgeVisiblePositionRight - offsetX && !hasRightBuddy) {
				MakeNewBuddy(1);
				hasRightBuddy = true;
			} else if (kamera.transform.position.x <= edgeVisiblePositionLeft + offsetX && !hasLeftBuddy) {
				MakeNewBuddy(-1);
				hasLeftBuddy = true;
			}
		}
	}

	// méthode pour créer un buddy sur le coté requis
	void MakeNewBuddy(int rightOrLeft) {
		// calculer la nouvelle position du nouveau buddy
		Vector3 newPosition = new Vector3 (myTransform.position.x + spriteWidth * rightOrLeft, myTransform.position.y, myTransform.position.z);
		// instantiation du nouveau corps dans une variable
		Transform newBuddy = Instantiate (myTransform, newPosition, myTransform.rotation) as Transform;
		// si ce n'est pas tilable, on inverse l'échelle pour que ça colle bien
		if (reverseScale) {
			newBuddy.localScale = new Vector3(newBuddy.localScale.x * -1, newBuddy.localScale.y, newBuddy.localScale.z);
		}

		newBuddy.parent = myTransform.parent;
		if (rightOrLeft > 0)
			newBuddy.GetComponent<Tiling> ().hasLeftBuddy = true;
		else
			newBuddy.GetComponent<Tiling> ().hasRightBuddy = true;
	}
}
