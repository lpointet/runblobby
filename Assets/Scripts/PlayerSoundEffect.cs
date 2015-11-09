using UnityEngine;

public class PlayerSoundEffect : MonoBehaviour {

	private AudioSource soundSource;
	public AudioClip jumpSfx;
	public AudioClip moveSfx;

	void Awake() {
		soundSource = GetComponent<AudioSource> ();
	}
	
	private void FootStepSound() {
		soundSource.Stop ();
		soundSource.volume = 0.2f;
		soundSource.PlayOneShot (moveSfx);
	}
	
	public void JumpSound() {
		soundSource.Stop ();
		soundSource.volume = 0.4f;
		soundSource.PlayOneShot (jumpSfx);
	}
}
