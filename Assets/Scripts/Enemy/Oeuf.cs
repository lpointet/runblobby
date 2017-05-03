using UnityEngine;

public class Oeuf : MonoBehaviour {

	private Animator myAnim;
	private AudioSource myAudio;
	private Transform myTransform;

	private bool broken;
	private float invincibleDelay = 0.2f;
	private float timeBeforeBreakable;

	[SerializeField] ExplosionRadius explosionEffect;

	// Hell particularités
	private float timeBeforeExplosion;

	public bool IsBroken() {
		return broken;
	}

	void Awake () {
		myAnim = GetComponentInChildren<Animator> ();
		myAudio = GetComponent<AudioSource> ();
		myTransform = transform;
	}

	void OnEnable () {
		broken = false;
		myAnim.SetBool ("broken", broken);

		timeBeforeBreakable = TimeManager.time;
	}

	void Update () {
		// On ne fait rien si on n'est pas en Hell
		if (LevelManager.levelManager.GetCurrentDifficulty () < 2)
			return;
		
		if (!broken) {
			myTransform.localScale = Vector2.one * (1 + 0.25f * Mathf.Sin ((CameraManager.cameraRightPosition + 2f - myTransform.position.x) * (TimeManager.time - timeBeforeBreakable)));
			// On force l'explosion quand on est proche de la position par défaut du héros
			if (myTransform.position.x < 1)
				Explode ();
		}
	}

	void OnTriggerEnter2D (Collider2D other) {
		// On ne casse pas l'oeuf trop vite...
		if (TimeManager.time < timeBeforeBreakable + invincibleDelay)
			return;
		
		if (other.name == "Heros" || other.CompareTag("Bullet")) {
			if (!IsBroken()) {
				Explode ();
			}
		}
	}

	private void Explode () {
		myTransform.localScale = Vector2.one;

		broken = true;
		myAudio.pitch = 1.0f + Random.Range (-0.25f, 0.35f);
		myAudio.Play ();
		myAnim.SetBool ("broken", broken);

		// Effet d'explosion
		explosionEffect.StartExplosion ();
	}
}
