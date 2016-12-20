using UnityEngine;
using System.Collections;

public class Sting : MonoBehaviour {

	private Rigidbody2D myRb;

	void Awake () {
		myRb = GetComponent<Rigidbody2D> ();
	}

	void OnBecameInvisible () {
		gameObject.SetActive (false);
	}

	public void SetCourse (float speed, Vector2 direction) {
		float rotaZ; // Angle entre le dard et le boss

		rotaZ = Mathf.Atan2 (direction.y, direction.x); // Angle entre l'horizontal et le vecteur calculé précédemment
		transform.rotation = Quaternion.Euler (0f, 0f, rotaZ * Mathf.Rad2Deg - 90);

		myRb.velocity = new Vector2 (Mathf.Cos (rotaZ), Mathf.Sin (rotaZ)) * speed;
	}
}
