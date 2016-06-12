using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LevelItem : MonoBehaviour {

	[System.Serializable]
	public class ModeButton {
		public GameObject button;
		public RectTransform rectTransform;
		public Vector2 initialPosition;
	}

	[SerializeField] private int levelNumber;
	private bool selected = false;

	[Header("Main Button")]
	[SerializeField] private Image background;
	[SerializeField] private Animator myBGAnim;
	[SerializeField] private GameObject selectBorder;
	[SerializeField] private Image myImage;
	[SerializeField] private Animator myAnim;

	[SerializeField] private Color selectColor;
	[SerializeField] private Color deselectColor;

	[Header("Mode Button")]
	[SerializeField] private ModeButton storyNormal;
	[SerializeField] private ModeButton storyHard;
	[SerializeField] private ModeButton storyHell;
	[SerializeField] private ModeButton arcade;

	public int GetLevelNumber() {
		return levelNumber;
	}

	public bool IsSelected() {
		return selected;
	}

	void Start () {
		// On n'affiche les boutons que si au moins le premier existe
		if (storyNormal.button != null) {
			InitModeButton (storyNormal);
			InitModeButton (storyHard);
			InitModeButton (storyHell);
			InitModeButton (arcade);
		}

		DeselectLevelItem();
	}

	private void InitModeButton(ModeButton mode) {
		mode.rectTransform = mode.button.GetComponent<RectTransform> ();
		mode.initialPosition = mode.rectTransform.anchorMax;
		mode.button.SetActive (false);
	}

	public void DeselectLevelItem() {
		selected = false;

		myBGAnim.StartPlayback ();
		myAnim.StartPlayback ();
		background.color = deselectColor;
		myImage.color = deselectColor;
		selectBorder.SetActive (false);

		if (storyNormal.button != null) {
			float transitionTime = 0.1f;
			StartCoroutine (DepopButton (storyNormal, transitionTime));
			StartCoroutine (DepopButton (storyHard, transitionTime));
			StartCoroutine (DepopButton (storyHell, transitionTime));
			StartCoroutine (DepopButton (arcade, transitionTime));
		}
	}

	public void SelectLevelItem() {
		selected = true;

		myBGAnim.StopPlayback ();
		myAnim.StopPlayback ();
		background.color = selectColor;
		myImage.color = selectColor;
		selectBorder.SetActive (true);

		if (storyNormal.button != null) {
			float transitionTime = 0.25f;
			StartCoroutine (PopButton (storyNormal, transitionTime, 0));
			StartCoroutine (PopButton (storyHard, transitionTime, 0.05f));
			StartCoroutine (PopButton (storyHell, transitionTime, 0.1f));
			StartCoroutine (PopButton (arcade, transitionTime, 0f));
		}
	}

	private IEnumerator PopButton(ModeButton obj, float transitionTime = 0, float delay = 0) {
		yield return new WaitForSeconds (delay * Time.timeScale);

		obj.rectTransform.localScale = Vector3.zero;
		obj.button.SetActive (true);

		float timeToComplete = 0;
		float popScale = 0;
		float maxScale = 1.75f;
		Vector2 translateVector = Vector2.one * 0.5f;

		while (timeToComplete < transitionTime) {
			popScale = Mathf.Lerp (0, maxScale, timeToComplete / transitionTime);
			obj.rectTransform.localScale = popScale * Vector3.one;

			translateVector = Vector2.Lerp (translateVector, obj.initialPosition, timeToComplete / transitionTime);
			obj.rectTransform.anchorMax = translateVector;
			obj.rectTransform.anchorMin = translateVector;

			timeToComplete += Time.deltaTime / Time.timeScale;
			yield return null;
		}

		obj.rectTransform.localScale = Vector3.one;
	}

	private IEnumerator DepopButton(ModeButton obj, float transitionTime = 0) {
		float timeToComplete = 0;
		float depopScale = 0;
		Vector2 endVector = Vector2.one * 0.5f;
		Vector2 translateVector;

		while (timeToComplete < transitionTime) {
			depopScale = Mathf.Lerp (1, 0, timeToComplete / transitionTime);
			obj.rectTransform.localScale = depopScale * Vector3.one;

			translateVector = Vector2.Lerp (obj.initialPosition, endVector, timeToComplete / transitionTime);
			obj.rectTransform.anchorMax = translateVector;
			obj.rectTransform.anchorMin = translateVector;

			timeToComplete += Time.deltaTime / Time.timeScale;
			yield return null;
		}

		obj.rectTransform.localScale = Vector3.zero;
		obj.button.SetActive (false);
	}

	/* Lancement du jeu selon le mode indiqué
	 * 0 = story normal
	 * 1 = story hard
	 * 2 = story hell
	 * 9 = arcade */
	public void Mode_Click(int mode) {
		// Ajustement des paramètres avant de lancer le niveau
		_GameData.currentLevel = GetLevelNumber();
		_GameData.currentLevelName = GameData.gameData.playerData.levelData [GetLevelNumber()].levelName;

		switch (mode) {
		case 0:
		case 1:
		case 2:
			_GameData.currentDifficulty = mode; // TODO difficulté : v2
			_GameData.isStory = true;
			break;
		case 9:
			_GameData.isStory = false;
			break;
		default:
			Debug.LogWarning ("404. Difficulty not found.");
			break;
		}
			
		MainMenuManager.mainMenuManager.LoadLevel (GetLevelNumber());
	}
}
