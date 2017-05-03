using UnityEngine;

public class PoolMud : Pool {

	private float timeToAddMud;

	protected override void EnterEffect () {
		// Si on est en mode "histoire", avec une difficulté de Hard+
		if (LevelManager.levelManager.IsStory () && LevelManager.levelManager.GetCurrentDifficulty () > 0) {
			WaterBubble.waterBubble.PreventNextBreath ();
		}
	}

	protected override void StayEffect () {
		if (TimeManager.time > timeToAddMud) {
			// On ajoute de l'opacité à la boue environnante
			MudScreen.mudScreen.AddMud (0.15f);

			timeToAddMud = TimeManager.time + 0.1f;
		}
	}

}
