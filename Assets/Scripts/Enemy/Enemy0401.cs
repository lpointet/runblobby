using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/* LOUSTIK LE CONCOMBRE DE MER
 * Tire des bulles d'eau en sinusoïde
 * Aspire parfois le héros vers lui, tout en continuant de tirer des bulles d'eau
 * Le héros peut continuer de sauter et tirer pendant ce temps
 * Une fois proche, il enferme le héros dans une bulle qui le renvoie à sa place et l'empêche de bouger/tirer
 * Pendant ce temps, des morceaux de concombres tombent du haut et le joueur doit cliquer pour les détruire
 * Si les morceaux de concombres touchent le héros, il est blessé
 * Si les morceaux de concombres touchent le fond, ils soulèvent de la vase qui cache en partie l'écran plusieurs secondes
 * Hard+ : les morceaux de concombres se divisent en deux une première fois au clic du joueur
 */

public class Enemy0401 : Enemy {

	[Header("Atk 1 : Floating Bubble")]
	[SerializeField] private float floatingBubbleAttackDelay = 0.5f;
	[SerializeField] private GameObject floatingBubble;

	[Header("Atk 2 : Warping Hero")]
	private bool isWarping = false; // TRUE tant que le joueur est aspiré et jusqu'à qu'il retourne à son point de départ
	[SerializeField] private float warpingInDelay; // Temps que le héros met à venir vers le concombre
	private float warpingOutDelay; // Temps que le héros met à retourner à sa place quand le concombre l'expulse
	[SerializeField] private float normalWarpingOutDelay;
	[SerializeField] private float hardWarpingOutDelay;
	[SerializeField] private float hellWarpingOutDelay;

	[SerializeField] private GameObject paralysingBubble;
	[SerializeField] private GameObject particleAspiration;

	[Header("Atk 2 follow-up : Falling Cucumber")]
	[SerializeField] private GameObject fallingCucumber;
	private int fallingCucumberNumber;
	[SerializeField] private int normalFallingCucumberNumber;
	[SerializeField] private int hardFallingCucumberNumber;
	[SerializeField] private int hellFallingCucumberNumber;

	private Transform playerTransform;

	void Start() {
		playerTransform = LevelManager.player.transform;
	}

	protected override void NormalLoad () {
		warpingOutDelay = normalWarpingOutDelay;
		fallingCucumberNumber = normalFallingCucumberNumber;
	}

	protected override void HardLoad () {
		warpingOutDelay = hardWarpingOutDelay;
		fallingCucumberNumber = hardFallingCucumberNumber;
	}

	protected override void HellLoad () {
		warpingOutDelay = hellWarpingOutDelay;
		fallingCucumberNumber = hellFallingCucumberNumber;
	}

	protected override void ChooseAttack (int numberAttack) {
		// Si on est en train d'aspirer le héros (donc attaque 2), on ne peut déclencher que l'attaque 1 pendant ce temps
		if (isWarping)
			numberAttack = 1;
		
		switch (numberAttack) {
		case 1:
			timeToFire += floatingBubbleAttackDelay;

			// Appel durant l'animation si on est dans le cas classique, sinon on ne joue pas l'animation
			if (!isWarping) {
				// Animation du concombre qui lève les yeux
				myAnim.SetTrigger ("launchingBubble");
				// L'appel de la fonction d'attaque "FloatingBubble" se fait dans l'Animator
			} else {
				FloatingBubble ();
			}
			break;
		case 2:
			// Animation du concombre qui aspire
			myAnim.SetTrigger ("warpingHero");

			StartCoroutine (WarpingHero ());

			break;
		}
	}

	// ATTAQUE 1 : FLOATING BUBBLE
	private void FloatingBubble () {
		GameObject obj = PoolingManager.current.Spawn (floatingBubble.name);

		if (obj != null) {
			obj.transform.position = new Vector2 (myTransform.position.x - 1.0f, myTransform.position.y - 0.5f);
			obj.transform.rotation = Quaternion.identity;

			obj.SetActive (true);
		}
	}

	// ATTAQUE 2 : WARPING HERO
	private IEnumerator WarpingHero () {
		isWarping = true;
		particleAspiration.SetActive (true);

		yield return new WaitForSecondsRealtime (0.5f);
		timeToFire += 0.5f;

		float warpingTime = 0;
		float distanceToTravel; // Distance entre le joueur au moment de l'invocation et le boss, à 3 unités devant lui
		distanceToTravel = myTransform.position.x - playerTransform.position.x - 3.0f;

		float currentPlayerPosX = playerTransform.position.x;

		//* Aspiration du héros
		while (warpingTime < warpingInDelay && playerTransform.position.x < myTransform.position.x - 3.0f) {
			// On fait avancer le héros vers le boss, à 3 unités de lui
			//playerTransform.Translate (Vector2.right * TimeManager.deltaTime * distanceToTravel / warpingInDelay);
			playerTransform.position = new Vector2 (AnimationCurve.EaseInOut (0, currentPlayerPosX, warpingInDelay, distanceToTravel).Evaluate (warpingTime), playerTransform.position.y);

			warpingTime += TimeManager.deltaTime;
			yield return null;
		}

		isWarping = false;
		particleAspiration.SetActive (false);

		// Animation du concombre qui arrête d'aspirer
		myAnim.SetTrigger ("releasingHero");
		// Ajout du temps de chute des rochers pour ne pas tirer des bulles en même temps
		timeToFire += warpingOutDelay;

		//* Relachement du héros
		warpingTime = 0;
		distanceToTravel = playerTransform.position.x; // On veut le faire retourner en (0, 0)
		// Création de la bulle autour du héros + paralysie
		paralysingBubble.SetActive (true);
		paralysingBubble.transform.parent = playerTransform;
		paralysingBubble.transform.position = playerTransform.position;
		LevelManager.player.SwitchTouch (false);
		// On fait tomber les concombres !
		StartCoroutine (FallingCucumber());

		while (warpingTime < warpingOutDelay && playerTransform.position.x > 0) {
			// On fait avancer le héros vers sa position "normale"
			playerTransform.Translate (Vector2.left * TimeManager.deltaTime * distanceToTravel / warpingOutDelay);

			warpingTime += TimeManager.deltaTime;
			yield return null;
		}

		LevelManager.player.SwitchTouch (true);
		paralysingBubble.transform.parent = myTransform;
		paralysingBubble.SetActive (false);
	}

	private IEnumerator FallingCucumber () {
		for (int i = 0; i < fallingCucumberNumber; i++) {
			// Temps d'attente entre chaque concombre, relatif au nombre de concombres
			yield return new WaitForSecondsRealtime (i * fallingCucumberNumber / warpingOutDelay);

			GameObject obj = PoolingManager.current.Spawn (fallingCucumber.name);

			if (obj != null) {
				obj.SetActive (true);
			}
		}
	}

	// A la mort, le concombre se fait découper
	protected override void Despawn () {
		// On arrête tout
		StopAllCoroutines ();

		Die ();

		// On détruit tous les projectiles restants
        foreach (FallingCucumber cucumber in FindObjectsOfType(typeof(FallingCucumber)))
            cucumber.Despawn ();
        foreach (FloatingBubble bubble in FindObjectsOfType(typeof(FloatingBubble)))
            bubble.Despawn ();

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