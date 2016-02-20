using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CreditScrollList : MonoBehaviour {

	public TextAsset creditFile;
	private string fileOneString;
	private List<string> eachLine;
	private Match testTypeLine;

	public RectTransform contentPanel;

	public GameObject itemSection;
	public GameObject itemContenu;
	public GameObject itemSaut;
	private GameObject item;

	public float speedScroll = 1f;
	public float pressedSpeedScroll = 2.5f;
	private float dragTime;
	private bool pressed = false;

	void Start() {
		// Chargement du fichier
		fileOneString = creditFile.text;
		eachLine = new List<string> (fileOneString.Split("\n"[0]));

		// Remplissage ligne par ligne
		foreach (string line in eachLine) {
			testTypeLine = Regex.Match(line, "#(.+)#");

			switch (testTypeLine.Groups[1].Value) {
			case "Section":
				item = Instantiate (itemSection);
				break;
			case "Contenu":
				item = Instantiate (itemContenu);
				break;
			case "Saut":
				item = Instantiate (itemSaut);
				break;
			}
			item.transform.SetParent (contentPanel, false);
		}

		// Padding TOP et BOT (commencer/terminer au milieu)
		int paddingSize = Mathf.RoundToInt(Screen.height * (GetComponent<RectTransform>().anchorMax.y - GetComponent<RectTransform>().anchorMin.y) / 2 - itemSection.GetComponent<LayoutElement>().minHeight);
		contentPanel.GetComponent<VerticalLayoutGroup> ().padding = new RectOffset (0, 0, paddingSize, paddingSize);
	}

	void OnEnable () {
		contentPanel.anchoredPosition = Vector2.zero;
		dragTime = Time.time - 1; // (2-1)s de lancement
	}

	void Update () {
		// Si on drag, on arrête le défilement pendant 5sec
		// TODO 661 est une valeur arbitraire, je ne sais pas comment la calculer...
		if ((contentPanel.anchoredPosition.y >= -1 && contentPanel.anchoredPosition.y < 661 && dragTime + 2 < Time.time) || pressed) {
			float currentSpeed;
			currentSpeed = pressed ? pressedSpeedScroll : speedScroll; // On ajuste la vitesse en fonction de ce qu'il se passe
			contentPanel.anchoredPosition = new Vector2 (0, contentPanel.anchoredPosition.y + currentSpeed);

			// Si on dépasse les limites, on recadre (pour éviter de ne plus appeler cette fonction)
			if (contentPanel.anchoredPosition.y + currentSpeed < 0)
				contentPanel.anchoredPosition = Vector2.zero;
			else if (contentPanel.anchoredPosition.y + currentSpeed >= 661)
				contentPanel.anchoredPosition = new Vector2 (0, 660);
		}

		// TODO il faut faire des boutons pour accès rapides
	}

	public void Pressing() {
		pressed = true;
		// On agit différement selon que l'on clique plus haut ou plus bas
		pressedSpeedScroll = (Camera.main.ScreenToWorldPoint (Input.mousePosition).y > 0) ? -Mathf.Abs (pressedSpeedScroll) : Mathf.Abs (pressedSpeedScroll);
	}

	public void ReleasePression() {
		pressed = false;
		dragTime = Time.time;
	}
}
