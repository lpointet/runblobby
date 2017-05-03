using UnityEngine;

public class ThunderStrike : MonoBehaviour {

	private Transform playerTransform;
	private Transform myTransform;
	private Collider2D myCollider;
	private Animator myAnim;
	private AudioSource myAudio;

	private float distanceFromPlayer = 10.0f; 		// Distance à laquelle l'objet doit tomber par rapport au joueur
	//public float fallingSpeed = 2f;				// Vitesse de chute

	private bool thunderAppeared = false;

	public AudioClip audioThunder;
	public AudioClip audioFire;

	void Awake () {
		playerTransform = LevelManager.player.transform;
		myTransform = transform;
		myCollider = GetComponent<Collider2D> ();
		myAnim = GetComponent<Animator> ();
		myAudio = GetComponent<AudioSource> ();
	}

	void OnEnable() {
		thunderAppeared = false;
		myCollider.enabled = false;
		//myTransform.localScale = Vector2.one;
	}

	void Update () {
		// Distance d'apparition en fonction de la vitesse du joueur
		distanceFromPlayer = 7 * LevelManager.player.RatioSpeed();

		// On déclenche la foudre dès qu'il apparait dans le champ de vision
		if (!thunderAppeared && myTransform.position.x <= playerTransform.position.x + distanceFromPlayer) {
			// On agrandit un peu
			//myTransform.localScale = Random.Range(0.9f, 1.1f) * Vector2.one;
			// On déclenche l'animation
			myAnim.SetTrigger("thunder");
			myAudio.PlayOneShot(audioThunder);

			thunderAppeared = true;
		}
	}

	public void FireAppear() {
		//myTransform.localScale = Vector2.one;
		myCollider.enabled = true;

		myAudio.Stop ();
		myAudio.pitch = 1.0f + Random.Range (-0.25f, 0.35f);
		myAudio.clip = audioFire;
		myAudio.Play ();
	}
}
