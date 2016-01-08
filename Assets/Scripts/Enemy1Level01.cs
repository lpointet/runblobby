using UnityEngine;
using System.Collections;

/* GASTON LE GASTEROPODE
 * Il se contente d'envoyer des bombes/glands de temps en temps, en parabole
 */
public class Enemy1Level01 : Enemy {

	[Header("Gaston Special")]
	public float delayBaving = 2f;
	private float timeToBave;
	public string baveName = "Bave";

	private Animator myAnim;

	protected override void Awake () {
		base.Awake();

		myAnim = GetComponent<Animator> ();
		timeToBave = Time.time + Random.Range(delayBaving / 2f, 3 * delayBaving / 2f);
	}

	void Start() {
		myAnim.SetFloat("ratioHP", 1f);
	}

	protected override void Update () {
		base.Update();

		if (IsDead () || LevelManager.GetPlayer ().IsDead ())
			return;

		// L'ennemi se rapproche du joueur au fil du temps (joueur considéré en position (0, 0)) pour finir à mi-chemin
		myTransform.Translate(Vector3.left * LevelManager.levelManager.GetLocalDistance() * startPosition[0] / (GetDistanceToKill() * 2));

		if (Time.time > timeToBave) {
			timeToBave = Time.time + Random.Range(delayBaving / 2f, 3 * delayBaving / 2f);
			BaveDrop ();
		}
	}

	private void BaveDrop() {
		RaycastHit2D hit;
		hit = Physics2D.Raycast (myTransform.position, Vector2.down, 20, layerGround); // On essaye de trouver le sol, sinon on ne fait rien

		if (hit.collider != null) {
			GameObject obj = PoolingManager.current.Spawn (baveName);

			if (obj != null) {
				obj.transform.position = new Vector2(myTransform.position.x, -1f); // On place juste en dessous du boss
				obj.transform.rotation = myTransform.rotation;
				obj.transform.parent = hit.transform;
				obj.transform.localScale = new Vector2 (Random.Range (1.75f, 2.5f), Random.Range (1f, 1.5f)); // Pas toujours la même taille de flaque
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
		myAnim.SetTrigger ("dead");

		RaycastHit2D hit;
		hit = Physics2D.Raycast (myTransform.position, Vector2.down, 20, layerGround);

		if (hit.collider != null) {
			myTransform.parent = hit.transform;

			GetComponent<EdgeCollider2D> ().enabled = false;
			GetComponent<Rigidbody2D> ().isKinematic = true;
		} else {
			GetComponent<EdgeCollider2D> ().enabled = false;
		}
	}
}
