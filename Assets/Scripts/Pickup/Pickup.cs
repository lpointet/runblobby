using UnityEngine;
using System.Collections;

public class Pickup : MonoBehaviour {
	
	protected bool parentAttach = false;
	protected Transform initialParent;		// Référence vers le parent initial
	protected Transform myTransform; 		// Référence vers le transform du bonus
    protected Collider2D myCollider;

	protected SpriteRenderer mySprite;
	protected SpriteRenderer myBackSprite;
	protected Animator myAnim;

	// Aurait pu être dans une classe à part...
	[Header("Sound")]
	public AudioClip pickupSound;
	public float pickupVolume;
	public AudioClip actifSound;
	public float actifVolume;
	public AudioClip despawnSound;
	public float despawnVolume;
	protected AudioSource soundSource;

	[Header("Autre")]
	public float lifeTime = 0;
	protected float timeToLive;				// Temps en secondes qu'il reste avant que le bonus ne fasse plus effet
	protected float despawnTime = 0; 		// Temps pour l'animation de despawn - Pas modifiable dans l'éditeur, dans Awake de la classe fille
	protected float weakTime = 0;			// Temps pour l'animation d'affaiblissement - Pas modifiable dans l'éditeur, dans Awake de la classe fille

	protected bool picked = false;
	protected bool despawnCalled = false;
	protected bool weakCalled = false;

	public int weight = 0;					// Probabilité d'apparition relative du bonus
    
    protected virtual void Awake() {
		myTransform = transform;
		initialParent = myTransform.parent;
		SpriteRenderer[] tempSprite = GetComponentsInChildren<SpriteRenderer> ();
		if (tempSprite.Length > 0)
			mySprite = tempSprite [0];
		if(tempSprite.Length > 1)
			myBackSprite = tempSprite [1];
		myAnim = GetComponentInChildren<Animator>();
        soundSource = GetComponent<AudioSource>();
        myCollider = GetComponent<Collider2D>();

		// Ajout des talents pour la durée des pickups
		lifeTime += GameData.gameData.playerData.talent.buffLength * GameData.gameData.playerData.talent.buffLengthPointValue;
    }

	protected virtual void OnEnable() {
		timeToLive = lifeTime;
		picked = false;
        despawnCalled = false;
		weakCalled = false;

		if (mySprite != null) {
			mySprite.enabled = true;
			mySprite.transform.localPosition = Vector2.zero; // On place le Sprite au milieu de son conteneur
		}
		if (myBackSprite != null) {
			myBackSprite.enabled = false;
			myBackSprite.transform.localPosition = Vector2.zero; // On place le Sprite au milieu de son conteneur
		}
    }

	protected virtual void Update() {
		if( !picked || TimeManager.paused) {
			return;
		}

		if (timeToLive <= despawnTime && !despawnCalled) {
			StartCoroutine (Despawn ());
		} else if (timeToLive <= weakTime + despawnTime && !weakCalled && !despawnCalled) {
			StartCoroutine (Weakening ());
		}

		if( lifeTime > 0 ) {
			// Mettre à jour le temps qui reste à vivre
			timeToLive -= TimeManager.deltaTime;
		}
	}

	// On lance les effets de Pick au moment de la collision
	void OnTriggerEnter2D (Collider2D other) {
		if (picked)
			return;
		
		if (other.name == "Heros") {
			picked = true;
			OnPick();
			if (!isActiveAndEnabled)
				return;
			PickEffect();
		}
	}

	// On lance les effets de Weakening (uniquement visuel) quand le délai est atteint (voir Update)
	private IEnumerator Weakening() {
		weakCalled = true;
		while (timeToLive > despawnTime) {
			WeakEffect (); // Appelé à chaque frame jusqu'à la disparition
			yield return null;
		}
	}

	// On lance les effets de Despawn quand le timer est écoulé (voir Update)
	private IEnumerator Despawn() {
		despawnCalled = true;
		DespawnEffect();
		yield return new WaitForSecondsRealtime (despawnTime);
		OnDespawn();
	}
	
	protected virtual void OnPick() {
		// Que faut-il faire lorsque cet objet a été ramassé ?
		// ATTENTION : le Fly a une règle spéciale, ajouter les modifications d'ici dans le Fly

		// On contrôle que le joueur n'a pas déjà un pickup du même type, auquel cas on augmente la durée de vie du pickup en cours
		// Si c'est une pièce, on ne rentre pas dans le processus
		if (this.GetType () != typeof(CoinPickup)) {
			Pickup existingPickup = LevelManager.player.HasTypePickup (this.GetType ());

			if (existingPickup != null && existingPickup.transform.parent == LevelManager.player.transform) {
				existingPickup.timeToLive += lifeTime;
				this.gameObject.SetActive (false);

				return;
			}
		}

		if( parentAttach ) {
			// Attacher le bonus au joueur
			myTransform.parent = LevelManager.player.transform;
			myTransform.position = myTransform.parent.position;
        }
			
		if (myBackSprite != null)
			myBackSprite.enabled = true;

        LevelManager.player.AddPickup( myCollider );
    }

	protected virtual void PickEffect() {
		PickupSound ();

		if (_StaticFunction.ExistsAndHasParameter ("picked", myAnim))
			myAnim.SetBool ("picked", true);
		
		else if (mySprite != null) { // On cache directement ceux qui n'ont pas d'animation de ramassage
			mySprite.enabled = false;
			if (myBackSprite != null)
				myBackSprite.enabled = true;
		}
	}

	protected virtual void WeakEffect() {
		// Que faut-il faire à chaque frame lorsque l'effet s'affaiblit ?
	}

	protected virtual void DespawnEffect() {
		DespawnSound ();

		// L'effet de la mort
		if (_StaticFunction.ExistsAndHasParameter ("end", myAnim)) // On joue une animation pour ceux qui ont une fin
			myAnim.SetBool ("end", true);
	}

	protected virtual void OnDespawn() {
        // Que faut-il faire lorsque cet objet a fini sa vie ?

		if( parentAttach ) {
			// Attacher le bonus à son parent initial
			myTransform.parent = initialParent;
		}

		if (_StaticFunction.ExistsAndHasParameter ("picked", myAnim))
			myAnim.SetBool ("picked", false);

		LevelManager.player.RemovePickup( myCollider );
		gameObject.SetActive( false );
    }

	protected virtual void PickupSound() {
		if (soundSource != null && pickupSound != null) {
			soundSource.pitch = 1.0f + Random.Range (-0.25f, 0.35f);
			soundSource.volume = pickupVolume;
			soundSource.PlayOneShot (pickupSound);
			// Son "actif" dès que le pickup a fini de se dérouler
			Invoke ("ActifSound", pickupSound.length * Time.timeScale);
		} else
			ActifSound ();
    }

	protected virtual void ActifSound() {
		if (soundSource != null && actifSound != null) {
			soundSource.clip = actifSound;
			soundSource.loop = true;
			soundSource.volume = actifVolume;
			soundSource.Play ();
		}
	}

	protected virtual void DespawnSound() {
		if (soundSource != null && despawnSound != null) {
			soundSource.volume = despawnVolume;
			soundSource.PlayOneShot (despawnSound);
		}
	}

	public void Disable() {
		timeToLive = 0;
	}
}
