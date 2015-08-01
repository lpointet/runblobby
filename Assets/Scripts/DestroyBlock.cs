using UnityEngine;
using System.Collections;

public class DestroyBlock : MonoBehaviour {

	void OnBecameInvisible() {
		Invoke ("Destroy", 5f);
	}

	void Destroy() {
		gameObject.SetActive (false);
	}

	void OnDisable() {
		CancelInvoke ();
	}
}