using UnityEngine;

public class Bounce : MonoBehaviour {

	public float offsetCheckBounce;
	public float bouncePower;
	public int experienceToGive = 1;

	private Collider2D myCollider;

	private void Awake() {
		myCollider = GetComponent<Collider2D>();
	}

	private void OnEnable() {
		myCollider.enabled = true;
	}

	void OnTriggerEnter2D(Collider2D other) {
		if (other.tag == "Player") {
			PlayerController player = LevelManager.GetPlayer();
            Rigidbody2D herosRb = other.attachedRigidbody;

			// Dans le cas où le contact se fait par dessus
			if (other.transform.position.y - offsetCheckBounce > transform.position.y) {
                herosRb.velocity = new Vector2(0, bouncePower * player.GetJumpHeight());

				LevelManager.MaybeKill( transform );

                // Permettre au héros de sauter même après un bounce
                player.bounced = true;

				// Ne pas déclencher d'autres actions avec ce collider, il a fait son job
				myCollider.enabled = false;

				// On fournit l'xp au joueur
				ScoreManager.AddPoint(experienceToGive, ScoreManager.Types.Experience);
			}
		}
	}
}
