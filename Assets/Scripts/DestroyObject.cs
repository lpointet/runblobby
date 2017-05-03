using UnityEngine;

public class DestroyObject : MonoBehaviour {

	[SerializeField] private bool gonnaExplode = false;
	[SerializeField] private int expToGive;
	private GameObject parentGameObject;

	// Affichage de la zone de TouchArea
	protected Collider2D myCollider;
	protected SpriteRenderer mySprite;

	[SerializeField] private GameObject explosionEffect;

	protected virtual void Awake () {
		parentGameObject = transform.parent.gameObject;
		mySprite = GetComponent<SpriteRenderer> ();
		myCollider = GetComponent<Collider2D> ();
	}

	protected virtual void OnEnable () {
		// Savoir si l'on active ou non les effets
	}

	protected virtual void Destroy () {
		if (gonnaExplode) {
			// Effet visuel
			GameObject explosion = PoolingManager.current.Spawn (explosionEffect.name);

			if (explosion != null) {
				explosion.transform.position = this.transform.position;
				explosion.transform.rotation = this.transform.rotation;
				explosion.SetActive (true);
			}
		}

		// On donne l'xp si besoin
		if (expToGive > 0) {
			ScoreManager.AddPoint (expToGive, ScoreManager.Types.Experience);
			StartCoroutine (UIManager.uiManager.CombatText (LevelManager.player.transform, expToGive.ToString ("0 xp"), LogType.special, (isOver => {
				if (isOver) {
					DisableObject ();
				}
			})));
		} else {
			DisableObject ();
		}

		// On désactive le sprite et le collider de l'effet du TouchArea
		myCollider.enabled = false;
		mySprite.enabled = false;

		// On désactive le sprite et le collider du parent
		parentGameObject.GetComponent<SpriteRenderer> ().enabled = false;
		parentGameObject.GetComponent<Collider2D> ().enabled = false;
	}

	protected virtual void DisableObject () {
		parentGameObject.GetComponent<SpriteRenderer> ().enabled = true;
		parentGameObject.GetComponent<Collider2D> ().enabled = true;

		parentGameObject.SetActive (false);
	}
}
