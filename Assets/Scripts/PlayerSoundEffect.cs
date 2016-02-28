﻿using UnityEngine;

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

	public AudioClip airDeathSfx;
	public float airDeathVolume;

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
		if (Time.timeScale > 0 && !player.IsDead ()) {
			// Saut appelé par le PlayerController

			// Ecrasement
			if (!wasGrounded && player.IsGrounded() && !player.IsFlying ()) {
				SplashSound();
			}

			// Déplacement
			else if (!soundSource.isPlaying && player.IsGrounded () && !player.IsFlying () && Time.unscaledTime > timeToMove) {
				FootStepSound();

				timeToMove = Time.unscaledTime + (player.GetInitialMoveSpeed () / player.GetMoveSpeed()) * (4 / 7f); // Infinity si MoveSpeed = 0
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

	public void AirDeathSound() {
		PlaySound (airDeathSfx, airDeathVolume);
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
