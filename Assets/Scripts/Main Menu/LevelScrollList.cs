using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class Level {
	public Sprite background;
	public string name;
	public int distance;
	public int maxDistance;
	public bool isBoss;
	public int sceneNumber;
}

public class LevelScrollList : MonoBehaviour {
	
	public RectTransform contentPanel;

	public GameObject levelItem;
	public List<Level> levelList;
	
	private Button[] levelButton;
	public RectTransform center; // Centre du contentPanel pour comparer la distance au bouton
	
	private float[] distance; // Distance des boutons au centre
	private bool dragging = false; // True quand on bouge le panneau
	private int buttonDistance; // Distance entre les boutons
	private int minButtonNum = 0; // Bouton le plus proche du centre

	private float currentLerpTime;
	private float lerpTime = 1f; // Temps pour se "caler"
	
	void Start () {
		GetComponent<Image> ().color = new Color (0, 0, 0, 0); // On cache la liste avant peuplement (alpha = 0)

		PopulateList ();

		levelButton = contentPanel.GetComponentsInChildren<Button> ();
		distance = new float[levelButton.Length];

		StartCoroutine (WaitEndOfFrame ());
	}

	void OnEnable() {
		//PlacerLevel (0);
	}

	IEnumerator WaitEndOfFrame() {
		yield return new WaitForEndOfFrame();
		
		// Distance entre les boutons (ils sont placés à la fin de la première frame seulement)
		buttonDistance = (int)Mathf.Abs (levelButton[1].GetComponent<RectTransform> ().anchoredPosition.y - levelButton[0].GetComponent<RectTransform> ().anchoredPosition.y);

		PlacerLevel (0);

		// On remonte le alpha de 1 pour réaffiche la liste une fois peuplée
		GetComponent<Image> ().color = new Color32 (0, 0, 0, 1);
	}

	void Update() {
		// On cherche la distance minimale au centre du contenu (matérialisé par un autre point non lié au contenu)
		for (int i = 0; i < levelButton.Length; i++) {
			distance [i] = Mathf.Abs (center.transform.position.y - levelButton [i].transform.position.y);
		}
		float minDistance = Mathf.Min (distance);
		// On récupère l'index du level le plus proche du centre
		for (int j = 0; j < levelButton.Length; j++) {
			if (minDistance == distance [j]) 
				minButtonNum = j;
		}
		
		// On "glisse" vers la position centrale quand on arrete de tirer
		if (dragging)
			currentLerpTime = 0;
		else {
			// On approche de la fin du positionnement une fois par image
			currentLerpTime += Time.deltaTime;
			if(currentLerpTime > lerpTime)
				currentLerpTime = lerpTime;
			// On calcule la nouvelle position : index du bouton * hauteur - taille totale des boutons / 2
			float decallage = minButtonNum * buttonDistance - (levelButton.Length-1) * buttonDistance / 2;
			float newY = Mathf.Lerp (contentPanel.anchoredPosition.y, decallage, currentLerpTime / lerpTime);
			
			contentPanel.anchoredPosition = new Vector2 (contentPanel.anchoredPosition.x, newY);
		}

		// On rend inactifs les boutons qui sont loin du centre
		for (int i = 0; i < levelButton.Length; i++) {

			if (distance[i] > buttonDistance * 1.5f)
				levelButton[i].interactable = false;
			else
				levelButton[i].interactable = true;
		}
	}

	private void PopulateList() {
		foreach (Level item in levelList) {
			GameObject newLevel = Instantiate(levelItem) as GameObject;
			SampleLevel level = newLevel.GetComponent<SampleLevel>();
			
			level.name = item.name;
			level.background.sprite = item.background;
			level.levelName.text = item.name;
			if(item.maxDistance > 0)
				level.progress.value = item.distance / (float)item.maxDistance;
			else
				level.progress.value = 0;
			level.distance.text = item.distance.ToString();
			level.deadBoss.SetActive(item.isBoss);
			level.sceneNumber = item.sceneNumber;

			level.transform.SetParent (contentPanel);
			level.transform.localScale = Vector3.one; // Corrige un bug que je n'explique pas...
		}
	}

	// Permet de remplir la levelList
	private void GetPlayerLevel() {

	}

	public void StartDragging() {
		dragging = true;
	}
	public void EndDragging() {
		dragging = false;
	}

	// Remet en place du level qui a les boutons actifs si l'on bouge
	public void ResetLevel() {
		for (int i = 0; i < levelButton.Length; i++) {
			if (!levelButton[i].GetComponent<SampleLevel>().progress.gameObject.activeInHierarchy) {
				levelButton[i].GetComponent<SampleLevel>().SwitchMode(false);
			}
		}
	}

	// Positionner le level de son choix au milieu
	public void PlacerLevel(int level) {
		// On calcule la nouvelle position : index du bouton * hauteur - taille totale des boutons / 2
		float newY = level * buttonDistance - (levelButton.Length-1) * buttonDistance / 2;
		
		contentPanel.anchoredPosition = new Vector2 (contentPanel.anchoredPosition.x, newY);
	}
}
