﻿using UnityEngine;

public class EnemyFlying : MonoBehaviour {

	private bool enemyIsHere = false;
	private bool enemyIsOnPlayerY = false;

	private Transform myTransform;
	private Transform player;

	private AudioSource sound;

	public float flySpeed = 1f;
	
	public bool isSinus = false;		// vole en sinus
	public bool isPingPong = false;		// vole en zigzag
	public bool isBoomerang = false;	// revient en volant quand il est au bord de l'écran

	private float enemyPositionY;	// Pour savoir selon qu'elle ligne se diriger selon l'effet
	private float mouvement = 0;	// Permet de toujours commencer de la meme façon le mouvement non-linéaire

	bool isOver;
	bool isOverPrevious;

	void Start () {
		sound = GetComponent<AudioSource> ();
		player = LevelManager.GetPlayer ().transform;
		myTransform = transform;
	}

	void Update () {
		if (enemyIsHere && !enemyIsOnPlayerY) {
			
			// On fait bouger l'ennemi vers le bas ou vers le haut, selon sa position par rapport au joueur
			if (isOver)
				myTransform.Translate (new Vector2(-1,-2).normalized * Time.fixedDeltaTime * flySpeed / 2.0f);
			else
				myTransform.Translate (new Vector2(-1,2).normalized * Time.fixedDeltaTime * flySpeed / 2.0f);
			
			IsOverPlayer ();
			
			// Si, par rapport à la dernière frame, il est toujours au meme endroit, on ne fait rien
			if (isOver != isOverPrevious) {
				enemyIsOnPlayerY = true;
				enemyPositionY = myTransform.position.y;
			}
			
			isOverPrevious = isOver;
		}

		if (enemyIsOnPlayerY) {
			if(isSinus || isPingPong) {
				if(isSinus) // On donne une vitesse sinusoidale
					myTransform.position = new Vector2(myTransform.position.x - Time.deltaTime * flySpeed, enemyPositionY + Mathf.Sin (2 * flySpeed * mouvement) / 2.0f);
				else if (isPingPong) // On donne une vitesse pingpongidale
					myTransform.position = new Vector2(myTransform.position.x - Time.deltaTime * flySpeed, enemyPositionY + Mathf.PingPong (flySpeed / 2.0f * mouvement, 0.5f));

				mouvement += Time.deltaTime;
			}
			// On donne une vitesse horizontale
			else
				myTransform.Translate (Vector3.left * Time.deltaTime * flySpeed);
		}
	}

	private void IsOverPlayer() {
		if(myTransform.position.y > player.position.y)
			isOver = true;
		else
			isOver = false;
	}

	void OnBecameVisible() {
		IsOverPlayer ();
		isOverPrevious = isOver;

		// Déclencher le mouvement de l'ennemi
		enemyIsHere = true;

		if (sound)
			sound.Play ();
	}

	void OnBecameInvisible() {
		if (sound)
			sound.Stop ();
	}
}
