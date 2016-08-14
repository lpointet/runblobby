using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// S'il faut augmenter le nombre de munition (maximum technique de 10 actuellement), le graphique et les maxValue doivent être revues
public class AmmoManager : MonoBehaviour {

	private Slider ammunition;
	[SerializeField] private Image ammoReloadBar;
	[SerializeField] private Slider ammoMask;

	void Awake () {
		ammunition = GetComponent<Slider> ();
	}

	void Update () {
		if (LevelManager.levelManager.GetEnemyEnCours () != null) {
			ammoMask.value = LevelManager.player.GetWeapon ().currentWeaponAmmunition;
			ammunition.value = Mathf.FloorToInt (LevelManager.player.GetWeapon ().GetCurrentAmmo ());
			ammoReloadBar.fillAmount = LevelManager.player.GetWeapon ().GetCurrentAmmo () % 1;
		}
	}
}
