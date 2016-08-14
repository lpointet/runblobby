using UnityEngine;
using System.Collections.Generic;

public class PermanentShield : MonoBehaviour {

	[SerializeField] private int maxNumberOfBall = 10;
	[SerializeField] private ShieldBall typeOfBall;

	private Transform myTransform;
	private List<ShieldBall> listOfBall;

	[SerializeField] private float timeToRotate;
	[SerializeField] private float distanceFromCenter = 1f;
	public float rotationSpeed { get; private set; }
	private Vector2 center;

	void Awake () {
		myTransform = transform;
	}

	void Start () {
		listOfBall = new List<ShieldBall> ();
		rotationSpeed = 360 / timeToRotate; // Un tour complet en timeToRotate secondes
		center = myTransform.localPosition;

		// Maximum de balles issu des talents
		maxNumberOfBall = Mathf.RoundToInt (GameData.gameData.playerData.talent.shieldDef * GameData.gameData.playerData.talent.shieldDefPointValue);
	}

	void Update () {
		// On fait tourner le contenant des balles
		if (LevelManager.player.permanentShield > 0) {
			//myTransform.Rotate (Vector3.back * (TimeManager.deltaTime * rotationSpeed));
			myTransform.Rotate (Vector3.one * (TimeManager.deltaTime * rotationSpeed));
		}
	}

	public void CreateShield (int numberOfHit) {
		RefreshShield (numberOfHit);
	}

	public void RefreshShield (int newNumberOfHit) {
		// On compte le nombre de balles qu'il faut ajouter, sans dépasser le max possible
		int finalNumberOfBall = Mathf.Min (maxNumberOfBall, listOfBall.Count + newNumberOfHit);
		int ballToAdd = finalNumberOfBall - listOfBall.Count;

		// On ajoute au compteur de "shield" du joueur la valeur ajoutée
		LevelManager.player.permanentShield += ballToAdd;

		// Pour chaque nouvelle balle à ajouter, on l'active
		for (int i = 0; i < ballToAdd; i++) {
			GameObject shieldBall = PoolingManager.current.Spawn ("ShieldBall");

			if (shieldBall != null) {
				shieldBall.transform.parent = myTransform;
				shieldBall.transform.rotation = Quaternion.identity; // Permet d'avoir les boules toujours dans le même sens

				shieldBall.SetActive (true);

				listOfBall.Add (shieldBall.GetComponent<ShieldBall> ());
			}
		}

		OrderBalls (); // Organisation des balles
	}

	public void HitShield (int numberOfHit) {
		// Si on essaye d'enlever plus de balles qu'il n'en existe
		if (numberOfHit > listOfBall.Count)
			return;

		// On retire le "shield" au joueur
		LevelManager.player.permanentShield -= numberOfHit;

		// On désactive chaque balle touchée
		for (int i = 0; i < numberOfHit; i++) {
			listOfBall [listOfBall.Count - 1].transform.parent = PoolingManager.pooledObjectParent;
			listOfBall [listOfBall.Count - 1].gameObject.SetActive (false);
			listOfBall.RemoveAt (listOfBall.Count - 1);
		}

		OrderBalls (); // Organisation des balles
	}

	private void OrderBalls () {
		// On place les balles en cercle selon leur nombre
		for (int i = 0; i < listOfBall.Count; i++) {
			float deltaAngle = i * 2 * Mathf.PI / (float)listOfBall.Count;
			listOfBall [i].transform.localPosition = new Vector2 (center.x + Mathf.Sin (deltaAngle) * distanceFromCenter, center.y + Mathf.Cos (deltaAngle) * distanceFromCenter);
		}
	}
}