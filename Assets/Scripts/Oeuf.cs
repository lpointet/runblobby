using UnityEngine;

public class Oeuf : MonoBehaviour {

	private Animator myAnim;
	private AudioSource myAudio;

	private bool broken;

	public bool IsBroken() {
		return broken;
	}

	void Awake () {
		myAnim = GetComponent<Animator> ();
		myAudio = GetComponent<AudioSource> ();
	}

	void OnEnable () {
		broken = false;
		myAnim.SetBool ("broken", broken);
	}

	void OnTriggerEnter2D (Collider2D other) {
		if (other.name == "Heros" || other.CompareTag("Bullet")) {
			if (!IsBroken()) {
				broken = true;
				myAudio.Play ();
				myAnim.SetBool ("broken", broken);
			}
		}
	}
}
