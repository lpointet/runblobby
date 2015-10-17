using UnityEngine;

public class LastWishPickup : Pickup {

    public float radiusMagnet = 0f;
    public LayerMask layerCoins;

    private PlayerController player;
    private bool effectOnGoing = false;

    protected override void Awake() {
        base.Awake();

        parentAttach = true;
    }

    void Start() {
        player = LevelManager.getPlayer();
    }

    protected override void Update() {
        base.Update();

        if( !picked ) {
            return;
        }

        if( player.IsDead() ) {
            if( !effectOnGoing ) {
                Effect();
            }

            // T'as un magnet
            player.AttractCoins( radiusMagnet, layerCoins );
        }
        else {
            // Ce pickup ne doit jamais disparaitre jusqu'à la mort du joueur
            timeToLive = lifeTime;
        }
    }

    protected override void OnPick() {
        base.OnPick();

        if( player.HasLastWish() ) {
            // Un last wish a déjà été récup, on se casse de là
            timeToLive = 0;
        }
        else {
            player.SetLastWish( true );
        }
    }

    protected override void OnDespawn() {
        base.OnDespawn();

        if( effectOnGoing ) {
            // Désactiver le vol
            player.Land();
            player.SetLastWish( false );
            player.OnKill();
        }
    }

    public void Effect() {
        effectOnGoing = true;

        // C'est pour l'instant le seul moyen que j'ai trouvé pour ne pas rester dans la position de la mort (qui peut être bloquante)
        // TODO: remplacer ou améliorer avec une animation ?
        player.transform.position = LevelManager.levelManager.currentCheckPoint.transform.position;

        // Tu voles
        player.Fly();

        // T'es invul
        player.SetInvincible( lifeTime );
    }
}
