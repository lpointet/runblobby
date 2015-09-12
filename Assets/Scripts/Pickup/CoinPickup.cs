using UnityEngine;

public class CoinPickup : Pickup {
	
	public int pointToAdd;
	private Vector3 initialPosition;
	private Quaternion initialRotation;

	private Animator anim;
	private AudioSource soundSource;
	
	protected override void Awake() {
		base.Awake();
		initialPosition = transform.localPosition;
		initialRotation = transform.localRotation;

		anim = GetComponent<Animator> ();
		soundSource = GetComponent<AudioSource> ();
		despawnTime = 1f;
	}
	
	public void Reset() {
		myTransform.localPosition = initialPosition;
		myTransform.localRotation = initialRotation;
		//anim.SetBool ("picked", false);
	}

	protected override void OnEnable() {
		base.OnEnable();

		// Réinitialiser les positions
		Reset();
	}

	void OnBecameInvisible() {
		// On ne veut pas pouvoir interagir avec cette pièce si elle n'est plus visible (cf. AutoCoinPickup)
		gameObject.SetActive( false );
	}

	protected override void OnPick() {
		base.OnPick();

		// Ajouter les points au joueur
		ScoreManager.AddPoint(pointToAdd, ScoreManager.Types.Coin);
	}

	protected override void PickEffect() {
		// Déclenche les effets visuels dans l'animator
		anim.SetBool ("picked", true);
	}

	private void PickupSound() {
		soundSource.Play ();
	}
}
