using UnityEngine;

public class ShieldBall : MonoBehaviour {

	private Transform myTransform;
	private Transform parentTransform;

	void Start () {
		myTransform = transform;
		parentTransform = transform.parent;
	}

	void Update () {
		if (LevelManager.player.permanentShield > 0)
			myTransform.localRotation = Quaternion.Inverse(parentTransform.rotation);
		
		myTransform.localScale = Vector3.one * (0.875f + myTransform.position.z / 8f);

		if (myTransform.position.z < 0)
			GetComponent<SpriteRenderer> ().sortingLayerName = "Player";
		else
			GetComponent<SpriteRenderer> ().sortingLayerName = "Foreground";
	}
}
