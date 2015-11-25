using UnityEngine;
using System.Collections;

public class InviciblePickup : Pickup {

	public AudioClip soundActif;
	public float soundActifVolume = 0.4f;
	public AudioClip soundEnd;
	public float soundEndVolume = 0.8f;

	private SpriteRenderer mySprite;
	private float timeToClignote = 3f;
	private float clignottant;
	private float coefClignottant = 0.05f;
	private float alphaSprite = 1f;

	protected override void Awake() {
		base.Awake();

		mySprite = GetComponent<SpriteRenderer> ();

		parentAttach = true;
		despawnTime = 0.3f;

		clignottant = Mathf.Sqrt (Mathf.Sqrt (Mathf.PI / (2f * coefClignottant)));
	}

	protected override void Update() {
		base.Update ();

		if( picked && timeToLive < timeToClignote && timeToLive > 0) {
			alphaSprite = Mathf.Abs (Mathf.Sin (coefClignottant * _StaticFunction.MathPower(clignottant, 4)));
			mySprite.color = new Color (mySprite.color.r, mySprite.color.g, mySprite.color.b, alphaSprite);
			clignottant += Time.deltaTime;
			//mySprite.sharedMaterial.SetFloat ("_HueShift", _StaticFunction.MappingScale (timeToLive, lifeTime, 0, 0, -20));
		}
	}
	
	protected override void OnPick() {
		base.OnPick();

		LevelManager.GetPlayer().SetInvincible( lifeTime );
	}

	protected override void PickEffect() {
		base.PickEffect();

		StartCoroutine (PlayActifSound (soundSource.clip.length));
	}
	
	protected override void DespawnEffect() {
		base.DespawnEffect();

		soundSource.clip = soundEnd;
		soundSource.volume = soundEndVolume;
		soundSource.loop = false;
		soundSource.Play ();

		mySprite.color = new Color (mySprite.color.r, mySprite.color.g, mySprite.color.b, 1f);
		//mySprite.sharedMaterial.SetFloat ("_HueShift", 0);
	}

	private IEnumerator PlayActifSound(float delay) {
		yield return new WaitForSeconds(delay);

		soundSource.clip = soundActif;
		soundSource.volume = soundActifVolume;
		soundSource.loop = true;
		soundSource.Play ();
	}
}
