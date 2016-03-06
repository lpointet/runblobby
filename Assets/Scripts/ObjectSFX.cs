using UnityEngine;
using System.Collections;

[RequireComponent (typeof (AudioSource))]
public class ObjectSFX : MonoBehaviour {

	private AudioSource sound;
	private float soundVolume;

	public bool clearPlay;
	public float delayPlay;

	public bool clearStop;
	public float delayStop;
	
	void Awake () {
		sound = GetComponent<AudioSource> ();
		soundVolume = sound.volume;
	}

	// On démarre le son
	void OnBecameVisible() {
		if (clearPlay)
			sound.Play ();
		else {
			StartCoroutine(_StaticFunction.AudioFadeIn (sound, soundVolume, delayPlay));
		}
	}

	// On arrete le son
	void OnBecameInvisible() {
		if (clearPlay || !gameObject.activeInHierarchy)
			sound.Stop ();
		else {
			StartCoroutine(_StaticFunction.AudioFadeOut (sound, 0, delayStop));
		}
	}
}
