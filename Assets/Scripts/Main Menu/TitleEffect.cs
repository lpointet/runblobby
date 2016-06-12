using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class TitleEffect : MonoBehaviour {

	private Text[] letter;

	public List<Color> randomColorLetter;

	void Awake() {
		letter = GetComponentsInChildren<Text> ();
		randomColorLetter.Add (letter [0].color);
	}
	
	public void Effect() {
		int randLetter = Random.Range (0, letter.Length);

		// On enlève de la liste la couleur de la lettre choisie, on choisit une couleur, puis on remet l'ancienne couleur dans la liste
		// Ceci nous évite d'avoir la même couleur qui se réapplique sur la même lettre
		randomColorLetter.Remove (letter [randLetter].color);
		int randColor = Random.Range (0, randomColorLetter.Count);
		randomColorLetter.Add (letter [randLetter].color);

		letter [randLetter].color = randomColorLetter [randColor];
	}
}