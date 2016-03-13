using UnityEngine;

public class DestroyBlock : MonoBehaviour {

	void OnBecameInvisible() {
		Invoke ("Destroy", 5f * Time.timeScale);
	}

	void Destroy() {
		gameObject.SetActive (false);
	}

	void OnDisable() {
		CancelInvoke ();
	}
}