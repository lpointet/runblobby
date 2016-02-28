using UnityEngine;
using System.Collections;

public class Pickup : MonoBehaviour {
	
	public float lifeTime = 0;
	protected bool parentAttach = false;
	protected Transform initialParent;		// Référence vers le parent initial
	protected Transform myTransform; 		// Référence vers le transform du bonus
    protected Collider2D myCollider;
	protected float despawnTime = 0; 		// protected parce qu'il doit etre réglé dans la classe fille directement, pas modifiable dans l'éditeur, dans Awake
	protected SpriteRenderer myRender;
	protected bool picked = false;
	protected float timeToLive;				// Temps en secondes qu'il reste avant que le bonus ne fasse plus effet
	public int weight = 0;					// Probabilité d'apparition relative du bonus
    protected Animator myAnim;
    protected AudioSource soundSource;
    protected bool despawnCalled = false;

    protected virtual void Awake() {
		myTransform = transform;
		initialParent = myTransform.parent;
		myRender = GetComponentInChildren<SpriteRenderer>();
		myAnim = GetComponentInChildren<Animator>();
        soundSource = GetComponent<AudioSource>();
        myCollider = GetComponent<Collider2D>();
    }

	protected virtual void OnEnable() {
		timeToLive = lifeTime;
		picked = false;
        despawnCalled = false;
    }

	protected virtual void Update() {
		if( !picked || Time.timeScale == 0) {
			return;
		}
		
		if( timeToLive <= 0 && !despawnCalled ) {
			StartCoroutine( Despawn() );
		}
		
		if( lifeTime > 0 ) {
			// Mettre à jour le temps qui reste à vivre
			timeToLive -= Time.unscaledDeltaTime;
		}
	}

	void OnTriggerEnter2D(Collider2D other) {
		if (other.name == "Heros") {
			picked = true;
			OnPick();
			PickEffect();
		}
	}
	
	protected virtual void OnPick() {
		// Que faut-il faire lorsque cet objet a été ramassé ?

		if( parentAttach ) {
			// Attacher le bonus au joueur
			myTransform.parent = LevelManager.GetPlayer().transform;
			myTransform.position = myTransform.parent.position;
        }

		myRender.transform.localPosition = Vector2.zero; // On place le Sprite au milieu de son conteneur

        LevelManager.GetPlayer().AddPickup( myCollider );
    }
	
	protected virtual void OnDespawn() {
        // Que faut-il faire lorsque cet objet a fini sa vie ?

		if( parentAttach ) {
			// Attacher le bonus à son parent initial
			myTransform.parent = initialParent;
		}
			
        LevelManager.GetPlayer().RemovePickup( myCollider );
		gameObject.SetActive( false );
    }

	protected virtual void PickEffect() {
		PickupSound ();

		if (_StaticFunction.ExistsAndHasParameter ("picked", myAnim))
        	myAnim.SetBool ("picked", true);

		else if (myRender != null) // On cache directement ceux qui n'ont pas d'animation de ramassage
			myRender.enabled = false;
    }
	
	protected virtual void DespawnEffect() {
		// L'effet de la mort
		if (_StaticFunction.ExistsAndHasParameter ("end", myAnim)) // On joue une animation pour ceux qui ont une fin
			myAnim.SetBool ("end", true);

		if (soundSource)
			_StaticFunction.AudioFadeOut (soundSource, 0, despawnTime);
	}

	private IEnumerator Despawn() {
        despawnCalled = true;
        DespawnEffect();
		yield return new WaitForSeconds (despawnTime * Time.timeScale);
		OnDespawn();
	}

    protected void PickupSound() {
        if (soundSource) soundSource.Play();
    }

	public void Disable() {
		timeToLive = 0;
	}
}
