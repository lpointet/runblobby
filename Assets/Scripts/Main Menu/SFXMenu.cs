using UnityEngine;
using System.Collections;

public class SFXMenu : MonoBehaviour {

	public AudioClip buttonYesClick;
	public AudioClip buttonNoClick;
	public AudioClip changeLevel;

	private AudioSource sourceSfx;
	
	void Awake () {
		sourceSfx = GetComponent<AudioSource> ();
	}

	public void ButtonYesClick() {
		sourceSfx.PlayOneShot (buttonYesClick);
	}

	public void ButtonNoClick() {
		sourceSfx.PlayOneShot (buttonNoClick);
	}

	public void ChangeLevel() {
		sourceSfx.PlayOneShot (changeLevel);
	}

	public bool IsPlaying() {
		return sourceSfx.isPlaying;
	}
}
