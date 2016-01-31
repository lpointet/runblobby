using UnityEngine;
using System.Collections;

public class Oeuf : MonoBehaviour {

	private Animator myAnim;
	private bool broken;

	public bool IsBroken() {
		return broken;
	}

	void Awake () {
		myAnim = GetComponent<Animator> ();
	}

	void OnEnable () {
		broken = false;
		myAnim.SetBool ("broken", broken);
	}

	void OnTriggerEnter2D (Collider2D other) {
		if (other.name == "Heros" || other.CompareTag("Bullet")) {
			if (!broken) {
				broken = true;
				myAnim.SetBool ("broken", broken);
			}
		}
	}
}
