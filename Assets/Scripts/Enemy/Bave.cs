using UnityEngine;
using System.Collections;

public class Bave : MonoBehaviour {

	private AudioSource myAudio;
	private Transform myTransform;

	public Sprite[] baveSprite;

	public float minXScale;
	public float maxXScale;
	public float minYScale;
	public float maxYScale;
	private Vector2 finalScale;
	private float lerpingTime;

	private float backForce;
	public float normalBackForce = 0.02f;
	public float hardBackForce = 0.04f;
	public float hellBackForce = 0.08f;

	private int damageOnEnter;
	public int normalDamage = 0;
	public int hardDamage = 0;
	public int hellDamage = 1;

	void Awake () {
		myAudio = GetComponent<AudioSource> ();
		myTransform = transform;
	}

	void Start() {
		// On ajuste les effets selon la difficulté
		switch (LevelManager.levelManager.GetCurrentDifficulty()) {
		case 0:
			backForce = normalBackForce;
			damageOnEnter = normalDamage;
			break;
		case 1:
			backForce = hardBackForce;
			damageOnEnter = hardDamage;
			break;
		case 2:
			backForce = hellBackForce;
			damageOnEnter = hellDamage;
			break;
		default:
			backForce = normalBackForce;
			damageOnEnter = normalDamage;
			break;
		}
	}

	void OnEnable() {
		// On affiche un sprite aléatoire parmi la liste
		GetComponent<SpriteRenderer> ().sprite = baveSprite [Random.Range (0,  baveSprite.Length)];

		finalScale = new Vector2 (Random.Range (minXScale, maxXScale), Random.Range (minYScale, maxYScale)); // Pas toujours la même taille de flaque
		lerpingTime = 0;
		myTransform.localScale = Vector2.one;

		if (myAudio != null) {
			myAudio.pitch = 1.0f + Random.Range (-0.25f, 0.35f);
			myAudio.Play ();
		}
	}

	void Update() {
		if (myTransform.localScale.x <= finalScale.x) {
			lerpingTime += TimeManager.deltaTime / 0.5f;
			myTransform.localScale = Vector2.Lerp (Vector2.one, finalScale, lerpingTime);
		}
	}

	void OnTriggerStay2D(Collider2D other) {
		if (other.name == "Heros" && LevelManager.player.IsGrounded() && !LevelManager.player.IsFlying() && !LevelManager.player.IsDead()) {
			LevelManager.player.transform.Translate (Vector2.left * backForce);
		}
	}

	void OnTriggerEnter2D(Collider2D other) {
		if (damageOnEnter > 0 && other.name == "Heros" && LevelManager.player.IsGrounded() && !LevelManager.player.IsFlying()) {
			LevelManager.player.Hurt (damageOnEnter, 0, true);
		}
	}
}
