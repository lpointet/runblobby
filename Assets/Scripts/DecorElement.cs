using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DecorElement : MonoBehaviour {
	
	public int weight;
	public int size { get; private set; }

	void Awake () {
		size = Mathf.CeilToInt(GetComponent<SpriteRenderer> ().bounds.size.x);
	}
}
