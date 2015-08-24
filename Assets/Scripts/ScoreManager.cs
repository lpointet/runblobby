using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour {

	enum Types {
		All,
		Coin,
		Enemy
	};

	Text scoreText;

	private static int score;
	private static Dictionary<int, int> multipliers = new Dictionary<int, int>();

	// Use this for initialization
	void Start () {
		scoreText = GetComponent<Text> ();
	}
	
	// Update is called once per frame
	void Update () {
		scoreText.text = score + "$";
	}

	public static void AddPoint(int numberPoint, int type = (int)Types.All){
		int multiplier;
		if( multipliers.TryGetValue( type, out multiplier ) ) {
			numberPoint*= multiplier;
		}
		score += numberPoint;
	}

	public static void AddMultiplier( int multiplier, int type = (int)Types.All ) {
		int initial;
		if( multipliers.TryGetValue( type, out initial ) ) {
			multiplier = Mathf.Max( multiplier, initial );
			multipliers.Remove( type );
		}

		multipliers.Add( type, multiplier );
	}

	public static void RemoveMultiplier( int type = (int)Types.All ) {
		multipliers.Remove( type );
	}

	public static void Reset(){
		score = 0;
	}
}
