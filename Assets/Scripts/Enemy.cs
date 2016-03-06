using UnityEngine;
using UnityEngine.UI;

public class Enemy : Character {

	/**
	 * Enemy Stats
	 */
	[SerializeField] private float distanceToKill;
	[SerializeField] private int damageToGive;
	[SerializeField] private int pointScore;
	[SerializeField] private string firstName;
	[SerializeField] private string surName;
	/* End of Stats */

	protected Rigidbody2D myRb;
	protected EnemySoundEffect myAudio;
	protected SpriteRenderer myRend;

	public float[] popPosition = new float[2];
	public float[] startPosition = new float[2];
	private float lerpBeforeFight = 0;
	private bool popEnemy = false;

	[Header("Coin Drop")]
	public float frequence = 0.01f;
	public float mediumValue = 1.1f; // Offre une bien plus grande variété de feuilles que 1f
	private CoinPickup[] coins; // Fait référence à ListManager
	private CoinDrop[] possibleCoins;
	[SerializeField] protected LayerMask layerGround; // TODO autre possibilité pour que le paramètre passe aux enfants ?
	private float distanceParcourue = 0;
	private float pointLastDropCheck = 0;
	public float intervalleDrop = 0.5f;
	private float mediumSum; // Somme des mediumValue à chaque pièce qui tombe + 1 (= total optimal futur)
	private float currentSum = 0f; // Somme des value de coin réellement tombées (= total courant)

	/**
	 * Getters & Setters
	 */
	public float GetDistanceToKill() {
		return distanceToKill;
	}

	public int GetDamageToGive() {
		return damageToGive;
	}

	public int GetPointScore() {
		return pointScore;
	}

	public string GetName() {
		return firstName;
	}

	public string GetSurName() {
		return surName;
	}

	public void SetDistanceToKill( float value ) {
		distanceToKill = value;
	}

	public void SetDamageToGive( int value ) {
		damageToGive = value;
	}

	public void SetPointScore( int value ) {
		pointScore = value;
	}

	public void SetName( string value ) {
		name = value;
	}

	public void SetSurName( string value ) {
		surName = value;
	}

	public Vector2 GetStartPosition () {
		return new Vector2 (startPosition [0], startPosition [1]);
	}

	protected void SetPopPosition (Vector2 vector) {
		popPosition [0] = vector.x;
		popPosition [1] = vector.y;
	}

	protected Vector2 GetPopPosition () {
		return new Vector2 (popPosition [0], popPosition [1]);
	}
	/* End of Getters & Setters */

	void OnEnable() {
		Init();
	}
	
	protected override void Awake () {
        base.Awake();

		myRb = GetComponent<Rigidbody2D> ();
		myAudio = GetComponent<EnemySoundEffect> ();
		myRend = GetComponent<SpriteRenderer> ();

		myRend.enabled = false;
	}

	protected override void Init() { // TODO pourquoi c'est appelé deux fois ? Start (voir Character.cs) et OnEnable ?
		base.Init();

		PossibleCoin ();

		// On le place à sa position AVANT début du combat
		SetPopPosition (new Vector2(LevelManager.levelManager.cameraEndPosition - 3.5f, GetStartPosition().y));
		myTransform.position = GetPopPosition ();
		myTransform.rotation = Quaternion.identity;
		popEnemy = true;
		myRend.enabled = true;
	}

	protected override void Update () {
		base.Update();

		if (LevelManager.GetPlayer ().IsDead () || IsDead() || TimeManager.paused)
			return;

		// Déplacement avant le combat
		if (popEnemy) {
			myTransform.position = new Vector2 (Mathf.Lerp(GetPopPosition().x, GetStartPosition().x, lerpBeforeFight), myTransform.position.y);

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
		if( IsDead() ) {
			return;
		}

		if (other.name == "Heros")
			LevelManager.GetPlayer ().Hurt(GetDamageToGive());
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

	// TODO créer un script à part
	public class CoinDrop {
		private CoinPickup coin;
		private int poissonValue;
		private float esperance = 0;
		private int coinValue;

		public CoinDrop(CoinPickup coin, int poissonValue, float esperance) {
			this.coin = coin;
			this.poissonValue = poissonValue;
			this.esperance = esperance;
			coinValue = coin.pointToAdd;
		}

		public void SetEsperance(float value) {
			esperance = value;
		}

		public string GetCoinName() {
			return coin.name;
		}

		public int GetCoinValue() {
			return coinValue;
		}

		public int PourcentPoisson () {
			if (poissonValue > 0)
				return Mathf.Max(0, Mathf.RoundToInt (_StaticFunction.LoiPoisson (poissonValue, esperance, true) * 100));
			else // Si on est sur la dernière valeur du tableau, et donc que poissonValue = 0, la limite haute est toujours 100
				return 100;
		}
	}

	public override void OnKill() {
		ScoreManager.AddPoint (GetPointScore(), ScoreManager.Types.Experience);
		Despawn();
	}

	protected virtual void Despawn() {
		gameObject.SetActive (false);
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
				hit = Physics2D.Raycast (myTransform.position, Vector2.down, 20, layerGround); // On essaye de trouver le sol, sinon on ne fait rien

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

							coin.transform.position = new Vector2 (myTransform.position.x, hit.transform.position.y + 0.25f);
							coin.transform.parent = hit.transform;

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
