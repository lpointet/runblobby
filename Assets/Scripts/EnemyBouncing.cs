using UnityEngine;
using System.Collections;

public class EnemyBouncing : MonoBehaviour {

	public float offsetCheckBounce;
	public float bouncePower;

	public int damageToGive;
	public int scorePoint;

	void OnTriggerEnter2D(Collider2D other) {
		Rigidbody2D herosRb;

		if (other.name == "Heros") {
			herosRb = other.attachedRigidbody;
		
			// Dans le cas où le contact se fait par dessus
			if (other.transform.position.y - offsetCheckBounce > transform.position.y) {

				herosRb.velocity = new Vector2 (herosRb.velocity.x, bouncePower);
				ScoreManager.AddPoint (scorePoint);

				gameObject.SetActive(false);
			} 
			// Si le contact se fait par le coté
			else {
				LevelManager.getPlayer ().Hurt (damageToGive);
			}
		}
	}
}
