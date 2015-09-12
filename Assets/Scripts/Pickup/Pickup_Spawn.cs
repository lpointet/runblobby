using UnityEngine;
using System.Collections.Generic;

public class Pickup_Spawn : MonoBehaviour {

	private LevelManager levelManager;

	public List<GameObject> possibleBonus;
	private List<Pickup> listeBonus = new List<Pickup>();
	private List<int> weightBonus = new List<int>();
	private int poidsTotal = 0;

	private float calculusDistance = 0;
	private Vector2 initialPosition;
	
	void Awake () {
		levelManager = FindObjectOfType<LevelManager> ();
		initialPosition = transform.localPosition;

		Pickup tempPickup;
		foreach (GameObject pickup in possibleBonus) {
			tempPickup = pickup.GetComponent<Pickup>();
			listeBonus.Add(tempPickup);
			weightBonus.Add(tempPickup.weight);
			poidsTotal += tempPickup.weight;
		}
	}

	void OnEnable () {
		// On remet le point à sa place, non mais.
		transform.localPosition = initialPosition;

		int distanceSansbonus = 200;
		int distanceMaxBonus = 1000;
		calculusDistance = (float)_StaticFunction.MathPower ((levelManager.GetDistanceSinceLastBonus () - distanceSansbonus) / (float)(distanceMaxBonus - distanceSansbonus), 3);
		// Ajuster le calcul de la distance pour que :
		// Entre 0 et 200m : rien (pour assurer une distance mini sans bonus)
		// Entre 200m et 1000m : on monte progressivement de 0 à 1, avec une valeur à 600m de 0.1
		if (Mathf.Max (0, calculusDistance) < Random.value) 
			return;

		levelManager.ResetBonusDistance ();
		// On parcourt l'ensemble des pickups possibles, chacun avec sa probabilité d'apparition relative à l'ensemble des possibilités
		int random = Random.Range (0, poidsTotal);
		int i, choix;
		for(i = 0, choix = 0; i <= random; i += weightBonus[choix++]);
		Pickup instantPickup = Instantiate (listeBonus[choix-1], transform.position, transform.rotation) as Pickup;
		instantPickup.transform.parent = transform.parent;
	}
}
