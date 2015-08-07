using UnityEngine;
using System.Collections;

public class WeaponRotation : MonoBehaviour {

	public Transform follow;

	private Vector3 followPosition;
	private Vector3 vecteurWeapon;
	private float rotaZ;
	private Transform myTransform;

	void Awake() {
		myTransform = transform;
	}

	// Permet à l'arme de suivre quelque chose dans la scène ou la souris (si jamais elle est visible, sinon donne le transform.rotation pour les balles au moins)
	void Update () {
		followPosition = null == follow ? Camera.main.ScreenToWorldPoint(Input.mousePosition) : follow.transform.position;

		vecteurWeapon = followPosition - myTransform.position;
		vecteurWeapon.Normalize ();

		rotaZ = Mathf.Atan2 (vecteurWeapon.y, vecteurWeapon.x) * Mathf.Rad2Deg; // Angle entre l'horizontal et le vecteur calculé précédemment

		myTransform.rotation = Quaternion.Euler (0f, 0f, rotaZ);
	}
}
