using UnityEngine;
using System.Collections;

public class BackgroundAmbient : MonoBehaviour {

	protected Material myMaterial;

	protected float offsetY = 0;
	protected float offsetX = 0;
	protected float tilingY = 0;
	protected float tilingX = 0;

	public Color dawnColor;
	public Color middayColor;
	public Color duskColor;

	protected float partLevel = 1f; // Distance d'une "numberPart"ième du level
	protected int numberPart = 20;

	protected virtual void Start () {
		myMaterial = GetComponent<Renderer> ().material;

		// Adapater l'échelle à la taille de l'écran pour qu'on voit tout
		float scaleX = Camera.main.orthographicSize * Camera.main.aspect * 2 / GetComponent<MeshRenderer> ().bounds.size.x;
		float scaleY = Camera.main.orthographicSize * 2 / GetComponent<MeshRenderer> ().bounds.size.y;
		transform.localScale = new Vector3 (scaleX, scaleY, transform.localScale.z);

		if (!LevelManager.levelManager.IsStory ())
			myMaterial.mainTextureOffset = StartStoryPosition ();
		else
			partLevel = LevelManager.levelManager.listPhase [LevelManager.levelManager.listPhase.Length - 1] / numberPart;
	}

	protected virtual Vector2 StartStoryPosition () {
		// Position au départ
		return Vector2.zero;
	}

	protected virtual Vector2 MovingPosition (float ratioDistance) {
		// Position pendant le jeu - Calcul de offsetY et offsetX
		return Vector2.zero;
	}

	void Update () {
		if (!LevelManager.levelManager.IsStory ())
			return;

		if (!TimeManager.paused && !LevelManager.player.IsDead () && !LevelManager.IsEndingScene()) {
			// Position du background
			// Distance parcourue / Distance avant le dernier boss
			float ratioDistance = LevelManager.levelManager.GetDistanceTraveled () / (float)LevelManager.levelManager.listPhase [LevelManager.levelManager.listPhase.Length - 1];

			myMaterial.mainTextureOffset = MovingPosition (ratioDistance);

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
