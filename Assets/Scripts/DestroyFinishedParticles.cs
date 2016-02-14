using UnityEngine;

public class DestroyFinishedParticles : MonoBehaviour {

	private ParticleSystem thisParticles;
	
	void Awake () {
		thisParticles = GetComponent<ParticleSystem> ();
	}

	void Update () {
		if (thisParticles.IsAlive()) // TODO bug dans la version 5.3.1, attente de la correction
			return;
		gameObject.SetActive (false);
	}
}
