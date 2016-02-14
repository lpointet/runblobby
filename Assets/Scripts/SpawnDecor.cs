using UnityEngine;
using System.Collections;

public class SpawnDecor : MonoBehaviour {

	public GameObject[] listDecor;
	public int[] listDecorProba;
	private Transform myTransform;

	private int totalProba = 0;

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

		// Somme des probabilités
		for (int i = 0; i < listDecorProba.Length; totalProba += listDecorProba [i++]);
	}

	void OnEnable () {
		// On supprime les éventuels sprites qui ne sont pas partis
		foreach (SpriteRenderer item in GetComponentsInChildren<SpriteRenderer>()) {
			item.transform.parent = null;
			item.gameObject.SetActive (false);
		}
			
		// On teste si on doit faire apparaître un objet ou non
		if (myTransform.position.x >= positionCurrentDecor.position.x + sizeCurrentDecor && Random.Range (0, 101) <= probaApparition) {
			// On choisit un élément de décor aléatoire selon les probabilités d'apparition
			int probaRandom = Random.Range (0, totalProba);
			int k, decorChoisi;
			// On ajoute à k la valeur de la proportion si jamais k est inférieur à random, et on incrémente decorChoisi
			for(k = 0, decorChoisi = 0; k <= probaRandom; k += listDecorProba[decorChoisi++]);

			GameObject decor = PoolingManager.current.Spawn (listDecor [decorChoisi-1].name);

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
