using UnityEngine;
using System.Collections;

public class Oeuf : MonoBehaviour {

	private Animator myAnim;
	private Rigidbody2D myRb;

	private bool broken;

	public bool IsBroken() {
		return broken;
	}

	void Awake () {
		myAnim = GetComponent<Animator> ();
		myRb = GetComponent<Rigidbody2D> ();
	}

	void OnEnable () {
		broken = false;
		myAnim.SetBool ("broken", broken);
	}

	void OnTriggerEnter2D (Collider2D other) {
		if (other.name == "Heros" || other.CompareTag("Bullet")) {
			if (!IsBroken()) {
				broken = true;
				myAnim.SetBool ("broken", broken);
			}
		}
	}
}
