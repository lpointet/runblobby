using UnityEngine;

// La zone qui possède ce script doit être AU-DESSOUS (dans la liste) de tous les autres objets
public class DestroyOnClick : DestroyObject {

	protected override void OnEnable () {
		if (PlayerCanClickBreak ()) {
			myCollider.enabled = true;
			mySprite.enabled = true;
		} else {
			myCollider.enabled = false;
			mySprite.enabled = false;
		}
	}

	void Start () {
		Mediator.current.Subscribe<TouchClickable> (IsTouched);
	}

	private void IsTouched (TouchClickable touch) {
		if (touch.objectId == this.gameObject.GetInstanceID ()) {
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
