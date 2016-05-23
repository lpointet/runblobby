using UnityEngine;
using System.Collections.Generic;

public class CoinSpawn : MonoBehaviour {

	public int coinValue;

	//private GameObject instantCoin;
	private Vector2 initialPosition;

	private Transform myTransform;
	private bool isPop = false;

	void Awake () {
		initialPosition = transform.localPosition;
		myTransform = transform;
	}

	void OnEnable () {
		// On remet le point à sa place, non mais.
		transform.localPosition = initialPosition;

		// On désactive les coins non prises, non mais.
		/*if (null != instantCoin) {
			instantCoin.SetActive (false);
		}*/

		isPop = false;
	}

	void Update () {
		if (!isPop && myTransform.position.x < LevelManager.levelManager.cameraEndPosition) {
			// On choisit la bonne pièce ou la plus proche si mauvaise valeur
			int choix = 0;
			for (int i = 0; i < ListManager.current.coins.Length; i++) {
				if (ListManager.current.coins [i].pointToAdd < coinValue)
					choix++;
			}

			// On fait apparaître la pièce de la valeur demandée
			GameObject instantCoin = PoolingManager.current.Spawn (ListManager.current.coins [choix].name);

			instantCoin.SetActive (true);

			instantCoin.transform.parent = transform;
			instantCoin.transform.localPosition = Vector3.zero;

			isPop = true;
		}
	}

	void OnDrawGizmos() {
		Gizmos.color = Color.blue;
		Gizmos.DrawSphere(transform.position, 0.25f);
	}
}
