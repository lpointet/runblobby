using UnityEngine;

public class CoinPickup : Pickup {
	
	public int pointToAdd;
	private Vector3 initialPosition;
	private Quaternion initialRotation;
	
	protected override void Awake() {
		base.Awake();
		initialPosition = transform.localPosition;
		initialRotation = transform.localRotation;

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
        // On vérifie que le despawn n'est pas déjà en cours
        if( !despawnCalled ) {
	    	gameObject.SetActive( false );
        }
    }

    protected override void OnPick() {
		base.OnPick();

		// Ajouter les points au joueur
		ScoreManager.AddPoint(pointToAdd, ScoreManager.Types.Coin);
	}
}
