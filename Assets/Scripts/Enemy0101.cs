using UnityEngine;
using System.Collections;

/* GASTON LE GASTEROPODE
 * Il se contente d'envoyer de la bave de temps en temps, qui colle/blesse le joueur
 * TODO : en hard, renvoyer les missiles du joueur de temps en temps selon la normale de la carapace
 */
public class Enemy0101 : Enemy {

	[Header("Gaston Special")]
	public float delayBaving = 2f;
	private float timeToBave;
	public string baveName = "Bave";

	protected override void Awake () {
		base.Awake();

		timeToBave = TimeManager.time + Random.Range(delayBaving / 2f, 3 * delayBaving / 2f);
	}

	void Start() {
		myAnim.SetFloat("ratioHP", 1f);
	}

	protected override void Update () {
		base.Update();

		if (IsDead () || LevelManager.GetPlayer ().IsDead () || TimeManager.paused)
			return;

		// L'ennemi se rapproche du joueur au fil du temps (joueur considéré en position (0, 0)) pour finir à mi-chemin
		myTransform.Translate(Vector3.left * TimeManager.deltaTime * startPosition[0] / (GetTimeToKill() * 2));

		if (TimeManager.time > timeToBave) {
			timeToBave = TimeManager.time + Random.Range(delayBaving / 2f, 3 * delayBaving / 2f);
			BaveDrop ();
		}
	}

	private void BaveDrop() {
		RaycastHit2D hit;
		hit = Physics2D.Raycast (myTransform.position, Vector2.down, 20, layerGround); // On essaye de trouver le sol, sinon on ne fait rien

		if (hit.collider != null) {
			GameObject obj = PoolingManager.current.Spawn (baveName);

			if (obj != null) {
				obj.transform.position = new Vector2(myTransform.position.x, hit.transform.position.y); // On place juste en dessous du boss
				obj.transform.rotation = myTransform.rotation;
				obj.transform.parent = hit.transform;

				obj.SetActive (true);
			}
		}
	}

	public override void Hurt(int damage) {
		base.Hurt (damage);

		myAnim.SetFloat("ratioHP", GetHealthPoint() / (float)GetHealthPointMax());
	}

	// A la mort, attacher l'ennemi au sol, et laisser la carapace
	protected override void Despawn () {
		Die ();

		myAnim.SetTrigger ("dead");

		RaycastHit2D hit;
		hit = Physics2D.Raycast (myTransform.position, Vector2.down, 20, layerGround);

		if (hit.collider != null) {
			myTransform.parent = hit.transform;
			myRb.isKinematic = true;
			GetComponent<EdgeCollider2D> ().enabled = false;
			StartCoroutine (RollCarapace());
		}
	}

	// On le fait tourner, s'il est mort
	private IEnumerator RollCarapace() {
		float axeMaxRoll = 0;
		while (myTransform.rotation.eulerAngles.z < 15) {
			axeMaxRoll += TimeManager.deltaTime;
			myTransform.Rotate (Vector3.forward, axeMaxRoll);
			yield return null;
		}
	}
}
