using UnityEngine;

public class DestroyObject : MonoBehaviour {

	[SerializeField] private bool gonnaExplode = false;
	[SerializeField] private int expToGive;
	private GameObject parentGameObject;

	protected virtual void Awake () {
		parentGameObject = transform.parent.gameObject;
	}

	protected virtual void Destroy () {
		if (gonnaExplode) {
			// Effet visuel
			GameObject explosion = PoolingManager.current.Spawn ("ObjectExplosion");

			if (explosion != null) {
				explosion.transform.position = this.transform.position;
				explosion.transform.rotation = this.transform.rotation;
				explosion.SetActive (true);

				explosion.GetComponent<ParticleUnscaled> ().Play ();
			}
		}

		// On donne l'xp
		ScoreManager.AddPoint(expToGive, ScoreManager.Types.Experience);
		StartCoroutine (UIManager.uiManager.CombatText (LevelManager.player.transform, expToGive.ToString ("0 xp"), LogType.special, (isOver => {
			if (isOver) {
				DisableObject ();
			}
		})));

		// On désactive le sprite et le collider
		parentGameObject.GetComponent<SpriteRenderer> ().enabled = false;
		parentGameObject.GetComponent<Collider2D> ().enabled = false;
	}

	protected virtual void DisableObject () {
		parentGameObject.GetComponent<SpriteRenderer> ().enabled = true;
		parentGameObject.GetComponent<Collider2D> ().enabled = true;

		parentGameObject.SetActive (false);
	}
}
