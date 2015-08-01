using UnityEngine;
using System.Collections;

public class WeaponRotation : MonoBehaviour {

	// Permet à l'arme de suivre la souris (si jamais elle est visible, sinond donne le transform pour les balles au moins)
	void Update () {
		Vector3 vecteurWeapon = Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position;
		vecteurWeapon.Normalize ();

		float rotaZ = Mathf.Atan2 (vecteurWeapon.y, vecteurWeapon.x) * Mathf.Rad2Deg; // Angle entre l'horizontal et le vecteur calculé précédemment
		transform.rotation = Quaternion.Euler (0f, 0f, rotaZ);
	}
}
