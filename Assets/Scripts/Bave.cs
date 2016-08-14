using UnityEngine;
using System.Collections;

public class Bave : MonoBehaviour {

	private AudioSource myAudio;
	private Transform myTransform;

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
		GetComponent<SpriteRenderer> ().sprite = ListManager.current.bave [Random.Range (0,  ListManager.current.bave.Length)];

		finalScale = new Vector2 (Random.Range (1.75f, 2.5f), Random.Range (1f, 1.5f)); // Pas toujours la même taille de flaque
		lerpingTime = 0;
		myTransform.localScale = Vector2.one;

		myAudio.Play ();
	}

	void Update() {
		if (myTransform.localScale.x <= finalScale.x) {
			lerpingTime += TimeManager.deltaTime / 0.5f;
			myTransform.localScale = Vector2.Lerp (Vector2.one, finalScale, lerpingTime);
		}
	}

	void OnTriggerStay2D(Collider2D other) {
		if (other.name == "Heros" && LevelManager.player.IsGrounded() && !LevelManager.player.IsFlying() && !LevelManager.player.IsDead()) {
			Transform herosTransform = other.transform;
			herosTransform.Translate (Vector2.left * backForce);
		}
	}

	void OnTriggerEnter2D(Collider2D other) {
		if (damageOnEnter > 0 && other.name == "Heros" && LevelManager.player.IsGrounded() && !LevelManager.player.IsFlying()) {
			LevelManager.player.Hurt (damageOnEnter, 0, true);
		}
	}
}
