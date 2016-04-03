using UnityEngine;
using System.Collections.Generic;

public class PickupSpawn : MonoBehaviour {

	private LevelManager levelManager;
	private Transform myTransform;

	public Pickup[] specificPickup;
	private List<Pickup> listeBonus = new List<Pickup>();
	public int[] specificWeight;
	private List<int> weightBonus = new List<int>();
	private int poidsTotal = 100;

	private float calculusDistance = 0;
	private Vector2 initialPosition;
	private GameObject instantPickup;

	private bool activated = false;

    void Awake () {
		levelManager = LevelManager.levelManager;
		myTransform = transform;
		initialPosition = myTransform.localPosition;

		// Si on ne veut pas de la liste classique, on doit renseigner ceux que l'on veut ainsi que les poids associés
		if (specificPickup.Length > 0) {
			for (int i = 0; i < specificPickup.Length; i++) {
				listeBonus.Add (specificPickup[i]);

				int currentWeight = 0;
				// Si l'on ne veut pas les poids standards, on doit les changer également (il doit aussi y en avoir 1 pour 1 pickup)
				if (specificPickup.Length == specificWeight.Length) {
					currentWeight = specificWeight [i];
				} else {
					currentWeight = specificPickup [i].weight;
				}
				weightBonus.Add (currentWeight);

				//poidsTotal += currentWeight;
			}
		} else {
			for (int i = 0; i < ListManager.current.powerups.Length; i++) {
				listeBonus.Add (ListManager.current.powerups[i]);
				weightBonus.Add (ListManager.current.powerups[i].weight);
			}
		}
	}

	void OnEnable () {
		// On remet le point à sa place, non mais.
		myTransform.localPosition = initialPosition;

		// On désactive les pickups non pris, non mais.
		if (null != instantPickup) {
			instantPickup.gameObject.SetActive (false);
		}

		// On permet de faire réapparaître le pickup
		activated = false;
	}

	void Update () {
		if (!activated && myTransform.position.x < levelManager.cameraEndPosition) {
			activated = true;

			int distanceSansbonus = 200;
			int distanceMaxBonus = 1000;
			calculusDistance = (float)_StaticFunction.MathPower ((levelManager.GetDistanceSinceLastBonus () - distanceSansbonus) / (float)(distanceMaxBonus - distanceSansbonus), 3);
			//calculusDistance = 1; // TODO supprimer après les tests d'apparition !!
			// Ajuster le calcul de la distance pour que :
			// Entre 0 et 200m : rien (pour assurer une distance mini sans bonus)
			// Entre 200m et 1000m : on monte progressivement de 0 à 1, avec une valeur à 600m de 0.1
			if (Mathf.Max (0, calculusDistance) < Random.value)
				return;

			// On créé une liste temporaire en ôtant les pickups que le joueur a déjà
			List<Pickup> tempBonus = new List<Pickup> ();
			List<int> tempWeigh = new List<int> ();
			poidsTotal = 0;

			for (int i = 0; i < listeBonus.Count; i++) {
				if (!LevelManager.GetPlayer ().HasTypePickup (listeBonus [i].GetType ())) {
					tempBonus.Add (listeBonus [i]);
					tempWeigh.Add (weightBonus [i]);

					poidsTotal += weightBonus [i];
				}
			}

			levelManager.ResetBonusDistance ();

			// On parcourt l'ensemble des pickups possibles, chacun avec sa probabilité d'apparition relative à l'ensemble des possibilités
			int choix = 1;

			if (tempWeigh.Count > 1) {
				int random = Random.Range (0, poidsTotal);
				int i;
				for (i = 0, choix = 0; i <= random; i += tempWeigh [choix++]);
			}

			instantPickup = PoolingManager.current.Spawn (tempBonus [choix - 1].name);

			instantPickup.gameObject.SetActive (true);

			instantPickup.transform.parent = myTransform;
			instantPickup.transform.localPosition = Vector3.zero;
		}
    }
}
