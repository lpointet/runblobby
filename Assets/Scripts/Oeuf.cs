using UnityEngine;

public class Oeuf : MonoBehaviour {

	private Animator myAnim;
	private AudioSource myAudio;

	private bool broken;
	private float invincibleDelay = 0.2f;
	private float timeBeforeBreakable;

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
		timeBeforeBreakable = TimeManager.time;
	}

	void OnTriggerEnter2D (Collider2D other) {
		// On ne casse pas l'oeuf trop vite...
		if (TimeManager.time < timeBeforeBreakable + invincibleDelay)
			return;
		
		if (other.name == "Heros" || other.CompareTag("Bullet")) {
			if (!IsBroken()) {
				broken = true;
				myAudio.Play ();
				myAnim.SetBool ("broken", broken);
			}
		}
	}
}
