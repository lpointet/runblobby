using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TitleEffect : MonoBehaviour {

	private Text[] letter;

	private Color32 Blanc = _StaticFunction.ToColor (0xFFFFFF);
	private Color32 BleuClair = _StaticFunction.ToColor (0x4DACF9);
	private Color32 BleuFonce = _StaticFunction.ToColor(0x034590);

	private Color32[] couleur;

	void Awake() {
		letter = GetComponentsInChildren<Text> ();
		couleur = new Color32[] {Blanc, BleuClair, BleuFonce};
	}
	
	public void Effect() {
		int randLetter = Random.Range (0, 4);
		int randColor = Random.Range (0, 3);

		letter [randLetter].color = couleur [randColor];
	}
}
