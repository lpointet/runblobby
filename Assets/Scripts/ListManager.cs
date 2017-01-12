using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[System.Serializable]
public class ItemDescription {
	public Sprite itemImage;
	public string itemName;
	public int itemLevel;
	public int itemMode;
	public ItemType itemType;
}

public class ListManager : MonoBehaviour {

	public static ListManager current;

	public CoinPickup[] coins;
	public Pickup[] powerups;
	public Sprite[] cloudBlock;
	public Sprite[] level;
	public ItemDescription[] item;

	void Awake() {
		if (current == null) {
			current = this;
		} else
			DestroyObject (this);
	}
}
