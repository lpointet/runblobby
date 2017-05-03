using UnityEngine;

public class EnemySoundEffect : MonoBehaviour {

	private AudioSource soundSource;
	private Enemy enemy;

	public AudioClip jumpSfx;
	public float jumpVolume;

	public AudioClip moveSfx;
	public float moveVolume;
	public float delayMove;
	private float timeToMove;

	public AudioClip hitSfx;
	public float hitVolume;

	public AudioClip deathSfx;
	public float deathVolume;

	void Awake() {
		soundSource = GetComponent<AudioSource> ();
		enemy = GetComponent<Enemy> ();
	}

	void Start() {
		timeToMove = 0f;
	}

	void Update() {
		if (!TimeManager.paused && enemy != null && !enemy.IsDead ()) {
			if (!soundSource.isPlaying  && TimeManager.time > timeToMove) {
				FootStepSound();

				timeToMove = TimeManager.time + delayMove;
			}
		}
	}

	private void FootStepSound() {
		PlaySound (moveSfx, moveVolume);
	}

	private void JumpSound() {
		PlaySound (jumpSfx, jumpVolume);
	}

	public void DeathSound() {
		PlaySound (deathSfx, deathVolume);
	}

	public void HitSound() {
		PlaySound (hitSfx, hitVolume);
	}

	private void PlaySound(AudioClip sound, float volume) {
		//soundSource.Stop ();
		soundSource.pitch = 1.0f + Random.Range (-0.25f, 0.35f);
		soundSource.clip = sound;
		soundSource.volume = volume;
		soundSource.Play ();
	}
}
