using UnityEngine;
using System.Collections;

public class FlyPickup : Pickup {
	private float initialGravityScale;
	private Rigidbody2D playerBody;

	protected override void Awake() {
		base.Awake();

		parentAttach = true;
	}

	void Start() {
		playerBody = LevelManager.getPlayer().GetComponent<Rigidbody2D>();

		if( null == playerBody ) {
			Debug.LogError( "Pas de Rigidbody2D sur le joueur ?!?" );
		}

		initialGravityScale = playerBody.gravityScale;
	}

	protected override void OnPick() {
		base.OnPick();

		// Abaisser la gravité
		playerBody.gravityScale = 1.5f;

		// TODO: Faire décoller le joueur

		// TODO: Faire en sorte que le nombre de sauts soit illimité
	}

	protected override void OnDespawn() {
		base.OnDespawn();

		// Remettre la gravité
		playerBody.gravityScale = initialGravityScale;

		// TODO: Remettre une limite au nombre de sauts ?
	}
}
