using System.Collections;
using UnityEngine;

/* GAEL THE STUBBORN GOLEMIC
 * Le golem a énormément de HP, et le temps imparti ne permet pas de le tuer avec les attaques conventionnelles
 * Lance en cloche des rochers vers le héros, qui tombent toujours trop tôt
 * Lorsque les rochers touchent le sol, le sol tremble et le héros est paralysé brièvement s'il est au sol
 * Si les attaques du héros touchent les rochers, ceux-ci retournent vers le golem et le blessent
 * Saute sur place et fait tomber un rocher entre lui et le héros, en plus de paralyser si le héros est au sol
 * Hard+ : A partir de 25%, 50%, se dédouble, et lance donc potentiellement deux attaques en même temps
 */
public class Enemy0302 : Enemy {

	[Header("Gael Special")]
	private static bool isDuplicated = false;
	private float ratioDuplicate;
	[Range (0.0f, 1.0f)] [SerializeField] private float hardRatioDuplicate;
	[Range (0.0f, 1.0f)] [SerializeField] private float hellRatioDuplicate;

	[Header("Atk 1 : Throw Rock")]
	[SerializeField] private float throwRockAttackDelay;
	[SerializeField] private GameObject golemRock;

	[Header("Atk 2 : Hit Ground")]
	[SerializeField] private float hitGroundAttackDelay;
	[SerializeField] private GameObject bigRock;

	protected override void Init () {
		PossibleCoin ();

		if (!isDuplicated) {
			// Ajout du temps des talents
			timeToKill += GameData.gameData.playerData.talent.bossLengthBonus * GameData.gameData.playerData.talent.bossLengthBonusPointValue;

			// Diminution de la défense de l'ennemi (si talent + pickup récupéré)
			defense = Mathf.Max (0, defense + LevelManager.reduceEnemyDefense);

			// Diminution de la vie de l'ennemi (si talent + pickup récupéré)
			healthPoint = Mathf.Max (1, healthPoint + LevelManager.reduceEnemyHealth);
		} else {
			popPosition = new Vector2 (startPosition.x - 1.5f, -3.0f);
			startPosition = new Vector2 (startPosition.x - 1.5f, 0.5f);

			// On le place derrière le décor le temps du pop
			GetComponent<SpriteRenderer> ().sortingOrder = 0;

			// Le doublon ne peut faire que la première attaque
			numberOfAttack = 1;

			// On augmente le temps d'attaque du doublon (pour éviter au minimum que les rochers se touchent entre eux en permanence)
			throwRockAttackDelay *= 1.2f;
		}

		// On le place à sa position AVANT début du combat
		if (popPosition.x == 0)
			popPosition.x = CameraManager.cameraRightPosition + 2f;
		if (popPosition.y == 0)
			popPosition.y = CameraManager.cameraUpPosition + 2f;

		myTransform.position = popPosition;
		myTransform.rotation = Quaternion.identity;

		popEnemy = true;
		mySprite.enabled = true;

		timeToFire = TimeManager.time + LevelManager.levelManager.enemySpawnDelay + Random.Range (0.5f, 1f);

		// Chargement différent selon la difficulté et le mode
		if (LevelManager.levelManager.IsStory ()) {
			switch (LevelManager.levelManager.GetCurrentDifficulty ()) {
			// Normal
			case 0:
				NormalLoad ();
				break;
			// Hard
			case 1:
				HardLoad ();
				break;
			// Hell
			case 2:
				HellLoad ();
				break;
			}
		}

		// Chargement du nombre d'attaques
		for (int i = 1; i <= numberOfAttack; i++) {
			attackList.Add (i);
		}
	}

	protected override void HardLoad () {
		ratioDuplicate = hardRatioDuplicate;
	}

	protected override void HellLoad () {
		ratioDuplicate = hellRatioDuplicate;
	}

