using UnityEngine;

public class UIManager : MonoBehaviour {

	public GameObject PauseUI;
	public GameObject cdObject;
	private Animator cdAnim;
	
	private bool paused = false;

	void Start() {
		PauseUI.SetActive (false);
		cdAnim = cdObject.GetComponent<Animator> ();
	}
	
	void Update() {
		//* MENU PAUSE
		if (Input.GetButtonDown ("Pause")) {
			paused = !paused;

			if (paused) {
				PauseUI.SetActive (true);
				Time.timeScale = 0;
			} else {
				PauseUI.SetActive(false);
				Time.timeScale = 1;
			}
		}
		//* FIN MENU PAUSE
	}
	
	public void ResumeGame() {
		cdObject.SetActive (true);
		cdAnim.SetBool ("powerOn", true);
		PauseUI.SetActive(false);
		paused = false;
		// A la fin de l'animation, le timeScale redevient 1
		// S'il faut passer un paramètre de timeScale (si jamais il n'est pas à 1), il faudra passer un "faux" paramètre d'animator
	}
	
	public void MainMenu() {
		
	}
	
	public void Quit() {
		Application.Quit ();
	}
}
