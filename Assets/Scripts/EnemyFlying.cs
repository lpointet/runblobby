using UnityEngine;
using System.Collections;

public class EnemyFlying : MonoBehaviour {

	private bool enemyIsHere = false;
	private bool enemyIsOnPlayerY = false;

	private Transform myTransform;
	private Transform player;

	public float flySpeed = 1f;
	
	public bool isSinus = false;		// vole en sinus
	public bool isPingPong = false;		// vole en zigzag
	public bool isBoomerang = false;	// revient en volant quand il est au bord de l'écran

	private float enemyPositionY;	// Pour savoir selon qu'elle ligne se diriger selon l'effet
	private float mouvement = 0;	// Permet de toujours commencer de la meme façon le mouvement non-linéaire
	
	void Start () {
		player = LevelManager.getPlayer ().transform;
		myTransform = transform;
	}

	void Update () {
		if (enemyIsHere && !enemyIsOnPlayerY) {
			// On donne une vitesse ascendante/descendante à l'ennemi dès qu'il apparait, selon sa position au joueur
			// Puis, dès qu'on dépasse le joueur dans un sens ou l'autre, on considère etre à sa hauteur
			if (myTransform.position.y > player.position.y) {
				myTransform.Translate (Vector3.down * Time.deltaTime * flySpeed / 2.0f);
				if(myTransform.position.y <= player.position.y ) {
					enemyIsOnPlayerY = true;
					enemyPositionY = myTransform.position.y;
				}
			}
			else if (myTransform.position.y < player.position.y) {
				myTransform.Translate (Vector3.up * Time.deltaTime * flySpeed / 2.0f);
				if(myTransform.position.y >= player.position.y ) {
					enemyIsOnPlayerY = true;
					enemyPositionY = myTransform.position.y;
				}
			}
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

	void OnBecameVisible() {
		// Déclencher le mouvement de l'ennemi
		enemyIsHere = true;
	}
}
