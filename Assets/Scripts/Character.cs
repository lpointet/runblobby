using UnityEngine;
using System.Collections;

public class Character : MonoBehaviour {

	/**
	 * Character Stats 
	 */
	[SerializeField] private int healthPoint;
	[SerializeField] private int healthPointMax;
	[SerializeField] private float moveSpeed;
	[SerializeField] private float jumpHeight;
	[SerializeField] private bool isDead;
	/* End of Stats */
	
	private bool invincible = false;

	/**
	 * Getters & Setters
	 */
	public int GetHealthPoint() {
		return healthPoint;
	}

	public int GetHealthPointMax() {
		return healthPointMax;
	}

	public float GetMoveSpeed() {
		return moveSpeed;
	}

	public float GetJumpHeight() {
		return jumpHeight;
	}

	public bool IsDead() {
		return isDead;
	}

	public void SetHealthPoint( int value ) {
		healthPoint = Mathf.Clamp( value, 0, GetHealthPointMax() );
	}

	public void SetHealthPointMax( int value ) {
		healthPointMax = value;
	}

	public void SetMoveSpeed( float value ) {
		moveSpeed = value;
	}

	public void SetJumpHeight( float value ) {
		jumpHeight = value;
	}

	// Créer 2 setters pour isDead parce que les noms sont plus cool :)
	public void Die() {
		isDead = true;
	}
	public void Resurrect() {
		isDead = false;
	}
	/* End of Getters & Setters */

	void Start() {
		Init();
	}
	
	protected virtual void Init() {
		// Init health
		FullHealth();
		// Let it live
		Resurrect();
	}
	
	public virtual void OnKill() {
		// On fait le deuil
	}

	public void Hurt(int damage) {
		if( invincible ) {
			return;
		}

		SetHealthPoint( GetHealthPoint() - damage );
		
		if (GetHealthPoint() <= 0 && !IsDead()) {
			LevelManager.Kill( this );
		}
	}
	
	public void FullHealth() {
		SetHealthPoint( GetHealthPointMax() );
	}
	
	public void SetInvincible( float time ) {
		invincible = true;
		
		CancelInvoke( "SetDamageable" );
		Invoke( "SetDamageable", time );
	}
	
	public void SetDamageable() {
		invincible = false;
	}
}
