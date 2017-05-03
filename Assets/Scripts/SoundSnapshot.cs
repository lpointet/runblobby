using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class SoundSnapshot : MonoBehaviour {

	[SerializeField] private AudioMixerSnapshot musicSnap;
	[SerializeField] private AudioMixerSnapshot sfxSnap;

	void Start () {
		musicSnap.TransitionTo (0.0f);
		sfxSnap.TransitionTo (0.0f);
	}
}
