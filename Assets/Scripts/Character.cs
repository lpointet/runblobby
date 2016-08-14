using UnityEngine;
using System.Collections;

public class Character : MonoBehaviour {

	protected Transform myTransform;
	protected SpriteRenderer mySprite;

	/**
	 * Character Stats 
	 */
	[SerializeField] private int _healthPoint;

	[Header("Stats générales")]
	[SerializeField] private int _healthPointMax;
	[SerializeField] private int _defense;
	[SerializeField] private int _dodge;
	[SerializeField] private int _sendBack;
	[SerializeField] private int _reflection;
	[SerializeField] private float _invulnerabilityTime = 0.5f;
	[SerializeField] private int _vampirisme;
	[SerializeField] private int _regeneration;
	[SerializeField] private int _attack;
	[SerializeField] private int _attackDelay;
	[SerializeField] private int _attackSpeed;
	[SerializeField] private int _criticalHit;
	[SerializeField] private int _criticalPower;
	[SerializeField] private int _sharp;
	[SerializeField] private int _machineGun;
	[SerializeField] private int _shotDouble;
	[SerializeField] private int _shotWidth;
	[SerializeField] private int _shotRemote;

	[SerializeField] private float _moveSpeed;
	[SerializeField] private float _jumpHeight;
	[SerializeField] private float _maxHeight = 7.5f;
    /* End of Stats */

	protected bool isDead;

	protected Material initialMaterial;
	[SerializeField] protected Material invincibleMaterial;
    private bool invincible = false;

