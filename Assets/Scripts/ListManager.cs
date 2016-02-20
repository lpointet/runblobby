﻿using UnityEngine;
using System.Collections;

public class ListManager : MonoBehaviour {

	public static ListManager current;

	public CoinPickup[] coins;
	public Pickup[] powerups;
	public Sprite[] cloudBlock;

	void Awake() {
		if (current == null) {
			current = this;
		} else
			DestroyObject (this);
	}
}
