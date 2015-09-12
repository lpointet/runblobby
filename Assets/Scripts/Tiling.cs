using UnityEngine;

[RequireComponent (typeof(SpriteRenderer))]

public class Tiling : MonoBehaviour {

	public int offsetX = 2;					// offset pour éviter les erreurs
	public string poolName;

	private Transform rightBuddy = null;		// pour éviter des calculs inutiles s'ils existent déjà
	private Transform leftBuddy = null;

	public bool reverseScale = false;		// utilisé si l'objet n'est pas tilable

	private float spriteWidth = 0f;			// la largeur de l'élément
	private Camera kamera;
	private Transform myTransform;
	private float camHorizontalExtend;
	private float edgeVisiblePositionRight;
	private float edgeVisiblePositionLeft;

	void Awake() {
		kamera = Camera.main;
		myTransform = transform;
	}

	void Start () {
		SpriteRenderer sRenderer = GetComponent<SpriteRenderer> ();
		spriteWidth = sRenderer.sprite.bounds.size.x * Mathf.Abs(myTransform.localScale.x);
		// calculer la vision de la caméra (moitié) de ce que la caméra voit dans les coordonnées réelles
		camHorizontalExtend = kamera.orthographicSize * kamera.aspect;
	}

	void Update () {
		if (!leftBuddy || !rightBuddy) { // on évite les calculs si ça existe déjà
			if( !rightBuddy ) {
				// calculer la position x où la caméra peut voir le bord du sprite
				edgeVisiblePositionRight = (myTransform.position.x + spriteWidth/2) - camHorizontalExtend;
				// controle si on peut voir l'élément et on appelle de quoi le construire sinon
				if (kamera.transform.position.x >= edgeVisiblePositionRight - offsetX) {
					MakeNewBuddy();
				}
			}

			if( !leftBuddy ) {
				edgeVisiblePositionLeft = (myTransform.position.x + spriteWidth/2) + camHorizontalExtend;

				if (kamera.transform.position.x > edgeVisiblePositionLeft + offsetX) {
					myTransform.gameObject.SetActive(false);
					rightBuddy.GetComponent<Tiling>().leftBuddy = null;
					rightBuddy = null;
				}
			}
		}
	}

	// méthode pour créer un buddy sur le coté requis
	void MakeNewBuddy() {
		// calculer la nouvelle position du nouveau buddy
		Vector3 newPosition = new Vector3 (myTransform.position.x + spriteWidth, myTransform.position.y, myTransform.position.z);
		
		// instantiation du nouveau corps dans une variable
		GameObject newBuddy = PoolingManager.current.Spawn( poolName );

		if( null == newBuddy ) {
			return;
		}

		newBuddy.transform.position = newPosition;
		newBuddy.SetActive(true);

		// si ce n'est pas tilable, on inverse l'échelle pour que ça colle bien
		if (reverseScale && newBuddy.transform.localScale.x == myTransform.localScale.x) {
			newBuddy.transform.localScale = new Vector3(newBuddy.transform.localScale.x * -1, newBuddy.transform.localScale.y, newBuddy.transform.localScale.z);
		}

		newBuddy.transform.parent = myTransform.parent;
		newBuddy.GetComponent<Tiling> ().leftBuddy = myTransform;
		rightBuddy = newBuddy.transform;
	}
}
