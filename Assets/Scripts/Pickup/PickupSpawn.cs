using UnityEngine;
using System.Collections.Generic;

public class PickupSpawn : MonoBehaviour {

	private LevelManager levelManager;

	public Pickup[] specificPickup;
	private List<Pickup> listeBonus = new List<Pickup>();
	public int[] specificWeight;
	private List<int> weightBonus = new List<int>();
	private int poidsTotal = 100;

	private float calculusDistance = 0;
	private Vector2 initialPosition;
	private GameObject instantPickup;

    void Awake () {
		levelManager = FindObjectOfType<LevelManager> ();
		initialPosition = transform.localPosition;

		// Si onspecificPickupne veut pas de la liste classique, on doit renseigner ceux que l'on veut ainsi que les poids associés
		if (specificPickup.Length > 0) {
			for (int i = 0; i < specificPickup.Length; i++) {
				int currentWeight = 0;
				listeBonus.Add (specificPickup[i]);
				// Si l'on ne veut pas les poids standards, on doit les changer également (il doit aussi y en avoir 1 pour 1 pickup)
				if (specificPickup.Length == specificWeight.Length) {
					currentWeight = specificWeight [i];
				} else {
					currentWeight = specificPickup [i].weight;
				}
				weightBonus.Add (currentWeight);

				poidsTotal += currentWeight;
			}
		} else {
			for (int i = 0; i < ListManager.current.powerups.Length; i++) {
				listeBonus.Add (ListManager.current.powerups[i]);
				weightBonus.Add (ListManager.current.powerups[i].weight);
			}
		}
	}

	void Start () {
		
	}

	void OnEnable () {
		// On remet le point à sa place, non mais.
		transform.localPosition = initialPosition;

        // On désactive les pickups non pris, non mais.
        if( null != instantPickup ) {
			instantPickup.gameObject.SetActive (false);
        }

		int distanceSansbonus = 200;
		int distanceMaxBonus = 1000;
		calculusDistance = (float)_StaticFunction.MathPower ((levelManager.GetDistanceSinceLastBonus () - distanceSansbonus) / (float)(distanceMaxBonus - distanceSansbonus), 3);
		calculusDistance = 1;
		// Ajuster le calcul de la distance pour que :
		// Entre 0 et 200m : rien (pour assurer une distance mini sans bonus)
		// Entre 200m et 1000m : on monte progressivement de 0 à 1, avec une valeur à 600m de 0.1
		if (Mathf.Max (0, calculusDistance) < Random.value) 
			return;

		levelManager.ResetBonusDistance ();
		// On parcourt l'ensemble des pickups possibles, chacun avec sa probabilité d'apparition relative à l'ensemble des possibilités
		int random = Random.Range (0, poidsTotal);
		int i, choix;
		for (i = 0, choix = 0; i <= random; i += weightBonus [choix++]);

		// TODO empêcher dans la mesure du possible qu'un pickup déjà actif apparaisse
		instantPickup = PoolingManager.current.Spawn (listeBonus[choix-1].name);

		instantPickup.gameObject.SetActive (true);

		instantPickup.transform.position = transform.position;
		instantPickup.transform.parent = transform.parent;
    }
}
