using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Enemy : Character {

	protected Rigidbody2D myRb;
	protected EnemySoundEffect myAudio;
	protected Animator myAnim;

	/**
	 * Enemy Stats
	 */
	[Header("Stats ennemies")]
	[SerializeField] private float _timeToKill;
	[SerializeField] private int damageToGive;
	[SerializeField] private string _firstName;
	[SerializeField] private string _surName;
	/* End of Stats */

	[SerializeField] protected float[] popPosition = new float[2];
	[SerializeField] protected float[] startPosition = new float[2];
	private float lerpBeforeFight = 0;
	private bool popEnemy = false;

	[Header("Coin Drop")]
	[SerializeField] protected float frequence = 0.01f;
	[SerializeField] protected float mediumValue = 1.1f; // Offre une bien plus grande variété de feuilles que 1f
	private CoinPickup[] coins; // Fait référence à ListManager
	private CoinDrop[] possibleCoins;
	[SerializeField] protected bool coinToGround; // True = les pièces vont au sol, False = les pièces restent là où elles sont
	[SerializeField] protected LayerMask layerGround;
	private float distanceParcourue = 0;
	private float pointLastDropCheck = 0;
	[SerializeField] protected float intervalleDrop = 0.5f;
	private float mediumSum; // Somme des mediumValue à chaque pièce qui tombe + 1 (= total optimal futur)
	private float currentSum = 0f; // Somme des value de coin réellement tombées (= total courant)

	/**
	 * Getters & Setters
	 */
	public float timeToKill {
		get { return _timeToKill; }
		set { _timeToKill = value; }
	}
	public string firstName {
		get { return _firstName; }
		set { _firstName = value; }
	}
	public string surName {
		get { return _surName; }
		set { _surName = value; }
	}
	/* End of Getters & Setters */

	void OnEnable() {
		Init();
	}
	
	protected override void Awake () {
        base.Awake();

		myRb = GetComponent<Rigidbody2D> ();
		myAudio = GetComponent<EnemySoundEffect> ();
		myAnim = GetComponent<Animator> ();

		mySprite.enabled = false;
	}

	protected override void Init() {
		base.Init();

		PossibleCoin ();

		// On le place à sa position AVANT début du combat
		popPosition[0] = CameraManager.cameraEndPosition;
		popPosition[1] = startPosition[1];

		myTransform.position = new Vector2(popPosition[0], popPosition[1]);
		myTransform.rotation = Quaternion.identity;

		popEnemy = true;
		mySprite.enabled = true;

		// Ajout du temps des talents
		timeToKill += GameData.gameData.playerData.talent.bossLengthBonus * GameData.gameData.playerData.talent.bossLengthBonusPointValue;

		// Diminution de la défense de l'ennemi (si talent + pickup récupéré)
		defense = Mathf.Max (0, defense + LevelManager.reduceEnemyDefense);

		// Diminution de la vie de l'ennemi (si talent + pickup récupéré)
		healthPoint = Mathf.Max (1, healthPoint + LevelManager.reduceEnemyHealth);
	}

	protected override void Update () {
		base.Update();

		if (LevelManager.player.IsDead () || IsDead() || TimeManager.paused)
			return;

		// Déplacement avant le combat
		if (popEnemy) {
			myTransform.position = new Vector2 (Mathf.Lerp(popPosition[0], startPosition[0], lerpBeforeFight), myTransform.position.y);

			lerpBeforeFight += TimeManager.deltaTime / LevelManager.levelManager.enemySpawnDelay;

			if (myTransform.position.x <= startPosition [0])
				popEnemy = false;
			
			return;
		}

		// Laché de feuilles aléatoires
		MoneyDrop ();
	}

	void OnTriggerEnter2D(Collider2D other){
		// Si l'ennemi est déjà mort, il ne peut plus rien faire...
		if( IsDead() )
			return;

		if (other.name == "Heros")
			LevelManager.player.Hurt(damageToGive, sharp);
	}

	public override void Hurt(float damage, int penetration = 0, bool ignoreDefense = false, Character attacker = null) {
		if( IsDead() || LevelManager.player.IsDead() )
			return;

		base.Hurt (damage, penetration, ignoreDefense, attacker);

		if (myAudio != null) {
			if (!IsDead ())
				myAudio.HitSound ();
			else
				myAudio.DeathSound ();
		}
	}

	public override void OnKill() {
		GainExp ();
		Despawn ();
	}

	protected virtual void GainExp() {
		int pointDistance = Mathf.CeilToInt(LevelManager.levelManager.GetEnemyTimeToKill ());
		int pointBoss = 10 + LevelManager.levelManager.GetCurrentLevel (); // XP pour les boss, par défaut, à changer pour les boss finaux
		ScoreManager.AddPoint (pointDistance + pointBoss, ScoreManager.Types.Experience);
	}

	protected virtual void Despawn() {
		gameObject.SetActive (false);
	}

	private void PossibleCoin() {
		coins = ListManager.current.coins;

		frequence = Mathf.Clamp01 (frequence);

		// On choisit au maximum 5 pièces autour de la valeur moyenne des drops souhaités
		// Exemple : mediumValue = 7, pièce directement en dessous 5, on prend 1, 2, 5, 10, 20
		int mediane = coins.Length - 1; // Au pire si on ne trouve rien dans la boucle, on sait que la médiane sera sur la dernière pièce
		int tabSize = 3; // Au pire si on ne trouve rien dans la boucle, on sait que le tableau des pièces possibles fera 3
		int firstElement = 0; // Premier élément du tableau de "coins" à mettre dans "possibleCoins"

		for (int i = 0; i < coins.Length; i++) {
			// Si la différence entre la valeur souhaitée et la valeur de la pièce est négative (strictement), on doit retenir la valeur précédente comme pièce médiane
			if (mediumValue - coins [i].pointToAdd < 0) {
				mediane = i == 0 ? 0 : (i - 1); // S'assurer qu'on prenne au pire la première pièce
				tabSize = mediane > 1 ? 5 : (mediane + 3);
				if (mediane == coins.Length - 2) // On modifie juste la valeur si on tombe sur l'avant-dernier élément
					tabSize = 4;
				break;
			}
		}
		// Calcul de la position du premier élément dans la liste totale des pièces
		if (mediane > 2)
			firstElement = mediane - 2;
		else
			firstElement = 0;
		// On créé le tableau des pièces utilisables
		possibleCoins = new CoinDrop[tabSize];

		for (int i = 0; i < tabSize; i++) {
			// On calcule les valeurs 'moyennes' entre les valeurs exactes des pièces
			// Ceci permet d'utiliser la loi de Poisson pour le calcul de probabilité d'apparition des pièces
			// Exemple : entre 5 et 10, on trouve la valeur 7 qui correspond aux proba cumulées de faire apparaître un 5
			// On calculera donc la loi de Poisson pour un 7 à la place du 5 (solution la plus "fiable")
			int poissonValue;
			if (i < tabSize - 1) {
				poissonValue = Mathf.FloorToInt ((coins [firstElement + i].pointToAdd + coins [firstElement + i + 1].pointToAdd) / 2f);
			} else {
				poissonValue = 0;
			}
			// On initialise chaque élément du tableau
			possibleCoins [i] = new CoinDrop (coins [firstElement + i], poissonValue, mediumValue);
		}

		mediumSum = mediumValue; // La première valeur souhaitée est celle de la valeur moyenne
	}
		
	/* Fonction qui permet de déposer des feuilles à la suite du boss
	 * Dépend de la difficulté, de la fréquence souhaitée
	 * Possibilité de régler la valeur moyenne des feuilles qui tombent */
	protected void MoneyDrop() {
		distanceParcourue += LevelManager.levelManager.GetLocalDistance ();

		if (distanceParcourue > pointLastDropCheck + intervalleDrop) { // On ne propose de poser une feuille que tous les intervalleDrop parcourus
			pointLastDropCheck = distanceParcourue;

			if (Random.Range (0f, 1f) <= frequence) { // On pose une pièce en respectant la fréquence

				RaycastHit2D hit;
				hit = Physics2D.Raycast (myTransform.position, Vector2.down, 25, layerGround); // On essaye de trouver le sol, sinon on ne fait rien

				if (hit.collider != null) {
					// On calcule quelle type de pièce doit tomber en fonction de ce qui est souhaité en moyenne et des pièces précédentes
					int cannePeche = Random.Range(1, 101); // entre 1 et 100 inclus

					for (int i = 0; i < possibleCoins.Length; i++) {
						// On change l'espérance = ce qu'on souhaite comme moyenne pour la prochaine somme avec la somme courante
						// On ajoute un peu d'aléatoire pour éviter de se retrouver dans une situation bloquante
						possibleCoins [i].SetEsperance (mediumSum - currentSum + Random.Range((float)-mediumValue, (float)mediumValue));
						// On invoque la première pièce dont le calcul de Poisson cumulé dépasse ou vaut le nombre tiré au sort
						if (possibleCoins [i].PourcentPoisson () >= cannePeche) {
							GameObject coin = PoolingManager.current.Spawn (possibleCoins[i].GetCoinName());

							coin.gameObject.SetActive (true);

							coin.transform.parent = hit.transform;

							if (coinToGround) // Si on veut que les pièces touchent le sol
								coin.transform.position = new Vector2 (myTransform.position.x, hit.transform.position.y + 0.25f);
							else // Sinon on les laisse sur place
								coin.transform.position = new Vector2 (myTransform.position.x, myTransform.position.y);

							mediumSum += mediumValue;
							currentSum += possibleCoins [i].GetCoinValue ();

							break;
						}
					}
				}
			}
		}
	}
}