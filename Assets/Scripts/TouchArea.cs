using System.Collections;
using UnityEngine;

public class TouchArea : MonoBehaviour {

	private CircleCollider2D myCollider;
	private SpriteRenderer mySprite;

	void Awake () {
		mySprite = GetComponent<SpriteRenderer> ();
		myCollider = GetComponent<CircleCollider2D> ();
	}

	void OnEnable () {
		mySprite.enabled = true;
		myCollider.enabled = true;
	}

	void Start () {
		Mediator.current.Subscribe<TouchClickable> (IsTouched);
	}

	private void IsTouched (TouchClickable touch) {
		if (touch.objectId == this.gameObject.GetInstanceID ()) {
			Disable ();

			SendMessageUpwards("TouchDetected");
		}
	}

	private void Disable () {
		mySprite.enabled = false;
		myCollider.enabled = false;
	}
}
