using UnityEngine;

public class Character : MonoBehaviour {
	
	/**
	 * Character Stats 
	 */
	[SerializeField] private int healthPoint;
	[SerializeField] private int healthPointMax;
	[SerializeField] private float moveSpeed;
	[SerializeField] private float jumpHeight;
	[SerializeField] private bool isDead;
	[SerializeField] private float maxHeight = 7.5f;
    /* End of Stats */

    protected Transform myTransform;
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

	public float GetMaxHeight() {
		return maxHeight;
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

	public void SetMaxHeight( float value ) {
		maxHeight = value;
	}
    /* End of Getters & Setters */

    protected virtual void Awake() {
        myTransform = transform;
    }

    void Start() {
		Init();
		SetMaxHeight (Camera.main.orthographicSize + Camera.main.GetComponent<CameraManager> ().yOffset - 1);
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

	protected virtual void Update() {
		if( transform.position.y >= GetMaxHeight() ) {
			transform.position = new Vector3( transform.position.x, GetMaxHeight(), transform.position.z );
		}
	}
}