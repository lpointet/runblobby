using UnityEngine;
using System.Collections;

public class InviciblePickup : Pickup {

	public AudioClip soundActif;
	public float soundActifVolume = 0.4f;
	public AudioClip soundEnd;
	public float soundEndVolume = 0.8f;

	protected override void Awake() {
		base.Awake();

		parentAttach = true;
		despawnTime = 0.3f;
	}
	
	protected override void OnPick() {
		base.OnPick();

		LevelManager.GetPlayer().SetInvincible( lifeTime );
	}

	protected override void PickEffect() {
		base.PickEffect();

		StartCoroutine (PlayActifSound (soundSource.clip.length));
		Debug.Log (soundSource.clip.length);
	}
	
	protected override void DespawnEffect() {
		base.DespawnEffect();

		soundSource.clip = soundEnd;
		soundSource.volume = soundEndVolume;
		soundSource.loop = false;
		soundSource.Play ();
	}

	private IEnumerator PlayActifSound(float delay) {
		yield return new WaitForSeconds(delay);

		soundSource.clip = soundActif;
		soundSource.volume = soundActifVolume;
		soundSource.loop = true;
		soundSource.Play ();
	}
}
