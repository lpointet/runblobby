using UnityEngine;
using System.Collections;

public class FlyPickup : Pickup {

	private float initialGravityScale;
	private float initialJumpHeight;
	private int initialMaxDoubleJump;

	private Rigidbody2D playerBody;
	private PlayerController player;

	protected override void Awake() {
		base.Awake();

		parentAttach = true;
	}

	void Start() {
		player = LevelManager.getPlayer ();
		playerBody = player.GetComponent<Rigidbody2D>();

		if( null == playerBody ) {
			Debug.LogError( "Pas de Rigidbody2D sur le joueur ?!?" );
		}

		initialGravityScale = playerBody.gravityScale;
		initialJumpHeight = player.GetJumpHeight ();
		initialMaxDoubleJump = player.GetMaxDoubleJump();
	}

	protected override void OnPick() {
		base.OnPick();

		// Abaisser la gravité et la hauteur du saut
		playerBody.gravityScale = 0.2f;
		player.SetJumpHeight(2);

		// Faire décoller le joueur
		playerBody.velocity = new Vector2 (playerBody.velocity.x, player.GetJumpHeight());

		// Faire en sorte que le nombre de sauts soit illimité (= 1000, n'abusons pas !)
		player.SetMaxDoubleJump( 1000 );
	}

	protected override void OnDespawn() {
		base.OnDespawn();

		// Remettre la gravité et la hauteur du saut
		playerBody.gravityScale = initialGravityScale;
		player.SetJumpHeight(initialJumpHeight);

		// Remettre une limite au nombre de sauts
		player.SetMaxDoubleJump( initialMaxDoubleJump );
	}
}
