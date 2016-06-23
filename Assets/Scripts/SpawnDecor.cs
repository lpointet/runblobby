using UnityEngine;
using System.Collections;

public class SpawnDecor : MonoBehaviour {

	public DecorElement[] listeDecor;

	private Transform myTransform;
	private Transform initialParent;

	private int totalProba = 0;

	private static Transform positionCurrentDecor = null;
	private static int sizeCurrentDecor = 0;

	public int probaApparition = 75;
	private bool appeared = false;

	public int sizeAvailable = 10;

	// Permet de reset les variables static
	public static void Reset() {
		positionCurrentDecor = LevelManager.player.transform;
		sizeCurrentDecor = 0;
	}

	void Awake () {
		myTransform = transform;
		initialParent = transform.parent;

		if (positionCurrentDecor == null) {
			positionCurrentDecor = LevelManager.player.transform;
		}

		// Somme des probabilités
		for (int i = 0; i < listeDecor.Length; totalProba += listeDecor[i++].weight);
	}

	void OnEnable () {
		// On supprime les éventuels sprites qui ne sont pas partis
		foreach (DecorElement item in GetComponentsInChildren<DecorElement>()) {
			item.transform.parent = initialParent;
			item.gameObject.SetActive (false);
		}

		appeared = false;

		// On force le contenant à être sur le layer "Ground" pour la suppression de ce bloc spécifique dans le LevelManager
	}

	void Update () {
		// Si l'objet est déjà apparu on annule tout
		if (appeared || myTransform.position.x > CameraManager.cameraEndPosition)
			return;
		
		// Si l'objet entre dans le champ, on considère qu'il est apparu (même s'il n'y aura pas de sprite)
		if (myTransform.position.x < CameraManager.cameraEndPosition)
			appeared = true;
		
		// On teste si on doit faire apparaître un objet ou non
		if (myTransform.position.x >= positionCurrentDecor.position.x + sizeCurrentDecor && Random.Range (0, 101) <= probaApparition) {
			// On choisit un élément de décor aléatoire selon les probabilités d'apparition
			int probaRandom = Random.Range (0, totalProba);
			int k, decorChoisi;
			// On ajoute à k la valeur de la proportion si jamais k est inférieur à random, et on incrémente decorChoisi
			for(k = 0, decorChoisi = 0; k <= probaRandom; k += listeDecor[decorChoisi++].weight);

			GameObject decor = PoolingManager.current.Spawn (listeDecor[decorChoisi-1].name);

			if (decor != null) {
				int decorSize = decor.GetComponent<DecorElement> ().size;
				if (decorSize > sizeAvailable)
					return;
				
				decor.transform.position = myTransform.position;
				decor.transform.rotation = myTransform.rotation;
				decor.transform.parent = myTransform;

				decor.gameObject.SetActive (true);

				// On assigne la taille de l'élément courant, afin de ne pas en faire apparaître d'autre devant
				sizeCurrentDecor = decorSize;

				positionCurrentDecor = myTransform;
			}
		}
	}

	void OnDrawGizmos() {
		Gizmos.color = Color.yellow;
		Gizmos.DrawSphere(transform.position, 0.25f);
	}
}
