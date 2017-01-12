using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LevelItem : MonoBehaviour {

	[SerializeField] private int levelNumber;
	private bool selected = false;

	[Header("Main Button")]
	[SerializeField] private Image selectBorder;
	[SerializeField] private Image myImage;
	[SerializeField] private Animator myAnim;
	[SerializeField] private Text levelName;

	[SerializeField] private Color selectColor;
	[SerializeField] private Color deselectColor;
	[SerializeField] private Color borderDeselectColor;

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
		// Contrôle sur le nombre de level existant
		if (levelNumber > GameData.gameData.playerData.levelData.Count) {
			DisableLevelItem ();
			return;
		}
		
		// Activation du level si le boss du level précédent est mort
		if (levelNumber > 1 && !GameData.gameData.playerData.levelData [GetLevelNumber () - GameData.gameData.firstLevel - 1].storyData [0].isBossDead) {
			DisableLevelItem ();
			return;
		}

		// Récupération du titre
		levelName.text = GameData.gameData.playerData.levelData [GetLevelNumber () - GameData.gameData.firstLevel].levelName;

		// On n'affiche les boutons que si au moins le premier existe
		if (storyNormal != null) {
			// Toujours activé
			storyNormal.InitModeButton (GetLevelNumber(), true);
			// Si le boss de la difficulté "normal" est mort, on active "hard" et "arcade"
			bool buttonActivationHard = GameData.gameData.playerData.levelData [GetLevelNumber () - GameData.gameData.firstLevel].storyData [0].isBossDead;
			storyHard.InitModeButton (GetLevelNumber(), buttonActivationHard);
			arcade.InitModeButton (GetLevelNumber(), buttonActivationHard);
			// Si le boss de la difficulté "hard" est mort, on active "hell"
			bool buttonActivationHell = GameData.gameData.playerData.levelData [GetLevelNumber () - GameData.gameData.firstLevel].storyData [1].isBossDead;
			storyHell.InitModeButton (GetLevelNumber(), buttonActivationHell);
		}

		// On active le level en cours, et on "ferme" les autres
		if (_GameData.currentLevel != GetLevelNumber ())
			DeselectLevelItem ();
		else
			SelectLevelItem (true);
	}

	public void DeselectLevelItem() {
		selected = false;

		myAnim.StartPlayback ();
		myImage.color = deselectColor;
		selectBorder.color = borderDeselectColor;
		levelName.gameObject.SetActive (false);

		if (storyNormal != null) {
			float transitionTime = 0.1f;
			StartCoroutine (DepopButton (storyNormal, transitionTime));
			if (storyHard.activated)
				StartCoroutine (DepopButton (storyHard, transitionTime));
			if (storyHell.activated)
				StartCoroutine (DepopButton (storyHell, transitionTime));
			if (arcade.activated)
				StartCoroutine (DepopButton (arcade, transitionTime));
		}
	}

	public void SelectLevelItem(bool firstTime = false) {
		selected = true;

		myAnim.StopPlayback ();
		myImage.color = selectColor;
		selectBorder.color = selectColor;
		levelName.gameObject.SetActive (true);

		if (storyNormal != null) {
			if (!firstTime) {
				float transitionTime = 0.25f;
				StartCoroutine (PopButton (storyNormal, transitionTime, 0f));
				if (storyHard.activated)
					StartCoroutine (PopButton (storyHard, transitionTime, 0.05f));
				if (storyHell.activated)
					StartCoroutine (PopButton (storyHell, transitionTime, 0.1f));
				if (arcade.activated)
					StartCoroutine (PopButton (arcade, transitionTime, 0f));
			} else {
				StartCoroutine (PopButton (storyNormal, 0, 0));
				if (storyHard.activated)
					StartCoroutine (PopButton (storyHard, 0, 0));
				if (storyHell.activated)
					StartCoroutine (PopButton (storyHell, 0, 0));
				if (arcade.activated)
					StartCoroutine (PopButton (arcade, 0, 0));
			}
		}

		MainMenuManager.sfxSound.ButtonYesClick ();
	}

	private void DisableLevelItem () {
		gameObject.SetActive (false);
	}

	private IEnumerator PopButton(ModeButton obj, float transitionTime = 0, float delay = 0) {
		if (delay > 0)
			yield return new WaitForSecondsRealtime (delay);

		obj.rectTransform.localScale = Vector3.zero;
		obj.gameObject.SetActive (true);

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
		obj.rectTransform.anchorMax = obj.initialPosition;
		obj.rectTransform.anchorMin = obj.initialPosition;
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
		obj.gameObject.SetActive (false);
	}
}
