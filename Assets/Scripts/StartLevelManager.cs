using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class StartLevelManager : MonoBehaviour {

	private PlayerController player;

	Text countdownText;

	public float scalingTextBegin;
	private float scalingText;
	public int tailleCountdown;

	// Use this for initialization
	void Start () {
		countdownText = GetComponent<Text> ();
		player = FindObjectOfType<PlayerController> ();

		countdownText.text = "" + tailleCountdown;
		scalingText = scalingTextBegin;

		player.enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
		scalingText -= scalingTextBegin * Time.deltaTime; // change au bout d'une seconde en théorie

		if (scalingText <= 0) {
			tailleCountdown--;
			if (tailleCountdown > 0)
				countdownText.text = "" + tailleCountdown;
			else
				countdownText.text = "GO!";
			scalingText = scalingTextBegin;
		}
		if (tailleCountdown >= 0) {
			countdownText.transform.localScale = new Vector3(scalingText, scalingText, 0);
		} else {
			player.enabled = true;
			Destroy(gameObject);
		}
	}
}
