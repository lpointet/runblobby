using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// TODO ajouter des écrans après la victoire en cas de débloquage de nouveaux niveaux/diff/mode/items
// TODO afficher les boutons pour poursuivre APRES ces écrans (si pas d'écrans, après le chargement des barres habituelles)
// TODO mettre un texte par défaut pour la description des items "vides" dans le menu des équipements

/* DRAGO LE DRAGON
 * Se déplace lentement de bas en haut, avec peu de variations
 * Crée des souffles de flammes avec un seul espace pour que le héros passe
 * Soulève des vagues de lave qui se déplace vers le héros (diverses vitesses/hauteurs)
 * TODO Hard+ : Les boules d'eau durent moins longtemps
 * Hard+ : DECOR : Des boules de feu décollent de la lave et retombent
 */
public class Enemy0203 : Enemy {
	
	[Header("Drago Movement")]
	[SerializeField] private float amplitude;
	[SerializeField] private float waveFrequence;
	private float timeMouvement;
	private int goingUp; // Pour savoir si le dragon va vers le haut ou vers le bas - Utile pour la direction du souffle

	[Header("Atk 1 : Fire Breath")]
	private bool isBreathing = false;
	[SerializeField] private Firebreath firebreath;
	private float fireBreathAttackDelay; // Durée de l'attaque
	[SerializeField] private float fbNormalAttackDelay;
	[SerializeField] private float fbHardAttackDelay;
	[SerializeField] private float fbHellAttackDelay;
	[SerializeField] private float fireBreathBetweenShot; // Délai entre chaque "souffle"

	[Header("Atk 2 : Lava Wave")]
	[SerializeField] private GameObject eggDrop;
	private Transform eggDropTransform;
	private Vector2 initialEggPosition;
	[SerializeField] private GameObject lavaWave;
	private Transform lavaWaveTransform;
	private float lavaWaveAttackDelay; // Durée de l'attaque
	[SerializeField] private float lwNormalAttackDelay;
	[SerializeField] private float lwHardAttackDelay;
	[SerializeField] private float lwHellAttackDelay;
	private float lavaWaveSpeed; // Vitesse de la vague
	[SerializeField] private float lwNormalSpeed;
	[SerializeField] private float lwHardSpeed;
	[SerializeField] private float lwHellSpeed;
	private float lavaWaveMinHeight;
	private float lavaWaveMaxHeight;
	private int lavaWaveDamage = 1;
	[SerializeField] private int lwNormalDamage;
	[SerializeField] private int lwHardDamage;
	[SerializeField] private int lwHellDamage;

	[Header("Atk 3 : Fiery Jump Ball")]
	[SerializeField] private JumpFireBall jumpBall;

	protected override void Awake () {
		base.Awake();

		eggDropTransform = eggDrop.transform;
		initialEggPosition = eggDropTransform.localPosition;
		eggDrop.SetActive (false);

		lavaWaveTransform = lavaWave.transform;
		lavaWave.SetActive (false);

		startPosition [1] = Camera.main.orthographicSize - CameraManager.cameraManager.yOffset / 2f; // On le place au milieu de l'écran
		popPosition [1] = startPosition [1];
	}

	void Start () {
		lavaWaveMaxHeight = ((Camera.main.orthographicSize * 2f) / 3f) * 0.75f; // Taille de l'écran / (ratio du sprite) * 75%
		lavaWaveMinHeight = lavaWaveMaxHeight * 0.5f;
	}

	protected override void NormalLoad () {
		fireBreathAttackDelay = fbNormalAttackDelay;
		lavaWaveAttackDelay = lwNormalAttackDelay;
		lavaWaveSpeed = lwNormalSpeed;
		lavaWaveDamage = lwNormalDamage;
	}

	protected override void HardLoad () {
		fireBreathAttackDelay = fbHardAttackDelay;
		lavaWaveAttackDelay = lwHardAttackDelay;
		lavaWaveSpeed = lwHardSpeed;
		lavaWaveDamage = lwHardDamage;
	}

