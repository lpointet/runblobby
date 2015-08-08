using UnityEngine;
using System.Collections;

public class CoinPickup : Pickup {
	
	public int pointToAdd;

	protected override void OnPick() {
		// Ajouter les points au joueur
		ScoreManager.AddPoint(pointToAdd);
	}
}
