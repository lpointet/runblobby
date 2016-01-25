using UnityEngine;
using System.Collections;

public class Oeuf : MonoBehaviour {

	private Transform myTransform;
	private float varySize = 0;
	private bool isGrowing = true;
	private float delayGrowing = 0.15f;

	private Animator myAnim;
	private bool broken;

	public bool IsBroken() {
		return broken;
	}

	void Awake () {
		myAnim = GetComponent<Animator> ();
		myTransform = transform;
	}

	void OnEnable () {
		varySize = 0;
		isGrowing = true;
		transform.localScale = new Vector2 (varySize, varySize);

		broken = false;
		myAnim.SetBool ("broken", broken);
	}

	void Update () {
		if (isGrowing && varySize < 1.5f) {
			varySize += Time.deltaTime / delayGrowing;
		} else if (varySize > 1f) {
			isGrowing = false;
			varySize -= Time.deltaTime / delayGrowing;
			if (varySize < 1f)
				varySize = 1f;
		}

		myTransform.localScale = new Vector2 (varySize, varySize);
	}

	void OnTriggerEnter2D (Collider2D other) {
		if (other.name == "Heros" || other.CompareTag("Bullet")) {
			if (!broken)
				broken = true;
				myAnim.SetBool ("broken", broken);
		}
	}
}