	protected override void HellLoad () {
		fireBreathAttackDelay = fbHellAttackDelay;
		lavaWaveAttackDelay = lwHellAttackDelay;
		lavaWaveSpeed = lwHellSpeed;
		lavaWaveDamage = lwHellDamage;
	}

	protected override void Update () {
		if (IsDead () || LevelManager.player.IsDead () || TimeManager.paused)
			return;

		base.Update();

		// Une fois que le "chargement" du boss est terminé, il commence ses oscillations
		if (!popEnemy) {
			myTransform.position = new Vector2 (startPosition[0], startPosition[1] + amplitude * Mathf.Sin (waveFrequence * timeMouvement));
			goingUp = System.Math.Sign (Mathf.Cos (waveFrequence * timeMouvement)); // Le mouvement étant en Sin, le signe est en Cos

			timeMouvement += TimeManager.deltaTime;
		}
	}

	protected override void ChooseAttack (int numberAttack) {
		switch (numberAttack) {
		case 1:
			timeToFire += fireBreathAttackDelay;

			// Animation du dragon qui ouvre la bouche
			myAnim.SetBool("breathing", true);

			// L'appel de la fonction d'attaque "FireBreath" se fait dans l'Animator
			//FireBreath ();
			break;
		case 2:
			timeToFire += lavaWaveAttackDelay;
			StartCoroutine (EggDrop ());
			break;
		}
	}

	// ATTAQUE 1 : FIRE BREATH
	private void FireBreath () {
		if (isBreathing) // Evite que l'Animator appelle cette fonction en boucle
			return;

		isBreathing = true;

		float fireBreathAngle;
		float higherPointAngle;
		float lowerPointAngle;
		int fireBreathNumberShot;

		higherPointAngle = Mathf.Rad2Deg * Mathf.Atan2 (CameraManager.cameraUpPosition - myTransform.position.y, myTransform.position.x); // On considère que le héros est toujours en 0
		lowerPointAngle = Mathf.Rad2Deg * Mathf.Atan2 (myTransform.position.y - CameraManager.cameraDownPosition, myTransform.position.x);
		fireBreathAngle = lowerPointAngle + higherPointAngle;

		fireBreathNumberShot = Mathf.CeilToInt (fireBreathAngle / 7f); // Un "souffle" tous les 7 degrés

		int randomSafeSpot; // Permet de savoir quel souffle on évite, sachant qu'il ne peut être dans les deux premiers et derniers
		randomSafeSpot = Random.Range (2, fireBreathNumberShot - 2);

		for (int i = 0; i < fireBreathNumberShot; i++) {
			if (i == randomSafeSpot)
				continue;
			// Selon le sens du dragon, on commence par le bas ou par le haut
			if (goingUp < 0)
				StartCoroutine (SingleFireBreath (Quaternion.Euler (0, 0, i * fireBreathAngle / (float)(fireBreathNumberShot - 1) - higherPointAngle), i * fireBreathBetweenShot));
			else
				StartCoroutine (SingleFireBreath (Quaternion.Euler (0, 0, i * -fireBreathAngle / (float)(fireBreathNumberShot - 1) + lowerPointAngle), i * fireBreathBetweenShot));
		}

		StartCoroutine (AnimatorEndBreath (fireBreathBetweenShot * fireBreathNumberShot + 0.5f)); // Arrêt de l'animation
	}

	private IEnumerator SingleFireBreath (Quaternion fireRotation, float delay) {
		if (IsDead () || LevelManager.player.IsDead () || TimeManager.paused)
			yield break;

		yield return new WaitForSecondsRealtime (delay);

		GameObject obj = PoolingManager.current.Spawn (firebreath.name);

		if (obj != null) {
			obj.transform.position = new Vector2 (myTransform.position.x - 1.25f, myTransform.position.y);
			obj.transform.rotation = fireRotation;

			obj.SetActive (true);
		}
	}

