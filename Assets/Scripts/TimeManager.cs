using UnityEngine;

public class TimeManager : MonoBehaviour {

	public static TimeManager ownTime;

	public static float deltaTime { get; private set; }    	// Correspond à Time.deltaTime en prenant compte le timeScale
	public static float time { get; private set; }        	// Correspond à Time.time en prenant compte le timeScale
	public static bool paused { get; private set; }    		// Définit si l'on est en pause (timeScale = 0) ou non

	void Awake() {
		if (ownTime == null)
			ownTime = FindObjectOfType<TimeManager>();
		
		deltaTime = 0;
		time = 0;
		paused = false;
	}

	void Update() {
		if (Time.timeScale == 0) {
			paused = true;
			deltaTime = 0;
			return;
		}

		paused = false;
		deltaTime = Time.smoothDeltaTime / Time.timeScale;
		time += deltaTime;
	}
}
