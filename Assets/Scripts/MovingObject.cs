using UnityEngine;

public class MovingObject : MonoBehaviour {

	private Transform myTransform;

	void Awake() {
		myTransform = transform;
	}

	void Update () {
		myTransform.position += Vector3.left * LevelManager.levelManager.GetLocalDistance ();
	}
}
