using UnityEngine;
using System.Collections;

public class InviciblePickup : Pickup {

	private float clignottant;
	private float coefClignottant = 0.05f;

	protected override void Awake() {
		base.Awake();

		parentAttach = true;
		despawnTime = 0.3f;
		weakTime = 3f;

		clignottant = Mathf.Sqrt (Mathf.Sqrt (Mathf.PI / (2f * coefClignottant)));
	}
	
	protected override void OnPick() {
		base.OnPick();

		LevelManager.GetPlayer().SetInvincible( lifeTime );
	}

	protected override void WeakEffect() {
		Color tempColor = myRender.color;
		tempColor.a = Mathf.Abs (Mathf.Sin (coefClignottant * _StaticFunction.MathPower(clignottant, 4)));
		myRender.color = tempColor;

		clignottant += TimeManager.deltaTime;
	}
	
	protected override void DespawnEffect() {
		base.DespawnEffect();

		myRender.color = new Color (myRender.color.r, myRender.color.g, myRender.color.b, 1f);
	}
}
