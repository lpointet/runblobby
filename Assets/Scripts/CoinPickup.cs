using UnityEngine;
using System.Collections;

public class CoinPickup : Pickup {
	
	public int pointToAdd;
	private Vector3 initialPosition;
	private Quaternion initialRotation;
	private Transform myTransform;

	private bool picked = false;
	
	protected override void Awake() {
		initialPosition = transform.localPosition;
		initialRotation = transform.localRotation;
		myTransform = transform;
	}
	
	public void Reset() {
		transform.localPosition = initialPosition;
		transform.localRotation = initialRotation;
	}

	void OnEnable() {
		// Réinitialiser les positions
		Reset();
	}

	void OnBecameInvisible() {
		// On ne veut pas pouvoir interagir avec cette pièce si elle n'est plus visible (cf. AutoCoinPickup)
		gameObject.SetActive( false );
	}

	protected override void Update() {
		base.Update();

		// Un mouvement de haut en bas
		myTransform.localPosition = new Vector2(myTransform.localPosition.x, initialPosition.y + Mathf.Sin (4 * Time.time) / 20f);
		// Effets visuels (démarrés dans OnPick)
		if (picked) {
			myTransform.Translate(Vector3.up * Time.deltaTime);
		}
	}

	protected override void OnPick() {
		// Ajouter les points au joueur
		ScoreManager.AddPoint(pointToAdd);
		// Déclenche les effets visuels
		picked = true;
		GetComponent<Animation> ().Play ("FadeOut_GoingUp");
	}
}
