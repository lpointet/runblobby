using UnityEngine;
using System.Collections;

public class BackgroundCaustic : BackgroundAmbient {

	[SerializeField] private Color causticColor;
	[SerializeField] private Color emissionColor;

	protected override void Start () {
		base.Start ();

		// Albedo
		myMaterial.SetColor ("_MainTex", causticColor);
		// Emission
		myMaterial.SetColor ("_EmissionColor", emissionColor);
	}

	protected override Vector2 MovingPosition (float ratioDistance) {
		// Tiling pour background
		tilingX = 1.0f + 0.15f * Mathf.Sin (0.9f * TimeManager.time);
		tilingY = 1.0f + 0.1f * Mathf.Sin (1.0f * TimeManager.time);
		myMaterial.mainTextureScale = new Vector2 (tilingX, tilingY);

		offsetX = 0.15f * (TimeManager.time + Mathf.Sin (0.9f * TimeManager.time));
		offsetY = 0.1f * (TimeManager.time +  Mathf.Sin (1.0f * TimeManager.time));

		return new Vector2 (offsetX, offsetY);
	}

	void LateUpdate () {
		// Albedo
		myMaterial.SetColor ("_Color", causticColor);
	}
}
