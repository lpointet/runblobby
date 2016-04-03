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

	private float maxAnchor;
	private int paddingSize;

	public float speedScroll = 1f;
	public float pressedSpeedScroll = 5f;
	private float dragTime;
	private bool pressed = false;

	void Start() {
		StartCoroutine (WaitEndOfFrame ());
	}

	private IEnumerator WaitEndOfFrame() {
		yield return new WaitForEndOfFrame();

		// Padding TOP et BOT (commencer/terminer au milieu)
		paddingSize = Mathf.RoundToInt(Screen.height * (GetComponent<RectTransform>().anchorMax.y - GetComponent<RectTransform>().anchorMin.y) / 2f);
		contentPanel.GetComponent<VerticalLayoutGroup> ().padding = new RectOffset (0, 0, paddingSize, paddingSize);

		maxAnchor = contentPanel.sizeDelta.y;
	}

	void OnEnable () {
		contentPanel.anchoredPosition = Vector2.zero;
		dragTime = Time.unscaledTime - 1; // (2-1)s de lancement
	}

	void Update () {
		// Si on drag, on arrête le défilement pendant 5sec
		if ((contentPanel.anchoredPosition.y >= -1 && contentPanel.anchoredPosition.y < maxAnchor && dragTime + 2 < Time.unscaledTime) || pressed) {
			float currentSpeed;
			currentSpeed = pressed ? pressedSpeedScroll : speedScroll; // On ajuste la vitesse en fonction de ce qu'il se passe
			contentPanel.anchoredPosition = new Vector2 (0, contentPanel.anchoredPosition.y + currentSpeed);

			// Si on dépasse les limites, on recadre (pour éviter de ne plus appeler cette fonction)
			if (contentPanel.anchoredPosition.y < 0)
				contentPanel.anchoredPosition = Vector2.zero;
			else if (contentPanel.anchoredPosition.y >= maxAnchor)
				contentPanel.anchoredPosition = new Vector2 (0, maxAnchor);
		}
	}

	public void Pressing() {
		pressed = true;
		// On agit différement selon que l'on clique plus haut ou plus bas
		pressedSpeedScroll = (Camera.main.ScreenToWorldPoint (Input.mousePosition).y > 0) ? -Mathf.Abs (pressedSpeedScroll) : Mathf.Abs (pressedSpeedScroll);
	}

	public void ReleasePression() {
		pressed = false;
		dragTime = Time.unscaledTime;
	}
}
