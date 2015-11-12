using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Collections;

public class EndMenuManager : MonoBehaviour {

	private SFXMenu sfxSound;

	public void Rejouer_Click() {
		Application.UnloadLevel(1);
		Application.LoadLevel (Application.loadedLevel);
	}
}
