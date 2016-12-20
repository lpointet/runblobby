using UnityEngine;

public class PoolWater : Pool {

	[Header("Back Force")]
	private float backForce;
	public float normalBackForce;
	public float hardBackForce;
	public float hellBackForce;

	[Header("Pollen Discharge Ratio")]
	// Ratio déchargement du pollen en cours
	private float pollenDischarge;
	public float normalPollenDischarge;
	public float hardPollenDischarge;
	public float hellPollenDischarge;

	protected override void NormalLoad () {
		backForce = normalBackForce;
		pollenDischarge = normalPollenDischarge;
	}

	protected override void HardLoad () {
		backForce = hardBackForce;
		pollenDischarge = hardPollenDischarge;
	}

	protected override void HellLoad () {
		backForce = hellBackForce;
		pollenDischarge = hellPollenDischarge;
	}

	protected override void ArcadeLoad () {
		HardLoad ();
	}

	protected override void EnterEffect () {
		// Suppression d'une partie du pollen
		if (PollenEffect.current != null && PollenEffect.current.isActiveAndEnabled)
			PollenEffect.current.CoroutineRemovePollen (pollenDischarge);
	}

	protected override void StayEffect () {
		// Ajouter une force qui repousse le joueur
		LevelManager.player.transform.Translate (Vector2.left * backForce);
	}
}