	private IEnumerator AnimatorEndBreath (float delay) {
		yield return new WaitForSecondsRealtime (delay);
		// Animation du dragon qui ferme sa mouille !
		myAnim.SetBool("breathing", false);

		yield return new WaitForSecondsRealtime (1f);
		// Pour éviter que le souffle se déclenche une nouvelle fois dans la dernière animation
		isBreathing = false;
	}

	// ATTAQUE 2 : LAVA WAVE
	private IEnumerator EggDrop () {
		eggDrop.SetActive (true);
		eggDropTransform.localPosition = initialEggPosition;
		eggDropTransform.rotation = Quaternion.Euler (new Vector3 (0, 0, Random.Range( -15f, 15f)));
		eggDrop.GetComponent<Rigidbody2D> ().velocity = Vector2.zero;

		while (eggDropTransform.position.y > -CameraManager.cameraManager.yOffset) {
			yield return null;
		}

		eggDrop.SetActive (false);
		// Déclenchement de la lave quand l'oeuf touche la lave (le sol)
		LavaWave ();
	}

	private void LavaWave () {
		lavaWave.SetActive (true);

		lavaWaveTransform.localScale = Vector2.one * 0.1f;
		lavaWaveTransform.position = new Vector2 (myTransform.position.x, CameraManager.cameraDownPosition);

		StartCoroutine (GrowingLavaWave ());
	}

	private IEnumerator GrowingLavaWave () {
		float timeToGrow = 1f;
		float currentGrow = 0;

		float maxScaleX = Random.Range (lavaWaveMinHeight, lavaWaveMaxHeight);
		float maxScaleY = maxScaleX;
		Vector2 maxScale = new Vector2 (maxScaleX, maxScaleY);

		while (currentGrow < timeToGrow) {
			// La vague grossit pendant 1sec avant d'avancer
			lavaWaveTransform.localScale = Vector2.Lerp (Vector2.one * 0.1f, maxScale, currentGrow / timeToGrow);
			// On "force" à la bonne place
			lavaWaveTransform.position = new Vector2 (myTransform.position.x, CameraManager.cameraDownPosition);

			currentGrow += TimeManager.deltaTime;
			yield return null;
		}
		// Quand elle a fini de grossir, elle avance vers le héros
		StartCoroutine (MovingLavaWave (maxScale));
	}

	private IEnumerator MovingLavaWave (Vector2 startScale) {
		float distanceToTravel = myTransform.position.x - CameraManager.cameraStartPosition; // Fonctionne dans ce sens car startPosition est négatif
		float currentDistance = 0;

		while (currentDistance < distanceToTravel) {
			// Légère diminution de la taille de la vague
			lavaWaveTransform.localScale = Vector2.Lerp (startScale, startScale * 0.75f, currentDistance / distanceToTravel);
			// Déplacement de la vague
			lavaWaveTransform.position = new Vector2 (lavaWaveTransform.position.x - TimeManager.deltaTime * lavaWaveSpeed, CameraManager.cameraDownPosition); // Indépendant de la vitesse du héros

			currentDistance += LevelManager.levelManager.GetLocalDistance ();

			yield return null;
		}

		lavaWave.SetActive (false);
	}

	protected override void OnTriggerEnter2D (Collider2D other) {
		// Si l'ennemi est déjà mort, il ne peut plus rien faire...
		if( IsDead() )
			return;
		
		if (other.name == "Heros") {
			// On assume que si l'objet "LavaWave" est actif, le héros s'est fait toucher par ça
			if (lavaWave.activeInHierarchy) {
				LevelManager.player.Hurt (lavaWaveDamage, sharp);
			}
		}
	}

	// A la mort, le dragon tombe dans la lave
	protected override void Despawn () {
		// On arrête tout
		StopAllCoroutines ();
		jumpBall.Despawn ();

		Die ();

		myAnim.SetTrigger ("dead");

		myRb.gravityScale = 0.25f;
		myRb.AddForce (Vector2.up * 100);
		myRb.AddTorque (Random.Range (-7f, 7f));
	}
}