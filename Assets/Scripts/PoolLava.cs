using UnityEngine;

public class PoolLava : Pool {

	protected override void EnterEffect () {
		// Ajouter le maximum de chaleur au joueur
		BurningGround.current.FullHeat ();
	}
}
