using UnityEngine;
using System.Collections;

public class InviciblePickup : Pickup {

	public AudioClip soundActif;
	public float soundActifVolume = 0.4f;
	public AudioClip soundEnd;
	public float soundEndVolume = 0.8f;

	private float clignottant;
	private float coefClignottant = 0.05f;
	private float alphaSprite = 1f;

	protected override void Awake() {
		base.Awake();

		parentAttach = true;
		despawnTime = 0.3f;
		weakTime = 3f;

		clignottant = Mathf.Sqrt (Mathf.Sqrt (Mathf.PI / (2f * coefClignottant)));
	}
	
	protected override void OnPick() {
		base.OnPick();

		LevelManager.GetPlayer().SetInvincible( lifeTime );
	}

	protected override void PickEffect() {
		base.PickEffect();

		StartCoroutine (PlayActifSound (soundSource.clip.length));
	}

	protected override void WeakEffect() {
		alphaSprite = Mathf.Abs (Mathf.Sin (coefClignottant * _StaticFunction.MathPower(clignottant, 4)));
		myRender.color = new Color (myRender.color.r, myRender.color.g, myRender.color.b, alphaSprite);
		clignottant += TimeManager.deltaTime;
	}
	
	protected override void DespawnEffect() {
		base.DespawnEffect();

		soundSource.clip = soundEnd;
		soundSource.volume = soundEndVolume;
		soundSource.loop = false;
		soundSource.Play ();

		myRender.color = new Color (myRender.color.r, myRender.color.g, myRender.color.b, 1f);
		// TODO trouver pourquoi le pickup réapparait à la fin... (et pas que pour celui-ci)
	}

	private IEnumerator PlayActifSound(float delay) {
		yield return new WaitForSeconds(delay * Time.timeScale);

		soundSource.clip = soundActif;
		soundSource.volume = soundActifVolume;
		soundSource.loop = true;
		soundSource.Play ();
	}
}
