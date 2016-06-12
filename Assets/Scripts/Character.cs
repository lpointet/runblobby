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
	[SerializeField] private float maxHeight = 7.5f;
    /* End of Stats */

    protected Transform myTransform;
	protected SpriteRenderer mySprite;
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

	public bool IsInvincible () {
		return invincible;
	}
    /* End of Getters & Setters */

    protected virtual void Awake() {
        myTransform = transform;
		mySprite = GetComponent<SpriteRenderer> ();
    }

    void Start() {
		Init();
		SetMaxHeight (Camera.main.orthographicSize + Camera.main.GetComponent<CameraManager> ().yOffset - 1.5f);
	}
	
	protected virtual void Init() {
		// Init health
		FullHealth();
		// Let it live
		Resurrect();
		// On corrige l'alpha si besoin
		Color tempColor = mySprite.color;
		tempColor.a = 1;
		mySprite.color = tempColor;
	}
	
	public virtual void OnKill() {
		// On fait le deuil
	}
	
	public virtual void Hurt(int damage) {
		if( IsInvincible () || IsDead() )
			return;
		
		SetHealthPoint( GetHealthPoint() - damage );
		
		if (GetHealthPoint() <= 0 && !IsDead())
			LevelManager.Kill( this );

		// Effet visuel de blessure
		if (!IsDead ())
			StartCoroutine (HurtEffect ());
	}
	
	public void FullHealth() {
		SetHealthPoint( GetHealthPointMax() );
	}
	
	public void SetInvincible( float time ) {
		invincible = true;
		
		CancelInvoke( "SetDamageable" );
		Invoke( "SetDamageable", time * Time.timeScale );
	}
	
	public void SetDamageable() {
		invincible = false;
	}

	protected virtual void Update() {
		if( transform.position.y >= GetMaxHeight() && !IsDead() ) {
			transform.position = new Vector3( transform.position.x, GetMaxHeight(), transform.position.z );
		}
	}

	protected virtual IEnumerator HurtEffect() {
		Color tempColor = mySprite.color;
		float flashDelay = 0.1f;
		int flashNumber = 0;
		int flashNumberMax = 4;
		bool increment = false;

		// TODO faire ça dans tous les override
		SetInvincible (flashNumberMax * flashDelay);

		while (flashNumber < flashNumberMax) {
			if (increment)
				tempColor.a += TimeManager.deltaTime / flashDelay;
			else 
				tempColor.a -= TimeManager.deltaTime / flashDelay;

			if (tempColor.a > 1) {
				increment = false;
				flashNumber++;
			}
			else if (tempColor.a < 0.25f)
				increment = true;

			mySprite.color = tempColor;
			yield return null;
		}
		// Retour à la "normale"
		tempColor.a = 1;
		mySprite.color = tempColor;
	}
}