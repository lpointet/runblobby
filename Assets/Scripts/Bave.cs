using UnityEngine;
using System.Collections;

public class Bave : MonoBehaviour {

	public Sprite[] variousBave;

	private float backForce;
	public float normalBackForce = 0.04f;
	public float hardBackForce = 0.08f;
	public float hellBackForce = 0.16f;

	private int damageOnEnter;
	public int normalDamage = 0;
	public int hardDamage = 0;
	public int hellDamage = 1;

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
		GetComponent<SpriteRenderer> ().sprite = variousBave [Random.Range (0, variousBave.Length)];

		transform.localScale = new Vector2 (Random.Range (1.75f, 2.5f), Random.Range (1f, 1.5f)); // Pas toujours la même taille de flaque
	}

	void OnTriggerStay2D(Collider2D other) {
		if (other.name == "Heros" && LevelManager.GetPlayer().IsGrounded() && !LevelManager.GetPlayer().IsFlying() && !LevelManager.GetPlayer().IsDead()) {
			Transform herosTransform = other.transform;
			herosTransform.Translate (Vector2.left * backForce);
		}
	}

	void OnTriggerEnter2D(Collider2D other) {
		if (other.name == "Heros" && LevelManager.GetPlayer().IsGrounded() && !LevelManager.GetPlayer().IsFlying()) {
			LevelManager.GetPlayer ().Hurt (damageOnEnter);
		}
	}
}
