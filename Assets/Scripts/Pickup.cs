﻿using UnityEngine;
using System.Collections;

public class Pickup : MonoBehaviour {
	
	public float lifeTime = 0;
	private Renderer rdr;
	private float timeToLive;			// Temps en secondes qu'il reste avant que le bonus ne fasse plus effet
	
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
