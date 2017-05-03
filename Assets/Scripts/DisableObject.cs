using UnityEngine;

public class DisableObject : MonoBehaviour {

	private Sprite firstSprite;
	private SpriteRenderer mySprite;
	private Animator myAnim;

	private float initialDelay;
	[SerializeField] private float delayBeforeDisable = 0f;
	[SerializeField] private bool returnToPool = false;

	void Awake () {
		initialDelay = delayBeforeDisable;

		// Permet de remettre à 0 un sprite s'il y a une animation
		mySprite = GetComponent<SpriteRenderer> ();
		myAnim = GetComponent<Animator> ();
		if (mySprite != null)
			firstSprite = mySprite.sprite;
	}

	void OnEnable () {
		delayBeforeDisable = initialDelay;
	}

	void Update () {
		if (delayBeforeDisable <= 0)
			Disable ();

		delayBeforeDisable -= TimeManager.deltaTime;
	}

	private void Disable() {
		if (mySprite != null)
			mySprite.sprite = firstSprite;
		if (myAnim != null)
			myAnim.Play ("", 0, 0f);
		
		if (returnToPool)
			transform.SetParent (PoolingManager.pooledObjectParent, false);
		
		gameObject.SetActive (false);
	}
}