	protected override void Update () {
		if (IsDead () || LevelManager.player.IsDead () || TimeManager.paused)
			return;

		base.Update();

		// Si HP < ratioDuplicate, on duplique le boss en mode Hard et Hell
		// On rend également le golem courant invincible
		if (!isDuplicated && healthPoint <= ratioDuplicate * healthPointMax) {
			isDuplicated = true;
			Instantiate (this);
			SetInvincible (timeToKill);
		}

		if (isDuplicated && !popEnemy) {
			// On "force" les boss à être devant quand ils sont pop (utile pour le double)
			GetComponent<SpriteRenderer> ().sortingOrder = 10;
		}
	}

	protected override void ChooseAttack (int numberAttack) {
		switch (numberAttack) {
		case 1:
			timeToFire += throwRockAttackDelay;
			// Animation du golem qui ouvre le dos
			myAnim.SetTrigger("throwing");
			// L'appel de la fonction d'attaque "ThrowRock" se fait dans l'Animator
			//ThrowRock ();
			break;
		case 2:
			timeToFire += hitGroundAttackDelay;
			// Animation du golem qui frappe le sol
			myAnim.SetTrigger("hitting");
			// L'appel de la fonction d'attaque "HitGround" se fait dans l'Animator
			//HitGround ();
			break;
		case 3:

			break;
		}
	}

	// ATTAQUE 1 : THROW ROCK
	private void ThrowRock () {
		GameObject obj = PoolingManager.current.Spawn (golemRock.name);

		if (obj != null) {
			obj.transform.position = myTransform.position + Vector3.right * 0.5f;
			obj.transform.rotation = Quaternion.identity;

			obj.SetActive (true);
			// On jette le rocher
			Rigidbody2D rockRb = obj.GetComponent<Rigidbody2D> ();
			rockRb.velocity = new Vector2 (-Random.Range (1.5f, 2.0f), Random.Range (4.5f, 6.0f));
			rockRb.angularVelocity = Random.Range (2.5f, 10.0f);
		}
	}

	// ATTAQUE 2 : HIT GROUND
	private void HitGround () {
		GameObject obj = PoolingManager.current.Spawn (bigRock.name);

		if (obj != null) {
			obj.transform.position = new Vector2 (myTransform.position.x, CameraManager.cameraUpPosition);
			obj.transform.rotation = Quaternion.identity;

			obj.SetActive (true);

			obj.GetComponent<Rigidbody2D> ().velocity = Vector2.left * Random.Range (1.0f, 2.0f);
			StartCoroutine (CheckGroundForRock (obj.transform));
		}
	}

	private IEnumerator CheckGroundForRock (Transform rock, bool isBigRock = true) {
		RaycastHit2D hit;

		// On essaye de trouver le sol juste en-dessous pour rattacher la roche
		do {
			hit = Physics2D.Raycast (rock.position, Vector2.down, 1.05f, layerGround);
			yield return null;
		} while (hit.collider == null);

		rock.parent = hit.collider.transform;
		rock.GetComponent<Rigidbody2D> ().velocity = Vector2.zero;

		CameraManager.cameraManager.ShakeScreen ();

		if (LevelManager.player.IsGrounded ())
			LevelManager.player.ShakePlayer (0.35f, true, true, true);
	}

	// A la mort, se décompose en plusieurs rochers
	protected override void Despawn () {
		Die ();

		myAnim.SetTrigger ("dead");

		RaycastHit2D hit;
		hit = Physics2D.Raycast (myTransform.position, Vector2.down, 20, layerGround);

		if (hit.collider != null) {
			myTransform.parent = hit.transform;
			myRb.isKinematic = true;
			GetComponentInChildren<EdgeCollider2D> ().enabled = false;
		}

		// Si c'est le double qui meurt, on enlève l'invincibilité de l'autre (le vrai)
		if (isDuplicated) {
			LevelManager.levelManager.GetEnemyEnCours ().SetInvincible (0);
		}
	}
}
