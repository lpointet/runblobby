using UnityEngine;

public class PlayerSoundEffect : MonoBehaviour {

	private AudioSource soundSource;
	private PlayerController player;

	public AudioClip jumpSfx;
	public float jumpVolume;


	public AudioClip moveSfx;
	public float moveVolume;
	private float timeToMove;

	public AudioClip splashSfx;
	public float splashVolume;
	private bool wasGrounded = true;

	void Awake() {
		soundSource = GetComponent<AudioSource> ();
		player = LevelManager.GetPlayer ();
	}

	void Start() {
		timeToMove = 0f;
	}

	void Update() {
		if (!player.IsDead ()) {
			// Saut appelé par le PlayerController

			// Ecrasement
			if (!wasGrounded && player.IsGrounded()) {
				SplashSound();
			}

			// Déplacement
			else if (!soundSource.isPlaying && player.IsGrounded () && Time.time > timeToMove) {
				FootStepSound();

				timeToMove = Time.time + 4 / 7f;
			}
		}
		wasGrounded = player.IsGrounded (); // Connaitre l'état de la précédente frame
	}

	private void FootStepSound() {
		PlaySound (moveSfx, moveVolume);
	}
	
	public void JumpSound() {
		PlaySound (jumpSfx, jumpVolume);
	}

	public void SplashSound() {
		PlaySound (splashSfx, splashVolume);
	}

	private void PlaySound(AudioClip sound, float volume) {
		//soundSource.Stop ();
		soundSource.clip = sound;
		soundSource.volume = volume;
		soundSource.Play ();
	}
}
