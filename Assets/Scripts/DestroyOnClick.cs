using UnityEngine;

// La zone qui possède ce script doit être AU-DESSOUS (dans la liste) de tous les autres objets
public class DestroyOnClick : DestroyObject {

	void Start () {
		Mediator.current.Subscribe<TouchClickable> (IsTouched);
	}

	private void IsTouched (TouchClickable touch) {
		if (touch.objectId == this.gameObject.GetInstanceID () && PlayerCanClickBreak()) {
			Destroy ();
			LevelManager.player.canBreakByClick--;
		}
	}

	private bool PlayerCanClickBreak () {
		if (LevelManager.player.canBreakByClick > 0)
			return true;

		return false;
	}
}
