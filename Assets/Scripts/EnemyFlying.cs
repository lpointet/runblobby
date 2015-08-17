using UnityEngine;
using System.Collections;

public class EnemyFlying : MonoBehaviour {

	private bool enemyIsHere = false;
	private bool enemyIsOnPlayerY = false;

	private Transform myTransform;
	private Transform player;

	public float flySpeed = 1f;

	public bool isHorizontal = false;	// vole tout droit
	public bool isSinus = false;		// vole en sinus
	public bool isPingPong = false;		// vole en zigzag
	public bool isBoomerang = false;	// revient en volant quand il est au bord de l'écran

	private float enemyPositionY;	// Pour savoir selon qu'elle ligne se diriger selon l'effet
	
	void Start () {
		player = LevelManager.getPlayer ().transform;
		myTransform = transform;
	}

	void Update () {
		if (enemyIsHere && !enemyIsOnPlayerY) {
			// On donne une vitesse ascendante/descendante à l'ennemi dès qu'il apparait, selon sa position au joueur
			if (myTransform.position.y > player.position.y)
				myTransform.Translate (Vector3.down * Time.deltaTime * flySpeed / 2.0f);
			else if (myTransform.position.y < player.position.y)
				myTransform.Translate (Vector3.up * Time.deltaTime * flySpeed / 2.0f);

			// Lorsque l'ennemi est à la meme hauteur que le joueur, il fonce tout droit (arrondi sinon ils se croisent jamais...)
			if(Mathf.Round (myTransform.position.y * 10) / 10.0f == Mathf.Round (player.position.y * 10) / 10.0f) {
				enemyIsOnPlayerY = true;
				enemyPositionY = myTransform.position.y;
			}
		}
		if (enemyIsOnPlayerY) {
			// On donne une vitesse horizontale
			if(isHorizontal)
				myTransform.Translate (Vector3.left * Time.deltaTime * flySpeed);
			// On donne une vitesse sinusoidale
			if(isSinus)
				myTransform.position = new Vector2(myTransform.position.x - Time.deltaTime * flySpeed, enemyPositionY + Mathf.Sin (4 * Time.time));
			// On donne une vitesse pingpongidale
			if(isPingPong)
				myTransform.Translate (Vector3.left * Time.deltaTime * flySpeed);

		}
	}

	void OnBecameVisible() {
		// Déclencher le mouvement de l'ennemi
		enemyIsHere = true;
	}
}
