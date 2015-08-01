using UnityEngine;
using System.Collections;

public class DestroyFinishedParticles : MonoBehaviour {

	private ParticleSystem thisParticles;
	
	void Awake () {
		thisParticles = GetComponent<ParticleSystem> ();
	}

	void Update () {
		if (thisParticles.isPlaying)
			return;
		Destroy (gameObject);
	}
}
