using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class LevelScrollList : MonoBehaviour {
	
	public RectTransform contentPanel;

	public Button upLevel;
	public Button downLevel;

	public GameObject levelItem;
	//public List<Level> levelList;
	
	private Button[] levelButton;
	public RectTransform center; // Centre du contentPanel pour comparer la distance au bouton
	
	private float[] distance; // Distance des boutons au centre
	private bool dragging = false; // True quand on bouge le panneau
	private int buttonDistance; // Distance entre les boutons
	private int minButtonNum = 0; // Bouton le plus proche du centre

	private float currentLerpTime;
	private float lerpTime = 1f; // Temps pour se "caler"

	private SFXMenu sfxSound;
	private int recordMinButton = 0;

	void Awake() {
		sfxSound = GameObject.FindObjectOfType<SFXMenu> ();
	}
	
	void Start () {
		// On cache la liste avant peuplement
		GetComponent<Image> ().enabled = false;

		PopulateList ();

		levelButton = contentPanel.GetComponentsInChildren<Button> ();
		distance = new float[levelButton.Length];

		StartCoroutine (WaitEndOfFrame ());
	}

	void OnEnable() {
		//PlacerLevel (0);
	}

	private IEnumerator WaitEndOfFrame() {
		yield return new WaitForEndOfFrame();
		
		// Distance entre les boutons (ils sont placés à la fin de la première frame seulement)
		if (levelButton.Length > 1)
			buttonDistance = (int)Mathf.Abs (levelButton [1].GetComponent<RectTransform> ().anchoredPosition.y - levelButton [0].GetComponent<RectTransform> ().anchoredPosition.y);
		else
			buttonDistance = 0;

		currentLerpTime = lerpTime; // Pour éviter de rendre visible le déplacement (instantané du coup)
		PlacerLevel (0);

		// On réaffiche la liste une fois peuplée
		GetComponent<Image> ().enabled = true;
	}

	void Update() {
		// On cherche la distance minimale au centre du contenu (matérialisé par un autre point non lié au contenu)
		for (int i = 0; i < levelButton.Length; i++) {
			distance [i] = Mathf.Abs (center.transform.position.y - levelButton [i].transform.position.y);
		}
		float minDistance = Mathf.Min (distance);

		if (dragging) {
			// On récupère l'index du level le plus proche du centre
			for (int j = 0; j < levelButton.Length; j++) {
				if (minDistance == distance [j]) 
					minButtonNum = j;
			}
		
			// On "glisse" vers la position centrale quand on arrete de tirer
			currentLerpTime = 0;
		}
		else {
			// On approche de la fin du positionnement une fois par image
			currentLerpTime += Time.unscaledDeltaTime;
			if(currentLerpTime > lerpTime)
				currentLerpTime = lerpTime;

			PlacerLevel(minButtonNum);
		}

		RafraichirImageLevel ();
	}

	// Permet de remplir la levelList
	private void PopulateList() {
		foreach (LevelData item in GameData.gameData.playerData.levelData) {
			GameObject newLevel = Instantiate(levelItem) as GameObject;
			SampleLevel level = newLevel.GetComponent<SampleLevel>();
			
			level.name = item.levelName;
			level.background.sprite = Resources.Load <Sprite> ("Level" + item.levelNumber + "_Selection");
			level.levelName.text = item.levelName;
			if(item.storyData[0].distanceMax > 0)
				level.progress.value = item.storyData[0].distanceRecord / (float)item.storyData[0].distanceMax;
			else
				level.progress.value = 0;
			level.distance.text = item.storyData[0].distanceRecord.ToString();
			level.deadBoss.SetActive(item.storyData[0].isBossDead);
			level.sceneNumber = item.levelNumber + (GameData.gameData.firstLevel - 1); // Correction du level par rapport à la scène du premier level

			level.transform.SetParent (contentPanel, false);
		}
	}

	public void StartDragging() {
		dragging = true;
	}
	public void EndDragging() {
		dragging = false;
	}

	// Remet en place le level qui a les boutons actifs si l'on bouge
	public void ResetLevel() {
		for (int i = 0; i < levelButton.Length; i++) {
			if (!levelButton[i].GetComponent<SampleLevel>().progress.gameObject.activeInHierarchy) {
				levelButton[i].GetComponent<SampleLevel>().SwitchMode(false);
			}
		}
	}

	// Positionner le level de son choix au milieu
	public void PlacerLevel(int level, bool resetLerping = false) {
		// Afin de remettre le timer à 0 si on appelle depuis ailleurs
		if (resetLerping)
			currentLerpTime = 0;

		// On calcule la nouvelle position : index du bouton * hauteur - taille totale des boutons / 2
		float decallage = level * buttonDistance - (levelButton.Length-1) * buttonDistance / 2;
		float newY = Mathf.Lerp (contentPanel.anchoredPosition.y, decallage, currentLerpTime / lerpTime);
		
		contentPanel.anchoredPosition = new Vector2 (contentPanel.anchoredPosition.x, newY);
	}

	public void MonterLevel() {
		if (minButtonNum == 0)
			return;

		ResetLevel ();

		minButtonNum--;
		PlacerLevel (minButtonNum, true);
	}

	public void DescendreLevel() {
		if (minButtonNum == levelButton.Length - 1)
			return;

		ResetLevel ();

		minButtonNum++;
		PlacerLevel (minButtonNum, true);
	}
	
	private void RafraichirImageLevel() {
		if (buttonDistance <= 0)
			return;
		
		// On rend inactifs les boutons qui sont loin du centre
		for (int i = 0; i < levelButton.Length; i++) {
			
			if (distance[i] > buttonDistance * 1.6f)
				levelButton[i].interactable = false;
			else
				levelButton[i].interactable = true;
		}

		// On cache les boutons pour aller vers le haut ou le bas
		if (minButtonNum == levelButton.Length-1)
			upLevel.interactable = false;
		else
			upLevel.interactable = true;
		
		if(minButtonNum == 0)
			downLevel.interactable = false;
		else
			downLevel.interactable = true;

		// On joue un son quand le level actuellement sélectionné est différent du précédent (quand ça bouge en gros)
		if (recordMinButton != minButtonNum && !sfxSound.IsPlaying()) {
			recordMinButton = minButtonNum;
			sfxSound.ChangeLevel();
		}
	}
}
