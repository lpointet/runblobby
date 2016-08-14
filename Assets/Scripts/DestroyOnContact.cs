using UnityEngine;

public class DestroyOnContact : DestroyObject {

	void OnTriggerEnter2D(Collider2D other) {
		if (other.name == "Heros" && PlayerCanContactBreak ()) {
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
