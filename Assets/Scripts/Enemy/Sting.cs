using UnityEngine;
using System.Collections;

public class Sting : MonoBehaviour {

	private Rigidbody2D myRb;
	private Transform myTransform;

	private float moveSpeed = 3f;

	void Awake () {
		myRb = GetComponent<Rigidbody2D> ();
		myTransform = transform;
	}

	void OnEnable () {
		// Vitesse en fonction de l'angle
		float rotaZ = (myTransform.eulerAngles.z - 90) * Mathf.Deg2Rad; // -90 pour compenser l'angle du Sprite
		myRb.velocity = new Vector2 (-Mathf.Cos (rotaZ), -Mathf.Sin (rotaZ)) * moveSpeed;
	}

	void OnBecameInvisible () {
		gameObject.SetActive (false);
	}
}
