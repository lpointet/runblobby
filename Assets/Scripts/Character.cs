using UnityEngine;
using System.Collections;

public class Character : MonoBehaviour {

	/**
	 * Character Stats 
	 */
	public int healthPoint;
	public int healthPointMax;
	public float moveSpeed;
	public float jumpHeight;
	public bool isDead;
	/* End of Stats */

	void Start() {
		Init();
	}
	
	protected virtual void Init() {
		// Init health
		FullHealth();
	}
	
	public virtual void OnKill() {
		// On fait le deuil
	}

	public void Hurt(int damage) {
		healthPoint -= damage;
		
		if (healthPoint <= 0 && !isDead) {
			LevelManager.Kill( this );
		}
	}
	
	public void FullHealth() {
		healthPoint = healthPointMax;
	}
}
