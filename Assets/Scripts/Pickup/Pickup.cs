using UnityEngine;
using System.Collections;

public class Pickup : MonoBehaviour {
	
	public float lifeTime = 0;
	protected bool parentAttach = false;
	protected Transform initialParent;						// Référence vers le parent initial
	protected Transform myTransform; 							// Référence vers le transform du bonus
    protected Collider2D myCollider;
	protected float despawnTime = 0; 	// protected parce qu'il doit etre réglé dans la classe fille directement, pas modifiable dans l'éditeur
	private Renderer rdr;
	protected bool picked = false;
	protected float timeToLive;			// Temps en secondes qu'il reste avant que le bonus ne fasse plus effet
	public int weight = 0;			// Probabilité d'apparition relative du bonus
    protected Animator myAnim;
    protected AudioSource soundSource;
    protected bool despawnCalled = false;

    protected virtual void Awake() {
		rdr = GetComponent<Renderer>();
		myTransform = transform;
		initialParent = myTransform.parent;
        myAnim = GetComponent<Animator>();
        soundSource = GetComponent<AudioSource>();
        myCollider = GetComponent<Collider2D>();
    }

	protected virtual void OnEnable() {
		timeToLive = lifeTime;
		picked = false;
        despawnCalled = false;
    }
	
	protected virtual void OnPick() {
		// Que faut-il faire lorsque cet objet a été ramassé ?

		if( parentAttach ) {
			// Attacher le bonus au joueur
			myTransform.parent = LevelManager.getPlayer().transform;
			myTransform.position = myTransform.parent.position;
        }

        LevelManager.getPlayer().AddPickup( myCollider );
    }
	
	protected virtual void OnDespawn() {
        // Que faut-il faire lorsque cet objet a fini sa vie ?

		if( parentAttach ) {
			// Attacher le bonus à son parent initial
			myTransform.parent = initialParent;
		}

		gameObject.SetActive( false );
        LevelManager.getPlayer().RemovePickup( myCollider );
    }
	
	void OnTriggerEnter2D(Collider2D other){
		if (other.name == "Heros") {
			picked = true;
			OnPick();
			PickEffect();
		}
	}

	protected virtual void Update() {
		if( !picked ) {
			return;
		}

		if( timeToLive <= 0 && !despawnCalled ) {
			StartCoroutine( Despawn() );
		}

		if( lifeTime > 0 ) {
			// Mettre à jour le temps qui reste à vivre
			timeToLive -= Time.deltaTime;
		}
	}

	protected virtual void PickEffect() {
        if ( null != myAnim ) {
			if (!_StaticFunction.HasParameter ("picked", myAnim)) // On cache directement ceux qui n'ont pas d'animation de ramassage
				rdr.enabled = false;
			else
            	myAnim.SetBool("picked", true);
        }
    }
	
	protected virtual void DespawnEffect() {
		// L'effet de la mort
	}

	private IEnumerator Despawn() {
        despawnCalled = true;
        DespawnEffect();
		yield return new WaitForSeconds( despawnTime );
		OnDespawn();
	}

    protected void PickupSound() {
        if (soundSource) soundSource.Play();
    }
}
