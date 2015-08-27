using UnityEngine;
using System.Collections;

public class ContactEnemy : MonoBehaviour {

	public int damageToGive;
	public int pointScore;

	public bool bouncable;
	public float offsetCheckBounce;
	public float bouncePower;

	private Rigidbody2D herosRb;
	private Renderer myRend;
	
	void Awake () {
		myRend = GetComponent<Renderer> ();
	}

	void OnTriggerEnter2D(Collider2D other){
		if (other.name == "Heros") {
			herosRb = other.attachedRigidbody;

			if(other.transform.position.y - offsetCheckBounce > transform.position.y){
				herosRb.velocity = new Vector2(herosRb.velocity.x, bouncePower);
				ScoreManager.AddPoint(pointScore, ScoreManager.Types.Enemy);
			} else {
				LevelManager.getPlayer().Hurt(1);
			}

			myRend.enabled = false;
		}
	}
}
