using UnityEngine;
using System.Collections;

public class BlockManager : MonoBehaviour {

	void OnEnable() {
		foreach (Renderer rend in GetComponentsInChildren<Renderer>()) {
			rend.enabled = true;
		}
	}
}
