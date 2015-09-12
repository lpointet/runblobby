using UnityEngine;

public class CheckpointManager : MonoBehaviour {

	private LevelManager levelManager;

	// Use this for initialization
	void Start () {
		levelManager = FindObjectOfType<LevelManager> ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnTriggerEnter2D(Collider2D other){
		if (other.name == "Heros") {
			levelManager.currentCheckPoint = gameObject;
			Debug.Log ("Activated Checkpoint" + transform.position);
		}
	}
}
