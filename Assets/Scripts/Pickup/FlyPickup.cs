using UnityEngine;

public class FlyPickup : Pickup {

	private float initialGravityScale;
	private float initialJumpHeight;
	private int initialMaxDoubleJump;

	private Rigidbody2D playerBody;
	private PlayerController player;

    public LayerMask groundMask;

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
		player.Jump ();

		// Faire en sorte que le nombre de sauts soit illimité (= 1000, n'abusons pas !)
		player.SetMaxDoubleJump( 1000 );
	}

	protected override void OnDespawn() {
		base.OnDespawn();

		// Remettre les paramètres initiaux
		playerBody.gravityScale = initialGravityScale;
		player.SetJumpHeight(initialJumpHeight);
		player.SetMaxDoubleJump( initialMaxDoubleJump );

        // On signale au joueur qu'il était en train de voler, pour faire apparaître des nuages s'il tombe dans un trou
        player.wasFlying = true;

        // On "force" le joueur à sauter avant l'atterrissage, signant en même temps la fin du vol
        player.Jump();
    }
}
