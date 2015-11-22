using UnityEngine;
using System.Collections;

[RequireComponent (typeof (AudioSource))]
public class ObjectSFX : MonoBehaviour {

	private AudioSource sound;
	private float soundVolume;

	public bool clearPlay;
	public float delayPlay;
	private bool goPlay = false;

	public bool clearStop;
	public float delayStop;
	private bool goStop = false;
	
	void Awake () {
		sound = GetComponent<AudioSource> ();
		soundVolume = sound.volume;
	}

	void OnEnable() {
		goPlay = false;
		goStop = false;
	}

	void Update () {
		if (goPlay)
			_StaticFunction.AudioFadeIn (sound, soundVolume, delayPlay);

		if (goStop)
			_StaticFunction.AudioFadeOut (sound, 0, delayStop);
	}

	// On démarre le son
	void OnBecameVisible() {
		if (clearPlay)
			sound.Play ();
		else {
			goPlay = true;
			goStop = false;
		}
	}

	// On arrete le son
	void OnBecameInvisible() {
		if (clearPlay)
			sound.Stop ();
		else {
			goPlay = false;
			goStop = true;
		}
	}
}
