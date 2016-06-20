using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StartLevel : MonoBehaviour {

	public float delayBeforeStart = 1.5f;
	private float startingTime;

	private float startingTimeScale = 0;
	private float endingScale;	// Doit avoir la taille de l'écran à la fin
	private float currentScaling;

	public Text levelName;

	private bool disappearCalled = false;

	void Start () {
		startingTime = TimeManager.time;

		CameraManager cameraManager = Camera.main.GetComponent<CameraManager> ();

		endingScale = Camera.main.orthographicSize * 2 * 32 / 8; // Taille de l'écran * résolution par unité / rayon du cercle "visible"
		// On place le joueur dans de bonnes conditions de chute en parachute
		LevelManager.GetPlayer ().GetComponent<Rigidbody2D> ().gravityScale = 0.1f;
		LevelManager.GetPlayer ().transform.position = new Vector2 (0, Camera.main.orthographicSize + cameraManager.yOffset + 1);

		// On accroche le parachute au joueur
		LevelManager.GetPlayer ().ActiveParachute (true);

		if (_GameData.currentLevelName == null)
			_GameData.currentLevelName = "Nameless Level";

		StartCoroutine (LetterByLetter (_GameData.currentLevelName));
		levelName.gameObject.SetActive (true);
	}

	void Update () {
		// On attend la fin du délai pour démarrer
		if (!disappearCalled && startingTime + delayBeforeStart < TimeManager.time) {
			disappearCalled = true;

			LevelManager.levelManager.StartLevel ();
			Debug.Log ("Level started");

			StartCoroutine (Disparition ());
		} else {
			// On agrandit la taille du cercle au fil du temps
			startingTimeScale = TimeManager.time * TimeManager.time;
			currentScaling = _StaticFunction.MappingScale (startingTimeScale, startingTime, startingTime + delayBeforeStart, 1, endingScale);
			transform.localScale = Vector2.one * currentScaling;

			// On fait suivre au cercle le joueur (mais pas sur z)
			transform.position = new Vector3 (LevelManager.GetPlayer ().transform.position.x, LevelManager.GetPlayer ().transform.position.y, transform.position.z);
		}
	}

	private IEnumerator LetterByLetter (string text) {
		string tempString = "";
		for (int i = 0; i < text.Length; i++) {
			tempString += text [i];
			levelName.text = tempString;
			yield return new WaitForSeconds ((delayBeforeStart / 2f / text.Length) * Time.timeScale);
		}
	}

	// Disparition "douce" du texte
	private IEnumerator Disparition () {
		while (levelName.color.a > 0) {
			Color tempColor = levelName.color;
			tempColor.a -= TimeManager.deltaTime / delayBeforeStart;
			levelName.color = tempColor;
			yield return null;
		}
		levelName.gameObject.SetActive (false);
		gameObject.SetActive (false);
	}
}
