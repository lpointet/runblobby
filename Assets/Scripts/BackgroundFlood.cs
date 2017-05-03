using UnityEngine;
using System.Collections;

public class BackgroundFlood : BackgroundAmbient {

	protected override Vector2 MovingPosition (float ratioDistance) {
		offsetX += TimeManager.deltaTime * 0.1f;
		offsetY = _StaticFunction.MappingScale (ratioDistance, 0, 1, 0.25f, -0.25f);

		return new Vector2 (offsetX, offsetY);
	}
}
