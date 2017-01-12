using UnityEngine;

public class DisableObject : MonoBehaviour {

	private float initialDelay;
	[SerializeField] private float delayBeforeDisable = 0f;
	[SerializeField] private bool returnToPool = false;

	void Awake () {
		initialDelay = delayBeforeDisable;
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
		if (returnToPool)
			transform.SetParent (PoolingManager.pooledObjectParent, false);
		
		gameObject.SetActive (false);
	}
}
