using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ModeButton : MonoBehaviour {

	[HideInInspector] public RectTransform rectTransform;
	[HideInInspector] public Vector2 initialPosition;

	private int levelNumber;
	[SerializeField] private int mode; // 0 = normal, 1 = hard, 2 = hell, 9 = arcade

	[HideInInspector] public bool activated;

	[SerializeField] private Color colorTokenInactif;
	[SerializeField] private Image tokenDistance;
	[SerializeField] private Color colorDistance;
	[SerializeField] private Image tokenLeaf;
	[SerializeField] private Color colorLeaf;
	[SerializeField] private Image tokenLife;
	[SerializeField] private Color colorLife;

	void Awake () {
		rectTransform = GetComponent<RectTransform> ();
		initialPosition = rectTransform.anchorMax;

		gameObject.SetActive (false);
	}

	public void InitModeButton (int levelNumber, bool activated) {
		this.levelNumber = levelNumber;
		this.activated = activated;

		// Si en mode "histoire"
		if (mode != 9) {
			StoryData storyData = GameData.gameData.playerData.levelData [levelNumber - 1].storyData [mode];

			if (tokenDistance != null) {
				if (storyData.distanceRecord >= storyData.distanceMax)
					tokenDistance.color = colorDistance;
				else
					tokenDistance.color = colorTokenInactif;
			}
			if (tokenLeaf != null) {
				if (storyData.scoreRatioRecord >= 0.8f)
					tokenLeaf.color = colorLeaf;
				else
					tokenLeaf.color = colorTokenInactif;
			}
			if (tokenLife != null) {
				if (storyData.healthRatioRecord >= 0.75f)
					tokenLife.color = colorLife;
				else
					tokenLife.color = colorTokenInactif;
			}
		} else { // Si en mode "arcade"
			if (tokenDistance != null) {
				if (GameData.gameData.playerData.levelData [levelNumber - 1].arcadeData.distanceRecord >= 5000)
					tokenDistance.color = colorDistance;
				else
					tokenDistance.color = colorTokenInactif;
			}
		}
	}

	/* Lancement du jeu selon le mode indiqué
	 * 0 = story normal
	 * 1 = story hard
	 * 2 = story hell
	 * 9 = arcade */
	public void Mode_Click() {
		// Ajustement des paramètres avant de lancer le niveau
		_GameData.currentLevel = levelNumber;
		_GameData.currentLevelName = GameData.gameData.playerData.levelData [levelNumber - 1].levelName;

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

		MainMenuManager.mainMenuManager.LoadLevel (levelNumber);
	}
}
