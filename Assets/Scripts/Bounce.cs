using UnityEngine;

public class Bounce : MonoBehaviour {

	public float offsetCheckBounce;
	public float bouncePower;
	public int experienceToGive = 1;
	public int damageToGive;

	private Collider2D myCollider;
	private AudioSource myAudio;
	private Animator myAnim;

	private void Awake() {
		myCollider = GetComponent<Collider2D> ();
		myAudio = GetComponent<AudioSource> ();
		myAnim = GetComponent<Animator> ();
	}

	private void OnEnable() {
		myCollider.enabled = true;

		if (myAnim != null)
			myAnim.SetBool ("dead", false);
	}

	void OnTriggerEnter2D(Collider2D other) {
		if (other.name == "Heros") {
            Rigidbody2D herosRb = other.attachedRigidbody;

			// Dans le cas où le contact se fait par dessus, on rebondit
			if (other.transform.position.y - offsetCheckBounce > transform.position.y) {
				herosRb.velocity = new Vector2 (0, bouncePower * LevelManager.player.jumpHeight);

				LevelManager.MaybeKill (transform);

				// Joue le son du rebond
				myAudio.pitch = 1.0f + Random.Range (-0.25f, 0.35f);
				myAudio.Play ();

				// Permettre au héros de sauter même après un bounce
				LevelManager.player.bounced = true;

				// Ne pas déclencher d'autres actions avec ce collider, il a fait son job
				myCollider.enabled = false;

				// On fournit l'xp au joueur
				ScoreManager.AddPoint (experienceToGive, ScoreManager.Types.Experience);
			} else {
				LevelManager.player.Hurt(damageToGive, 0, true);
			}
		}
	}
}
