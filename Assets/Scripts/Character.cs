using UnityEngine;
using System.Collections;

public class Character : MonoBehaviour {

	public class Stats {
		public int pointsDeVie;
	}

	private Stats stats = new Stats();

	public void DamageCharacter(int damage) {
		stats.pointsDeVie -= damage;
	}
}
