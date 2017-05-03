using UnityEngine;

public class DestroyOnContact : DestroyObject {
	
	protected override void OnEnable () {
		if (PlayerCanContactBreak ()) {
			myCollider.enabled = true;
			mySprite.enabled = true;
		}
		else {
			myCollider.enabled = false;
			mySprite.enabled = false;
		}
	}

	void OnTriggerEnter2D (Collider2D other) {
		if (other.name == "Heros") {
			Destroy ();
			LevelManager.player.canBreakByContact--;
		}
	}

	private bool PlayerCanContactBreak () {
		if (LevelManager.player.canBreakByContact > 0)
			return true;

		return false;
	}
}
