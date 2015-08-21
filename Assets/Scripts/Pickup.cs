using UnityEngine;
using System.Collections;

public class Pickup : MonoBehaviour {
	
	public float lifeTime = 0;
	private Renderer rdr;
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
			if( lifeTime > 0 ) {
				Hide();
				StartCoroutine( Despawn() );
			}
			else {
				//gameObject.SetActive(false);
			}
			
			OnPick();
		}
	}

	protected virtual void Update() {
		if( lifeTime > 0 ) {
			// Mettre à jour le temps qui reste à vivre
			timeToLive-= Time.deltaTime;
		}
	}

	private void Hide() {
		rdr.enabled = false;
	}

	private IEnumerator Despawn() {
		yield return new WaitForSeconds( lifeTime );
		gameObject.SetActive( false );
		OnDespawn();
	}
}
