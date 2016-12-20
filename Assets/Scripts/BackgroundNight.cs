﻿using UnityEngine;
using System.Collections;

public class BackgroundNight : MonoBehaviour {

	private Material myMaterial;

	private float offsetY = 0;
	private float offsetX = 0;

	public Color dawnColor;
	public Color middayColor;
	public Color duskColor;

	private float partLevel = 1f; // Distance d'une "numberPart"ième du level
	private int numberPart = 20;

	void Start () {
		myMaterial = GetComponent<Renderer> ().material;

		// Adapater l'échelle à la taille de l'écran pour qu'on voit tout
		float scaleX = Camera.main.orthographicSize * Camera.main.aspect * 2 / GetComponent<MeshRenderer> ().bounds.size.x;
		float scaleY = Camera.main.orthographicSize * 2 / GetComponent<MeshRenderer> ().bounds.size.y;
		transform.localScale = new Vector3 (scaleX, scaleY, transform.localScale.z);

		if (!LevelManager.levelManager.IsStory ())
			myMaterial.mainTextureOffset = Vector2.zero;
		else
			partLevel = LevelManager.levelManager.listPhase [LevelManager.levelManager.listPhase.Length - 1] / numberPart;
	}

	void Update () {
		if (!LevelManager.levelManager.IsStory ())
			return;
		
		if (!TimeManager.paused && !LevelManager.player.IsDead () && !LevelManager.IsEndingScene()) {
			// Position du "ciel nocturne"
			// Distance parcourue / Distance avant le dernier boss
			float ratioDistance = LevelManager.levelManager.GetDistanceTraveled () / (float)LevelManager.levelManager.listPhase [LevelManager.levelManager.listPhase.Length - 1];
			offsetX = 0;
			offsetY = _StaticFunction.MappingScale (ratioDistance, 0, 1, 0, 0.5f);

			Vector2 offset = new Vector2 (offsetX, offsetY);
			myMaterial.mainTextureOffset = offset;

			// Lumière d'ambiance
			if (LevelManager.levelManager.GetDistanceTraveled () < partLevel) {
				float lerpColor = LevelManager.levelManager.GetDistanceTraveled () / (float)partLevel;
				RenderSettings.ambientLight = Color.Lerp (dawnColor, middayColor, lerpColor);
				myMaterial.color = Color.Lerp (dawnColor, middayColor, lerpColor);
			} else if (LevelManager.levelManager.GetDistanceTraveled () > (numberPart - 1) * partLevel) {
				float lerpColor = LevelManager.levelManager.GetDistanceTraveled () / (float)partLevel - (numberPart - 1);
				RenderSettings.ambientLight = Color.Lerp (middayColor, duskColor, lerpColor);
				myMaterial.color = Color.Lerp (middayColor, duskColor, lerpColor);
			}
		}
	}
}
