using UnityEngine;
using System.Collections;

public class BackgroundNight : BackgroundAmbient {

	protected override Vector2 MovingPosition (float ratioDistance) {
		offsetX = 0;
		offsetY = _StaticFunction.MappingScale (ratioDistance, 0, 1, 0, 0.5f);

		return new Vector2 (offsetX, offsetY);
	}
}
