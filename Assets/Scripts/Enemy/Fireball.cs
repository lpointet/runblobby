using UnityEngine;
using System.Collections;

public class Fireball : MonoBehaviour {

	private Rigidbody2D myRb;
	private Animator myAnim;
	private Transform myTransform;
	private SpriteRenderer mySprite;
	private ParticleSystem myParticle;
	private ParticleSystem.MainModule particleMain;

	[SerializeField] private float moveSpeed;

	private int damageToPlayer = 1;
	[SerializeField] private int normalDamage;
	[SerializeField] private int hardDamage;
	[SerializeField] private int hellDamage;
	[SerializeField] private int arcadeDamage;

	[SerializeField] private Color explosiveColor; // Utilisée pour différencier les boules qui vont exploser
	private Color naturalColor;

	private float chanceToExplode;
	[SerializeField] private float normalChanceToExplode;
	[SerializeField] private float hardChanceToExplode;
	[SerializeField] private float hellChanceToExplode;
	private bool isExploding = false; // Permet de savoir si elle doit exploser
	private bool hasExploded = false; // Permet de savoir si elle a déjà explosée
	private bool isAChild = false; // Permet de savoir si c'est une boule qui résulte d'une autre boule (et donc n'explose pas)
	private float distanceBeforeExplosion;

	public void IsAChild (bool child) {
		isAChild = child;
	}

	void Awake () {
		myRb = GetComponent<Rigidbody2D> ();
		myAnim = GetComponent<Animator> ();
		mySprite = GetComponent<SpriteRenderer> ();
		myParticle = GetComponentInChildren<ParticleSystem> ();
		myTransform = transform;
		particleMain = myParticle.main;

		naturalColor = mySprite.color;

		// Chargement différent selon la difficulté et le mode
		if (LevelManager.levelManager.IsStory ()) {
			switch (LevelManager.levelManager.GetCurrentDifficulty ()) {
			// Normal
			case 0:
				damageToPlayer = normalDamage;
				chanceToExplode = normalChanceToExplode;
				break;
				// Hard
			case 1:
				damageToPlayer = hardDamage;
				chanceToExplode = hardChanceToExplode;
				break;
				// Hell
			case 2:
				damageToPlayer = hellDamage;
				chanceToExplode = hellChanceToExplode;
				break;
			}
		} else // Arcade
			damageToPlayer = arcadeDamage;
	}

	void OnEnable () {
		myAnim.SetBool ("born", true);
		isExploding = false;
		hasExploded = false;

		// Vitesse en fonction de l'angle
		float rotaZ = myTransform.eulerAngles.z * Mathf.Deg2Rad;
		myRb.velocity = new Vector2 (-Mathf.Cos (rotaZ), -Mathf.Sin (rotaZ)) * moveSpeed;

		// Savoir si la boule va exploser ou non
		if (isAChild || Random.Range (0f, 1f) > chanceToExplode) {
			mySprite.color = naturalColor;
			particleMain.startColor = naturalColor;
		} else {
			mySprite.color = explosiveColor;
			particleMain.startColor = explosiveColor;
			isExploding = true;
			distanceBeforeExplosion = Random.Range (5f, 7.5f); // Explosion entre 5 et 7,5 unités devant le héros
		}
	}

	void Update () {
		if (!hasExploded && isExploding && myTransform.position.x < distanceBeforeExplosion) {
			mySprite.color = naturalColor;
			particleMain.startColor = naturalColor;
			hasExploded = true;

			FireExplosion (Random.Range(3, 5));
		}
	}

	// Invocation d'autant de boules que demandées
	// La direction dépend du nombre de boules
	private void FireExplosion (int numberOfBalls) { // Nombre de balles en plus de celle qui continue sa route
		float explosionAngle = 90; // Angle en degrés de l'explosion

		for (int i = 0; i < numberOfBalls; i++) {
			GameObject obj = PoolingManager.current.Spawn (name.Replace("(Clone)", ""));

			if (obj != null) {
				obj.transform.position = myTransform.position;
				obj.transform.rotation = Quaternion.Euler (0, 0, (i + 1) * -explosionAngle / (numberOfBalls));

				obj.GetComponent<Fireball> ().IsAChild (true);

				obj.SetActive (true);
			}
		}
	}

	void OnTriggerEnter2D (Collider2D other) {
		if (other.CompareTag ("Player")) {
			LevelManager.player.Hurt (damageToPlayer);
		}
	}

	void OnBecameInvisible () {
		Despawn ();
	}

	private void Despawn () {
		gameObject.SetActive (false);
	}
}
