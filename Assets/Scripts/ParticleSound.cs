using UnityEngine;
using System.Collections;

[RequireComponent (typeof (AudioSource))]
[RequireComponent (typeof (ParticleSystem))]
public class ParticleSound : MonoBehaviour {

	public AudioClip onBirthSound;
	public float volumeBirth = 1;
	public bool oneShotBirth = true;

	public AudioClip onDeathSound;
	public float volumeDeath = 1;
	public bool oneShotDeath = true;

	private AudioSource myAudio;
	private ParticleSystem myParticle;

	private int previousCountParticle = 0;

	void Awake () {
		myAudio = GetComponent<AudioSource> ();
		myParticle = GetComponent<ParticleSystem> ();
	}

	void Update () {
		// On ne fait rien si un son est déjà en train de jouer
		/*if (myAudio.isPlaying)
			return;*/
		
		int countParticle = myParticle.particleCount;

		if (countParticle > previousCountParticle) {
			PlaySound (onBirthSound, volumeBirth, oneShotBirth);
		} else if (countParticle < previousCountParticle) {
			PlaySound (onDeathSound, volumeDeath, oneShotDeath);
		}

		previousCountParticle = countParticle;
	}

	private void PlaySound (AudioClip sound, float volume, bool oneShot) {
		if (sound == null)
			return;
		
		myAudio.volume = volume;

		if (oneShot) {
			myAudio.clip = sound;
			myAudio.pitch = 1.0f + Random.Range (-0.25f, 0.35f);
			myAudio.Play ();
		} else
			myAudio.PlayOneShot (sound);
	}
}
