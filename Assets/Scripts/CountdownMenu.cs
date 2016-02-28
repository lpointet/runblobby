using UnityEngine;

public class CountdownMenu : MonoBehaviour {

	private Animator myAnim;

	void Awake() {
		myAnim = GetComponent<Animator> ();
	}

	private void EndAnimStartTime() {
		myAnim.SetBool ("powerOn", false);
		Time.timeScale = UIManager.uiManager.GetTimeScale();
		gameObject.SetActive (false);
	}
}
