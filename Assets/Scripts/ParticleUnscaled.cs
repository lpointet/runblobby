using UnityEngine;

public class ParticleUnscaled : MonoBehaviour {

	private ParticleSystem particle;

	private bool isPlaying = false;
	private float timePlaying = 0;

	private void Awake() {
		particle = GetComponent<ParticleSystem>();
	}

	void Update () {
		if (particle.loop) {
			particle.Simulate (Time.unscaledDeltaTime, true, false);
		} else if (isPlaying) {
			timePlaying += Time.unscaledDeltaTime;
			particle.Simulate (Time.unscaledDeltaTime, true, false);

			if (timePlaying >= particle.duration) {
				isPlaying = false;
				timePlaying = 0;
				particle.Simulate (0.0f, true, true); // TODO Corrige un bug Unity : https://issuetracker.unity3d.com/issues/particle-system-plays-only-once
			}
		}
	}

	public void Play() {
		if (!particle.isPlaying) {
			isPlaying = true;
			particle.Clear ();
		}
	}
}