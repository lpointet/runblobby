using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/* SERBE ET RUSSE LE CERBERES
 * Ils envoient régulièrement des boules de feu en ligne droite vers le joueur
 * TODO : en hard+, certaines boules de feu explosent à mi chemin en plein d'autres boules (couleur différente ?)
 * TODO : en hard+, il y a un plafond de feu (pour éviter les doubles sauts) ?
 */
public class Enemy0201 : Enemy {

	[Header("Serbe et Russe Special")]
	[SerializeField] private GameObject fireBallName;
	[SerializeField] private int numberOfBall;
	[SerializeField] private float delayBetweenFire; // Délai entre deux BdF
	private float[] yFireBallPosition = { -0.5f, 0.1f };

	[SerializeField] private GameObject firePit;

	void Start() {
		//myAnim.SetFloat("ratioHP", 1f);
	}

	protected override void ChooseAttack (int numberAttack) {
		switch (numberAttack) {
		case 1:
			int salveBall = numberOfBall + Random.Range (-1, 2); // Plus ou moins une boule
			timeToFire += salveBall * delayBetweenFire + 1;

			myAnim.SetBool ("fireBall", true);

			StartCoroutine (FireSalve (salveBall));
			break;
		case 2:
			timeToFire += Random.Range (0.5f, 1f); // Ajout d'un délai court

			// Animation du chien avec les pattes enflammées
			myAnim.SetTrigger("firePit");

			// L'appel de la fonction d'attaque "FirePit" se fait dans l'Animator
			break;
		}
	}

	private IEnumerator FireSalve (float numberOfShots) {
		// Attente de 1sec pour permettre à l'animation de se mettre en place
		yield return new WaitForSeconds (1 * Time.timeScale);

		int randomPosition = 1; // Permet de définir la position de la précédente boule de feu

		for (int i = 0; i < numberOfShots; i++) {
			// Si la précédent boule de feu était en 0 (= en bas) - Permet de ne pas avoir deux boules en bas de suite
			if (randomPosition == 0)
				randomPosition = Random.Range (1, yFireBallPosition.Length);
			else
				randomPosition = Random.Range (0, yFireBallPosition.Length);

			StartCoroutine (FireShot (randomPosition, i * delayBetweenFire));
		}

		yield return new WaitForSeconds ((numberOfShots * delayBetweenFire) * Time.timeScale);

		// Arrêt de l'animation qui tire des boules
		myAnim.SetBool ("fireBall", false);
	}

	private IEnumerator FireShot (int verticalIndex, float delay) {
		if (IsDead () || LevelManager.player.IsDead () || TimeManager.paused)
			yield break;
		
		yield return new WaitForSeconds (delay * Time.timeScale);

		GameObject obj = PoolingManager.current.Spawn (fireBallName.name);

		if (obj != null) {
			obj.transform.position = new Vector2(myTransform.position.x - 1.25f, myTransform.position.y + yFireBallPosition[verticalIndex]);
			obj.transform.rotation = myTransform.rotation;
			obj.transform.parent = myTransform;
			obj.GetComponent<Fireball> ().IsAChild (false);

			obj.SetActive (true);
		}
	}

	private void FirePit () {
		RaycastHit2D hit;
		hit = Physics2D.Raycast (myTransform.position, Vector2.down, 20, layerGround); // On essaye de trouver le sol, sinon on ne fait rien

		if (hit.collider != null) {
			GameObject obj = PoolingManager.current.Spawn (firePit.name);

			if (obj != null) {
				obj.transform.position = new Vector2(myTransform.position.x, hit.transform.position.y - 4/16f); // On place juste en dessous du boss
				obj.transform.rotation = myTransform.rotation;
				obj.transform.parent = hit.transform;

				obj.SetActive (true);
			}
		}
	}

	// TODO A la mort, transformer l'ennemi en flammes
	protected override void Despawn () {
		// On arrête tout
		StopAllCoroutines ();

		Die ();

		myAnim.SetTrigger ("dead");

		RaycastHit2D hit;
		hit = Physics2D.Raycast (myTransform.position, Vector2.down, 20, layerGround);

		if (hit.collider != null) {
			myTransform.parent = hit.transform;
			myRb.isKinematic = true;
			GetComponentInChildren<EdgeCollider2D> ().enabled = false;
		}
	}
}

