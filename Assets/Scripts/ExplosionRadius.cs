using UnityEngine;
using System.Collections;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(CircleCollider2D))]
public class ExplosionRadius : MonoBehaviour {

	[SerializeField] private float expansionTime = 0.1f;
	[SerializeField] private float stayTime = 0.5f;
	[SerializeField] private float startRadius = 0f;
	[SerializeField] private float normalEndRadius;
	[SerializeField] private float hardEndRadius;
	[SerializeField] private float hellEndRadius;
	[SerializeField] private float arcadeEndRadius;
	private float endRadius;

	[SerializeField] private Color explosionColor;

	[SerializeField] private float normalImpactDamage = 1f;
	[SerializeField] private float hardImpactDamage = 1f;
	[SerializeField] private float hellImpactDamage = 1f;
	[SerializeField] private float arcadeImpactDamage = 1f;
	private float impactDamage;

	private Transform myTransform;
	private SpriteRenderer mySprite;

	private float ratioScaling = 0.25f; // L'image étant sur 64 pixels alors que l'unité est sur 16 pixels, il faut diviser par 4 pour avoir la taille "réelle"
	private float currentExplosionTime = 0;

	private bool startExplosion = false;

	public void StartExplosion () {
		gameObject.SetActive (true);

		myTransform.localScale = Vector2.one * this.startRadius;

		currentExplosionTime = 0;
		startExplosion = true;
	}

	void Awake () {
		myTransform = transform;
		mySprite = GetComponent<SpriteRenderer> ();

		mySprite.color = explosionColor;

		myTransform.localScale = Vector2.zero; // Permet de "cacher" l'explosion
	}

	void OnEnable () {
		// Chargement différent selon la difficulté et le mode
		if (LevelManager.levelManager.IsStory ()) {
			switch (LevelManager.levelManager.GetCurrentDifficulty ()) {
			// Normal
			case 0:
				impactDamage = normalImpactDamage;
				endRadius = normalEndRadius;
				break;
			// Hard
			case 1:
				impactDamage = hardImpactDamage;
				endRadius = hardEndRadius;
				break;
			// Hell
			case 2:
				impactDamage = hellImpactDamage;
				endRadius = hellEndRadius;
				break;
			}
		} else { // Arcade
			impactDamage = arcadeImpactDamage;
			endRadius = arcadeEndRadius;
		}

		startRadius *= ratioScaling;
		endRadius *= ratioScaling;
	}

	void Update () {
		// On ne fait rien tant qu'on n'a pas demandé à commencer l'explosion
		if (!startExplosion)
			return;
		
		// Si le temps d'explosion dépasse le temps d'expansion et d'affichage, on arrête
		if (currentExplosionTime > expansionTime + stayTime)
			EndExplosionEffect ();

		currentExplosionTime += TimeManager.deltaTime;

		// Si le temps d'explosion dépasse le temps d'expansion, on n'agrandit plus le cercle, et on l'affadit
		if (currentExplosionTime > expansionTime) {
			Color tempColor = mySprite.color;
			tempColor.a = Mathf.Lerp (1, 0, (currentExplosionTime - expansionTime) / stayTime / 1.25f);
			mySprite.color = tempColor;

			return;
		}
		//TODO faire un collider aussi
		myTransform.localScale = Vector2.one * Mathf.Lerp (startRadius, endRadius, currentExplosionTime / expansionTime);
	}

	private void EndExplosionEffect () {
		startExplosion = false;
		myTransform.localScale = Vector2.zero;

		gameObject.SetActive (false);
	}

	void OnTriggerEnter2D (Collider2D other) {
		if (other.name == "Heros") {
			LevelManager.player.Hurt (impactDamage);
		}
	}
}
