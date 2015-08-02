using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ScoreManager : MonoBehaviour {

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

	public static void AddPoint(int numberPoint){
		score += numberPoint;
	}

	public static void Reset(){
		score = 0;
	}
}
