using UnityEngine;
using System.Collections;

public class ListManager : MonoBehaviour {

	public static ListManager current;

	public Sprite[] cloudBlock;
	public GameObject[] coins;

	void Awake() {
		if (current == null) {
			current = this;
		} else
			DestroyObject (this);
	}
}
