using UnityEngine;

public class PlayerSoundEffect : MonoBehaviour {

	private AudioSource soundSource;

	public AudioClip jumpSfx;
	public float jumpVolume;
	public AudioClip moveSfx;
	public float moveVolume;
	public AudioClip splashSfx;
	public float splashVolume;

	void Awake() {
		soundSource = GetComponent<AudioSource> ();
	}

	void Start() {
		NormalizeSound (ref jumpVolume);
		NormalizeSound (ref moveVolume);
		NormalizeSound (ref splashVolume);
	}
	
	private void FootStepSound() {
		PlaySound (moveSfx, moveVolume);
	}
	
	public void JumpSound() {
		PlaySound (jumpSfx, jumpVolume);
	}

	public void SplashSound() {
		//PlaySound (splashSfx, splashVolume);
	}

	private void PlaySound(AudioClip sound, float volume) {
		soundSource.Stop ();
		soundSource.PlayOneShot (sound, volume);
	}

	private void NormalizeSound(ref float volume) {
		if (volume > 1)
			volume = 1;
		if (volume < 0)
			volume = 0;
	}
}
