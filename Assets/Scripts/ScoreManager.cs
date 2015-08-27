using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour {

	public enum Types {
		All,
		Coin,
		Enemy
	};

	Text scoreText;

	private static int score;
	private static Dictionary<Types, int> multipliers = new Dictionary<Types, int>();

	// Use this for initialization
	void Start () {
		scoreText = GetComponent<Text> ();
	}
	
	// Update is called once per frame
	void Update () {
		scoreText.text = score + "$";
	}

	public static void AddPoint(int numberPoint, Types type = Types.All){
		int multiplier;
		if( multipliers.TryGetValue( type, out multiplier ) ) {
			numberPoint*= multiplier;
		}
		else if( Types.All != type && multipliers.TryGetValue( Types.All, out multiplier ) ) {
			numberPoint*= multiplier;
		}
		score += numberPoint;
	}

	public static void AddMultiplier( int multiplier, Types type = Types.All ) {
		int initial;
		if( multipliers.TryGetValue( type, out initial ) ) {
			multiplier = Mathf.Max( multiplier, initial );
			multipliers.Remove( type );
		}

		multipliers.Add( type, multiplier );
	}

	public static void RemoveMultiplier( Types type = Types.All ) {
		multipliers.Remove( type );
	}

	public static void Reset(){
		score = 0;
	}
}
