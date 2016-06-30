using UnityEngine;
using System.Collections;

public class Character : MonoBehaviour {

	protected Transform myTransform;
	protected SpriteRenderer mySprite;

	/**
	 * Character Stats 
	 */
	[Header("Stats générales")]
	[SerializeField] private int _healthPoint;
	[SerializeField] private int _healthPointMax;
	[SerializeField] private float _moveSpeed;
	[SerializeField] private float _jumpHeight;
	[SerializeField] private float _maxHeight = 7.5f;
	[SerializeField] private float _invincibleTime = 0.5f;

	[SerializeField] private int _defense = 1;
    /* End of Stats */

	protected bool isDead;

	[SerializeField] protected Material invincibleMaterial;
    private bool invincible = false;

    /**
	 * Getters & Setters
	 */
	public int healthPoint {
		get { return _healthPoint; }
		set { _healthPoint = Mathf.Clamp (value, 0, healthPointMax); }
	}
	public int healthPointMax {
		get { return _healthPointMax; }
		set { _healthPointMax = value; }
	}
	public float moveSpeed {
		get { return _moveSpeed; }
		set { _moveSpeed = value; }
	}
	public float jumpHeight {
		get { return _jumpHeight; }
		set { _jumpHeight = value; }
	}
	public float maxHeight {
		get { return _maxHeight; }
		set { _maxHeight = value; }
	}
	public float invincibleTime {
		get { return _invincibleTime; }
		set { _invincibleTime = value; }
	}

	public int defense {
		get { return _defense; }
		set { _defense = value; }
	}

	public bool IsInvincible () { return invincible; }

	public bool IsDead() { return isDead; }
	// Créer 2 setters pour isDead parce que les noms sont plus cool :)
	public void Die() { isDead = true; }
	public void Resurrect() { isDead = false; }

    /* End of Getters & Setters */

    protected virtual void Awake() {
        myTransform = transform;
		mySprite = GetComponent<SpriteRenderer> ();
    }

    void Start() {
		Init();
		maxHeight = Camera.main.orthographicSize + Camera.main.GetComponent<CameraManager> ().yOffset - 1.5f;
	}
	
	protected virtual void Init() {
		// Init health
		FullHealth();
		// Let it live
		Resurrect();
	}

	protected virtual void Update() {
		if( transform.position.y >= maxHeight && !IsDead() ) {
			transform.position = new Vector3( transform.position.x, maxHeight, transform.position.z );
		}
	}
	
	public virtual void OnKill() {
		// On fait le deuil
	}
	
	public virtual void Hurt(int damage) {
		if( IsInvincible () || IsDead() )
			return;
		
		healthPoint -= damage;
		
		if (healthPoint <= 0 && !IsDead())
			LevelManager.Kill( this );

		// Effet visuel de blessure et d'invincibilité
		if (!IsDead ()) {
			StartCoroutine (HurtEffect (invincibleTime));
			SetInvincible (invincibleTime);
		}
	}
	
	public void FullHealth() {
		healthPoint = healthPointMax;
	}
	
	public void SetInvincible( float time ) {
		invincible = true;
		
		CancelInvoke( "SetDamageable" );
		Invoke( "SetDamageable", time * Time.timeScale );
	}
	
	public void SetDamageable() {
		invincible = false;
	}

	protected virtual IEnumerator HurtEffect(float hurtingTime = 0.5f) {
		bool isWhite = true;
		Material ownMaterial = mySprite.material;

		float delaySwitch = 0.1f;
		float timeToSwitch = 0;

		mySprite.material = invincibleMaterial;

		while (hurtingTime > 0) {
			hurtingTime -= TimeManager.deltaTime;

			if (TimeManager.time > timeToSwitch) {
				timeToSwitch = TimeManager.time + delaySwitch;
				mySprite.material = isWhite ? ownMaterial : invincibleMaterial;
				isWhite = !isWhite;
			}

			yield return null;
		}

		// Retour à la "normale"
		mySprite.material = ownMaterial;
	}
}