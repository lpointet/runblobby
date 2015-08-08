using UnityEngine;
using System.Collections;

public class Pickup : MonoBehaviour {
	
	public float lifeTime = 0;
	private Renderer rdr;
	
	protected virtual void Awake() {
		rdr = GetComponent<Renderer>();
	}
	
	protected virtual void OnPick() {
		// Que faut-il faire lorsque cet objet a été ramassé ?
	}
	
	void OnTriggerEnter2D(Collider2D other){
		if (other.name == "Heros") {
			if( lifeTime > 0 ) {
				Hide();
				StartCoroutine( Despawn() );
			}
			else {
				gameObject.SetActive(false);
			}
			
			OnPick();
		}
	}

	private void Hide() {
		rdr.enabled = false;
	}

	private IEnumerator Despawn() {
		yield return new WaitForSeconds( lifeTime );
		gameObject.SetActive( false );
	}
}
