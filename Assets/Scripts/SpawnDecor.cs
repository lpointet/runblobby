using UnityEngine;
using System.Collections;

public class SpawnDecor : MonoBehaviour {

	public GameObject[] listDecor;
	private Transform myTransform;

	private static Transform positionCurrentDecor;
	private static int sizeCurrentDecor = 0;

	public int probaApparition = 75;
	public int caseDispo = 1; // TODO ne pas faire apparaître des éléments trop grands trop près du bord

	void Awake () {
		myTransform = transform;

		if (positionCurrentDecor == null) {
			positionCurrentDecor = new GameObject ().transform;
			positionCurrentDecor.position = Vector2.zero;
		}
	}

	void OnEnable () {
		// On supprime les éventuels sprites qui ne sont pas partis
		foreach (SpriteRenderer item in GetComponentsInChildren<SpriteRenderer>()) {
			item.transform.parent = null;
			item.gameObject.SetActive (false);
		}
			
		// On teste si on doit faire apparaître un objet ou non
		if (myTransform.position.x >= positionCurrentDecor.position.x + sizeCurrentDecor && Random.Range (0, 101) <= probaApparition) {
			int decorRandom = Random.Range (0, listDecor.Length); // On choisit un élément de décor aléatoire

			GameObject decor = PoolingManager.current.Spawn (listDecor [decorRandom].name);

			if (decor != null) {
				decor.transform.position = myTransform.position;
				decor.transform.rotation = myTransform.rotation;
				decor.transform.parent = myTransform;

				decor.gameObject.SetActive (true);

				// On assigne la taille de l'élément courant, afin de ne pas en faire apparaître d'autre devant
				sizeCurrentDecor = Mathf.CeilToInt (decor.GetComponent<SpriteRenderer> ().bounds.size.x);
				positionCurrentDecor = myTransform;
			}
		}
	}

	void OnDrawGizmos() {
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(transform.position, 0.25f);
	}
}
