using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour {
	
	public enum Types {
		All,
		Coin,
		Enemy
	};
	
	public struct Multiplier {
		public int value;
		public float endTime;
	}
	
	public static Dictionary<Types, Multiplier> multipliers = new Dictionary<Types, Multiplier>();

	Text scoreText;

	private static int score;

	// Use this for initialization
	void Start () {
		scoreText = GetComponent<Text> ();
	}
	
	// Update is called once per frame
	void Update () {
		scoreText.text = score + "$";
	}

	public static void AddPoint(int numberPoint, Types type){
		Multiplier multiplier;

		if( multipliers.TryGetValue( type, out multiplier ) ) {
			numberPoint*= multiplier.value;
		}
		else if( Types.All != type && multipliers.TryGetValue( Types.All, out multiplier ) ) {
			numberPoint*= multiplier.value;
		}
		score += numberPoint;
	}
	
	public static void AddMultiplier( int multiplier, Types type, float lifeTime ) {
		Multiplier initial;
		Multiplier newMult;
		
		newMult.value = multiplier;
		newMult.endTime = Time.time + lifeTime;
		
		if( multipliers.TryGetValue( type, out initial ) ) {
			newMult.value = Mathf.Max( multiplier, initial.value );
			multipliers.Remove( type );
		}
		
		multipliers.Add( type, newMult );
	}
	
	public static void MaybeRemoveMultiplier( Types type = Types.All ) {
		Multiplier initial;

		if( multipliers.TryGetValue( type, out initial ) ) {
			if( initial.endTime <= Time.time + 0.05f ) {
				multipliers.Remove( type );
			}
		}
	}

	public static void Reset(){
		score = 0;
	}
}
