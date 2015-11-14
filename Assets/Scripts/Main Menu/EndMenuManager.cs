using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Collections;

public class EndMenuManager : MonoBehaviour {

	public Text distance;
	public Text money;
	public Text experience;
	public Text playerLevel;

	private SFXMenu sfxSound;
	
	void OnEnable() {
		distance.text = Mathf.RoundToInt( LevelManager.levelManager.GetDistanceTraveled() ).ToString();
		money.text = Mathf.RoundToInt( ScoreManager.GetScore() ).ToString();
	}

	public void Rejouer_Click() {
		Application.UnloadLevel(1);
		Application.LoadLevel (Application.loadedLevel);
	}
}
