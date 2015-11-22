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

	public AudioClip deathSfx;
	public float deathVolume;

	public AudioClip fallDeathSfx;
	public float fallDeathVolume;

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

	private void SplashSound() {
		PlaySound (splashSfx, splashVolume);
	}

	public void DeathSound() {
		PlaySound (deathSfx, deathVolume);
	}

	public void FallDeathSound() {
		PlaySound (fallDeathSfx, fallDeathVolume);
	}

	private void PlaySound(AudioClip sound, float volume) {
		//soundSource.Stop ();
		soundSource.clip = sound;
		soundSource.volume = volume;
		soundSource.Play ();
	}
}
