﻿using UnityEngine;
using System.Collections;

[RequireComponent (typeof (AudioSource))]
public class SFXBackground : MonoBehaviour {

	private AudioSource myAudio;
	private PlayerController player;

	private int randSound;
	private float timeToNextSound = 0f;

	[Header("Birds")]
	public int minIntervalleBird = 7;
	public int maxIntervalleBird = 15;
	public float minVolumeBird = 0.3f;
	public float maxVolumeBird = 1f;
	public AudioClip[] birdBank;
	private float timeToSing = 5f;

	[Header("Cricket")]
	public int minIntervalleCricket = 5;
	public int maxIntervalleCricket = 8;
	public float minVolumeCricket = 0.4f;
	public float maxVolumeCricket = 0.7f;
	public AudioClip[] cricketBank;
	private float timeToCrick = 2f;

	[Header("Cow/Goat")]
	public int minIntervalleCow = 30;
	public int maxIntervalleCow = 60;
	public float minVolumeCow = 0.5f;
	public float maxVolumeCow = 0.7f;
	public AudioClip[] cowBank;
	private float timeToMoo = 45f;
	
	void Awake () {
		myAudio = GetComponent<AudioSource> ();
		player = LevelManager.GetPlayer ();
	}

	void Start () {
		timeToSing = Time.unscaledTime + Random.Range (minIntervalleBird, maxIntervalleBird);
		timeToCrick = Time.unscaledTime + Random.Range (minIntervalleCricket, maxIntervalleCricket);
		timeToMoo = Time.unscaledTime + Random.Range (minIntervalleCow, maxIntervalleCow);
	}

	void Update () {
		if (player.IsDead ())
			myAudio.Stop ();

		if (!myAudio.isPlaying && !player.IsDead () && Time.unscaledTime > timeToNextSound) {

			// Birds
			if (Time.unscaledTime > timeToSing && birdBank.Length > 0) {
				randSound = Random.Range (0, birdBank.Length);

				PlaySound (birdBank [randSound], Random.Range (minVolumeBird, maxVolumeBird));

				timeToSing = Time.unscaledTime + Random.Range (minIntervalleBird, maxIntervalleBird);
			}

			// Crickets
			else if (Time.unscaledTime > timeToCrick && cricketBank.Length > 0) {
				randSound = Random.Range (0, cricketBank.Length);
				
				PlaySound (cricketBank [randSound], Random.Range (minVolumeCricket, maxVolumeCricket));
				
				timeToCrick = Time.unscaledTime + Random.Range (minIntervalleCricket, maxIntervalleCricket);
			}

			// Cows/Goats
			else if (Time.unscaledTime > timeToMoo && cowBank.Length > 0) {
				randSound = Random.Range (0, cowBank.Length);
				
				PlaySound (cowBank [randSound], Random.Range (minVolumeCow, maxVolumeCow));
				
				timeToMoo = Time.unscaledTime + Random.Range (minIntervalleCow, maxIntervalleCow);
			}
		}
	}

	private void PlaySound (AudioClip sound, float volume) {
		myAudio.clip = sound;
		myAudio.volume = volume;
		myAudio.Play ();
		timeToNextSound = Time.unscaledTime + myAudio.clip.length + 0.5f;
	}
}
