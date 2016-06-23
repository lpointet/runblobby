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

	public AudioClip hurtSfx;
	public float hurtVolume;
	private bool hurted = false;

	public AudioClip deathSfx;
	public float deathVolume;

	public AudioClip airDeathSfx;
	public float airDeathVolume;

	public AudioClip fallDeathSfx;
	public float fallDeathVolume;

	void Awake() {
		soundSource = GetComponent<AudioSource> ();
		player = LevelManager.player;
	}

	void Start() {
		timeToMove = 0f;
	}

	void Update() {
		if (!TimeManager.paused && !player.IsDead ()) {
			// Saut appelé par le PlayerController

			// Ecrasement
			if (!wasGrounded && player.IsGrounded() && !player.IsFlying ()) {
				SplashSound();
			}

			// Déplacement
			else if (!soundSource.isPlaying && player.IsGrounded () && !player.IsFlying () && TimeManager.time > timeToMove) {
				FootStepSound();

				timeToMove = TimeManager.time + (player.initialMoveSpeed / player.moveSpeed) * (4 / 7f); // Infinity si MoveSpeed = 0
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

	public void HurtSound() {
		PlaySound (hurtSfx, hurtVolume);
		hurted = true;
		Invoke ("EndHurtingSound", hurtSfx.length * Time.timeScale);
	}

	public void DeathSound() {
		PlaySound (deathSfx, deathVolume);
	}

	public void AirDeathSound() {
		PlaySound (airDeathSfx, airDeathVolume);
	}

	public void FallDeathSound() {
		PlaySound (fallDeathSfx, fallDeathVolume);
	}

	private void PlaySound(AudioClip sound, float volume) {
		//soundSource.Stop ();
		if (hurted)
			return;
		
		soundSource.clip = sound;
		soundSource.volume = volume;
		soundSource.Play ();
	}

	private void EndHurtingSound () {
		hurted = false;
	}
}
