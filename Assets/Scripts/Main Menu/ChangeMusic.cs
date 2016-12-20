using UnityEngine;
using UnityEngine.SceneManagement;
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

	void OnEnable () {
		SceneManager.sceneLoaded += OnLevelIsLoaded;
	}

	void OnDisable () {
		SceneManager.sceneLoaded -= OnLevelIsLoaded;
	}

	private void OnLevelIsLoaded (Scene scene, LoadSceneMode mode) {
		// TODO appeler ici ce qu'il y a dans OnLevelWasLoaded, mais je ne suis pas sur que cette fonction soit encore utilisée
	}
}
