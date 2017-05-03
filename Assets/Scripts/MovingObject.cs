using UnityEngine;

public class MovingObject : MonoBehaviour {

	private Transform myTransform;
	[Range(0.0f, 2.0f)] [SerializeField] private float ratioPlayerSpeed = 1.0f;

	void Awake() {
		myTransform = transform;
	}

	void Update () {
		myTransform.position += Vector3.left * LevelManager.levelManager.GetLocalDistance () * ratioPlayerSpeed;
	}
}
