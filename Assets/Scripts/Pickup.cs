using UnityEngine;
using System.Collections;

public class Pickup : MonoBehaviour {
	
	public float lifeTime = 0;
	protected float despawnTime = 0; 	// protected parce qu'il doit etre réglé dans la classe fille directement, pas modifiable dans l'éditeur
	private Renderer rdr;
	protected bool picked = false;
	protected float timeToLive;			// Temps en secondes qu'il reste avant que le bonus ne fasse plus effet
	// TODO: On a besoin d'un 2ème timer ici :
	//  - le premier sert pour la durée de vie de l'effet en lui-meme
	//  - le deuxième sert pour le temps que le bonus met à disparaitre après la fin de sa vie, c'est utile pour :
	//    - avoir une animation de despawn (qui dure un certain temps)
	//    - avoir un fonctionnement différent entre la vie et la fin de vie du bonus (cf. autocoin qui finit ce qu'il a en cours et qui ralentit le mouvement)
	
	protected virtual void Awake() {
		rdr = GetComponent<Renderer>();
		timeToLive = lifeTime;
	}
	
	protected virtual void OnPick() {
		// Que faut-il faire lorsque cet objet a été ramassé ?
	}
	
	protected virtual void OnDespawn() {
		// Que faut-il faire lorsque cet objet a fini sa vie ?
	}
	
	void OnTriggerEnter2D(Collider2D other){
		if (other.name == "Heros") {
			picked = true;
			PickEffect();
			OnPick();
		}
	}

	protected virtual void Update() {
		if( !picked ) {
			return;
		}

		if( timeToLive <= 0 ) {
			StartCoroutine( Despawn() );
		}

		if( lifeTime > 0 ) {
			// Mettre à jour le temps qui reste à vivre
			timeToLive-= Time.deltaTime;
		}
	}

	protected virtual void PickEffect() {
		Hide();
	}
	
	protected virtual void DespawnEffect() {
		// L'effet de la mort
	}

	private void Hide() {
		rdr.enabled = false;
	}

	private IEnumerator Despawn() {
		DespawnEffect();
		yield return new WaitForSeconds( despawnTime );
		gameObject.SetActive( false );
		OnDespawn();
	}
}
