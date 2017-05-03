using System.Collections;
using UnityEngine;

public class LightCaustic : MonoBehaviour {

	private Light myLight;
	private Transform myTransform;

	private float moveX;
	private float moveY;

	void Awake () {
		myLight = GetComponent<Light> ();
		myTransform = transform;
	}

	void Update () {
		myLight.cookieSize = 10.0f + 0.25f * Mathf.Sin (TimeManager.time);

		moveX = TimeManager.time + 0.1f * Mathf.Sin (TimeManager.time);
		moveY = 0.1f * Mathf.Sin (TimeManager.time);
		myTransform.position = new Vector3 (moveX, moveY, myTransform.position.z);
	}
}
