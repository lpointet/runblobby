﻿using UnityEngine;

public class DestroyFinishedParticles : MonoBehaviour {

	private ParticleSystem thisParticles;
	
	void Awake () {
		thisParticles = GetComponent<ParticleSystem> ();
	}

	void Update () {
		if (thisParticles.IsAlive())
			return;
		gameObject.SetActive (false);
	}
}