	private float distanceBeforeRegeneration = 0;

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
	public int defense {
		get { return _defense; }
		set { _defense = value; }
	}
	public int dodge {
		get { return _dodge; }
		set { _dodge = value; }
	}
	public int sendBack {
		get { return _sendBack; }
		set { _sendBack = value; }
	}
	public int reflection {
		get { return _reflection; }
		set { _reflection = value; }
	}
	public float invulnerabilityTime {
		get { return _invulnerabilityTime; }
		set { _invulnerabilityTime = value; }
	}
	public int vampirisme {
		get { return _vampirisme; }
		set { _vampirisme = value; }
	}
	public int regeneration {
		get { return _regeneration; }
		set { _regeneration = value; }
	}
	public int attack {
		get { return _attack; }
		set { _attack = value; }
	}
	public int attackDelay {
		get { return _attackDelay; }
		set { _attackDelay = value; }
	}
	public int attackSpeed {
		get { return _attackSpeed; }
		set { _attackSpeed = value; }
	}
	public int criticalHit {
		get { return _criticalHit; }
		set { _criticalHit = value; }
	}
	public int criticalPower {
		get { return _criticalPower; }
		set { _criticalPower = value; }
	}
	public int sharp {
		get { return _sharp; }
		set { _sharp = value; }
	}
	public int machineGun {
		get { return _machineGun; }
		set { _machineGun = value; }
	}
	public int shotDouble {
		get { return _shotDouble; }
		set { _shotDouble = value; }
	}
	public int shotWidth {
		get { return _shotWidth; }
		set { _shotWidth = value; }
	}
	public int shotRemote {
		get { return _shotRemote; }
		set { _shotRemote = value; }
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

	public bool IsInvincible () { return invincible; }

	public bool IsDead() { return isDead; }
	// Créer 2 setters pour isDead parce que les noms sont plus cool :)
	public void Die() { isDead = true; }
	public void Resurrect() { isDead = false; }

    /* End of Getters & Setters */

    protected virtual void Awake() {
        myTransform = transform;
		mySprite = GetComponent<SpriteRenderer> ();
		initialMaterial = mySprite.material;
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
		// Pour ne pas aller plus haut que le haut de l'écran
		if( transform.position.y >= maxHeight && !IsDead() ) {
			transform.position = new Vector3( transform.position.x, maxHeight, transform.position.z );
		}

		if (IsDead () || TimeManager.paused)
			return;
		
		// Régénération
		if (healthPoint < healthPointMax)
			distanceBeforeRegeneration += LevelManager.levelManager.GetLocalDistance () * regeneration / 2000f;

		if (distanceBeforeRegeneration > 1) {
			distanceBeforeRegeneration--;
			healthPoint++;
			StartCoroutine (UIManager.uiManager.CombatText (myTransform, "+1", LogType.heal));
		}
	}
	
	public virtual void OnKill() {
		// On fait le deuil
	}
	
	public virtual void Hurt(float damage, int penetration = 0, bool ignoreDefense = false, Character attacker = null) {
		if( IsInvincible () || IsDead() )
			return;

		float finalDamage;		// Les dégâts après calcul
		int applicationDamage;	// Les dégâts arrondis finaux

		// Dégâts
		if (ignoreDefense)
			finalDamage = damage;
		else {
			// Tentative d'esquive des dégâts
			if (Random.Range (0, 100) < dodge) {
				StartCoroutine (UIManager.uiManager.CombatText (myTransform, "dodge", LogType.special));
				Debug.Log (string.Format("{0} dodge attack", name));
				return;
			}

			// Calcul des dégâts subis en fonctions de la défense et de la pénétration ennemie
			float damageAfterDefense = damage * (10 / (float)(10 + defense * (1 - penetration / 100.0f)));
			finalDamage = Mathf.Max (1, damageAfterDefense);

			// Tentative de renvoi des dégâts
			if (attacker != null && attacker != this) {
				if (Random.Range (0, 100) < reflection) {
					attacker.Hurt (finalDamage, 0, true);

					StartCoroutine (UIManager.uiManager.CombatText (attacker.transform, finalDamage.ToString ("-0 thorn"), LogType.damage));
					Debug.Log (string.Format("Damage return to {0}", attacker.name));
				}
			}
		}

		// Calcul de coup critique (les renvois de dégâts ne prennent pas en compte le critique)
		bool criticalHit = false;
		if (attacker != null && Random.Range (0, 100) < attacker.criticalHit) {
			finalDamage *= attacker.criticalPower / 100f;
			criticalHit = true;
		}

		// Application des dégâts
		applicationDamage = Mathf.RoundToInt (finalDamage);
		healthPoint -= applicationDamage;

		// Vampirisme de l'attaquant
		if (attacker != null && Random.Range (0, 100) < attacker.vampirisme) {
			attacker.healthPoint++;
			StartCoroutine (UIManager.uiManager.CombatText (attacker.transform, "+1 vampire", LogType.heal));
			Debug.Log (string.Format("{0} sucked one HP  on {1}", attacker.name, name));
		}

		// Affichage
		#if UNITY_EDITOR
		if (attacker != null)
			Debug.Log (string.Format("{0} has been hit by {1} for {2} damage ({3} base damage)", name, attacker.name, applicationDamage, damage));
		else
			Debug.Log (string.Format("{0} has been hit for {1} damage", name, applicationDamage));
		#endif

		if (criticalHit)
			StartCoroutine (UIManager.uiManager.CombatText (myTransform, string.Format("-{0} x{1}", applicationDamage, attacker.criticalPower / 100f), LogType.damage));
		else
			StartCoroutine (UIManager.uiManager.CombatText (myTransform, applicationDamage.ToString ("-0"), LogType.damage));
		
		if (healthPoint <= 0 && !IsDead())
			LevelManager.Kill( this );

		// Effet visuel de blessure et d'invincibilité
		if (!IsDead () && !IsInvincible ()) {
			StartCoroutine (HurtEffect (invulnerabilityTime));
			SetInvincible (invulnerabilityTime);
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

		float delaySwitch = 0.1f;
		float timeToSwitch = 0;

		mySprite.material = invincibleMaterial;

		while (hurtingTime > 0) {
			hurtingTime -= TimeManager.deltaTime;

			if (TimeManager.time > timeToSwitch) {
				timeToSwitch = TimeManager.time + delaySwitch;
				mySprite.material = isWhite ? initialMaterial : invincibleMaterial;
				isWhite = !isWhite;
			}

			yield return null;
		}

		// Retour à la "normale"
		mySprite.material = initialMaterial;
	}
}