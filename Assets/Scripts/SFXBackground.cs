using UnityEngine;
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
	
	void Awake () {
		myAudio = GetComponent<AudioSource> ();
		player = LevelManager.GetPlayer ();
	}

	void Start () {
		timeToSing = Time.time + Random.Range (minIntervalleBird, maxIntervalleBird);
		timeToCrick = Time.time + Random.Range (minIntervalleCricket, maxIntervalleCricket);
	}

	void Update () {
		if (player.IsDead ())
			myAudio.Stop ();

		if (!myAudio.isPlaying && !player.IsDead () && Time.time > timeToNextSound) {

			// Birds
			if (Time.time > timeToSing) {
				randSound = Random.Range (0, birdBank.Length);

				PlaySound (birdBank [randSound], Random.Range (minVolumeBird, maxVolumeBird));

				timeToSing = Time.time + Random.Range (minIntervalleBird, maxIntervalleBird);
			}

			// Crickets
			else if (Time.time > timeToCrick) {
				randSound = Random.Range (0, cricketBank.Length);
				
				PlaySound (cricketBank [randSound], Random.Range (minVolumeCricket, maxVolumeCricket));
				
				timeToCrick = Time.time + Random.Range (minIntervalleCricket, maxIntervalleCricket);
			}
		}
	}

	private void PlaySound (AudioClip sound, float volume) {
		myAudio.clip = sound;
		myAudio.volume = volume;
		myAudio.Play ();
		timeToNextSound = Time.time + myAudio.clip.length + 0.5f;
	}
}
