using UnityEngine;
using System.Collections;

public class BackgroundSun : BackgroundAmbient {

	protected override Vector2 StartStoryPosition () {
		return new Vector2 (0.2f, 0.25f);
	}

	protected override Vector2 MovingPosition (float ratioDistance) {
		offsetX = _StaticFunction.MappingScale (ratioDistance, 0, 1, 0.85f, 0.15f);
		offsetY = Mathf.PI * (1 + ratioDistance);

		return new Vector2 (offsetX, 0.6f + 0.45f * Mathf.Sin(offsetY));
	}
}
