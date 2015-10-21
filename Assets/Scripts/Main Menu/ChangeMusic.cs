using UnityEngine;
using System.Collections;

public class ChangeMusic : MonoBehaviour {

	public AudioClip level1Music;

	private AudioSource source;
	
	void Awake () {
		source = GetComponent<AudioSource> ();
	}

	void OnLevelWasLoaded(int level) {
		switch (level) {
		case 1:
			source.clip = level1Music;
			break;
		}

		source.Play ();
	}
}
