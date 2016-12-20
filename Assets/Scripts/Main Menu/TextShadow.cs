using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class TextShadow : MonoBehaviour {

	[SerializeField] private Text originalText;

	private Text shadowText;

	void Start () {
		shadowText = gameObject.GetComponent<Text> ();
	}

	void LateUpdate () {
		shadowText.text = originalText.text;
	}
}
