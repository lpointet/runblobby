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

	public int distanceSansbonus = 200;
	public int distanceMaxBonus = 1000;
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
				// Si le pickup n'est pas activé, on passe au suivant
				if (!IsPickupActivated (specificPickup [i]))
					continue;
				
				listeBonus.Add (specificPickup[i]);

				int currentWeight = 0;
				// Si l'on ne veut pas les poids standards, on doit les changer également (il doit aussi y en avoir 1 pour 1 pickup)
				if (specificPickup.Length == specificWeight.Length) {
					currentWeight = specificWeight [i];
				} else {
					currentWeight = specificPickup [i].weight;
				}
				weightBonus.Add (currentWeight);
			}
		} else {
			for (int i = 0; i < ListManager.current.powerups.Length; i++) {
				// Si le pickup n'est pas activé, on passe au suivant
				if (!IsPickupActivated (ListManager.current.powerups[i]))
					continue;
				
				listeBonus.Add (ListManager.current.powerups[i]);
				weightBonus.Add (ListManager.current.powerups[i].weight);
			}
		}
	}

	void Start () {
		// Ajout des points de talent
		distanceSansbonus = Mathf.RoundToInt (distanceSansbonus * (100 - GameData.gameData.playerData.talent.buffDelay * GameData.gameData.playerData.talent.buffDelayPointValue) / 100f);
		distanceMaxBonus = Mathf.RoundToInt (distanceMaxBonus * (100 - GameData.gameData.playerData.talent.buffDelay * GameData.gameData.playerData.talent.buffDelayPointValue) / 100f);
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
		// On ne fait pas apparaître d'autres pickups si le joueur est actuellement sous Last Wish actif
		if (LevelManager.player.HasLastWish () && LevelManager.player.GetLastWish ().IsLaunched ())
			return;
		
		if (!activated && myTransform.position.x < CameraManager.cameraEndPosition) {
			activated = true;

			calculusDistance = (float)_StaticFunction.MathPower ((levelManager.GetDistanceSinceLastBonus () - distanceSansbonus) / (float)(distanceMaxBonus - distanceSansbonus), 3);
			calculusDistance = 1;
			// Ajuster le calcul de la distance pour que :
			// Entre 0 					et distanceSansbonus : rien (pour assurer une distance mini sans bonus)
			// Entre distanceSansbonus 	et distanceMaxBonus  : on monte progressivement de 0 à 1, avec une valeur à (distanceMaxBonus + distanceSansbonus) / 2 de 0.1
			if (Mathf.Max (0, calculusDistance) < Random.value)
				return;

			// On créé une liste temporaire en ôtant les pickups que le joueur a déjà
			List<Pickup> tempBonus = new List<Pickup> ();
			List<int> tempWeigh = new List<int> ();
			poidsTotal = 0;

			for (int i = 0; i < listeBonus.Count; i++) {
				// Si le joueur est full life, on ne fait pas apparaître de "soin"
				if (listeBonus [i].GetType () == typeof(HealPickup) && LevelManager.player.healthPoint >= LevelManager.player.healthPointMax)
					continue;

				// Si le joueur a le pickup, on l'enlève de la liste
				if (!LevelManager.player.HasTypePickup (listeBonus [i].GetType ())) {
					tempBonus.Add (listeBonus [i]);
					tempWeigh.Add (weightBonus [i]);

					poidsTotal += weightBonus [i];
				}
			}

			// S'il n'y a aucun pickup ou qu'ils n'ont aucun poids, on ne fait rien
			if (poidsTotal <= 0)
				return;

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

	private bool IsPickupActivated (Pickup pickup) {

		if (pickup.GetType () == typeof(MultiplierPickup))
			return GameData.gameData.playerData.talent.leaf > 0 ? true : false;
		
		if (pickup.GetType () == typeof(HealPickup))
			return GameData.gameData.playerData.talent.heal > 0 ? true : false;
		
		if (pickup.GetType () == typeof(CloudPickup))
			return GameData.gameData.playerData.talent.cloud > 0 ? true : false;
		
		if (pickup.GetType () == typeof(LastWishPickup))
			return GameData.gameData.playerData.talent.lastWish > 0 ? true : false;

		return true;
	}

	void OnDrawGizmos() {
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(transform.position, 0.25f);
	}
}
