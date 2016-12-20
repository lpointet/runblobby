using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public enum ItemType { weapon, shield, helm, perfume };
public enum ItemStatName { attack, criticalHit, criticalPower, healthPoint, dodge, reflect, defense, thorn, regeneration, speed };

public class ItemButton : MonoBehaviour {

	// Variables pour les contrôles des prérequis
	private int distanceLimitPerfume = 5000;
	private float ratioHealthShield = 0.75f;
	private float ratioScoreHelm = 0.8f;

	public int itemNumber { get; private set; }			// ID unique de l'objet = level requis * 100 + mode de difficulté (0 = normal, 1 = hard, 2 = hell, 9 = arcade)
	private Equipment equipment;						// GameData.gameData.playerData.equipment

	private bool equipped = false;						// TRUE lorsque l'objet est celui actuellement équipé
	private bool selected = false;						// TRUE lorsque l'objet est sélectionné

	[Header("Appearance")]
	[SerializeField] private Color disableColor;		// Tant que l'objet n'est pas activé (prérequis non remplis)
	[SerializeField] private Color disableBGColor;		// BG tant que l'objet n'est pas activé (prérequis non remplis)
	[SerializeField] private Color activeBGColor;		// BG quand l'objet est activé
	[SerializeField] private Color selectBGColor;		// BG quand l'object est sélectionné (mais non équipé)
	[SerializeField] private Color equippedBGColor;		// BG quand l'objet est équipé

	[SerializeField] private Color textBonusColor;		// Texte des delta stats si meilleures stats
	[SerializeField] private Color textMalusColor;		// Texte des delta stats si moins bonnes stats
	[SerializeField] private Color textNeutralColor;	// Texte des delta stats si mêmes stats

	[SerializeField] private string itemName;			// Nom de l'objet
	[SerializeField][TextArea(3,10)] private string textDescription;	// Description textuelle de l'objet
	public Image itemImage;								// Image de l'objet
	private Image itemBG;								// Background de l'objet

	[Header("Technical")]
	public ItemType itemType;							// Type de l'objet
	[SerializeField] private int levelNumber;			// Level dans lequel obtenir l'objet
	[SerializeField] private int levelMode;				// Mode dans lequel obtenir l'objet (0 = normal, 1 = hard, 2 = hell, 9 = arcade)
	[SerializeField] private ItemStatName[] bonusStatName;	// Stats à améliorer
	[SerializeField] private int[] bonusStatValue;		// Valeur à ajouter à la stat
	[SerializeField] private int weight;				// Poids de l'objet

	void OnValidate () {
		name = itemName.Replace (" ", "");

		if (bonusStatValue.Length != bonusStatName.Length)
			bonusStatValue = new int[bonusStatName.Length];
	}

	public bool IsSelected () { return selected; }
	public bool IsEquipped () { return equipped; }
	public void EquipItemAtStart () { equipped = true; }

	public bool IsAvailable () {
		// Contrôle sur le levelMode (s'il vaut 0, c'est qu'à priori on n'a rien renseigné)
		if (levelMode == 0)
			return false;

		// Contrôle sur le nombre de level existant
		if (levelNumber > GameData.gameData.playerData.levelData.Count)
			return false;
		
		// Contrôle des prérequis à spécifier par type d'item
		switch (itemType) {
		case ItemType.weapon:
			switch (levelMode) {
			case 1:
			case 2:
				// On vérifie que le joueur a tué le boss du niveau dans le mode
				if (!GameData.gameData.playerData.levelData [levelNumber - GameData.gameData.firstLevel].storyData [levelMode].isBossDead)
					return false;
				break;
			}
			break;
		case ItemType.shield:
			switch (levelMode) {
			case 1:
			case 2:
				// On vérifie que le joueur n'est jamais descendu sous les "ratioHealthShield" HP pendant le niveau
				if (GameData.gameData.playerData.levelData [levelNumber - GameData.gameData.firstLevel].storyData [levelMode].healthRatioRecord < ratioHealthShield)
					return false;
				break;
			}
			break;
		case ItemType.helm:
			switch (levelMode) {
			case 1:
			case 2:
				// On vérifie que le joueur a récupéré au moins "ratioScoreHelm" des feuilles sur le niveau
				if (GameData.gameData.playerData.levelData [levelNumber - GameData.gameData.firstLevel].storyData [levelMode].scoreRatioRecord < ratioScoreHelm)
					return false;
				break;
			}
			break;
		case ItemType.perfume:
			// On vérifie que le joueur a parcouru au moins "distanceLimitPerfume"m en mode Arcade
			if (levelMode == 9 && GameData.gameData.playerData.levelData [levelNumber - GameData.gameData.firstLevel].arcadeData.distanceRecord < distanceLimitPerfume)
				return false;
			break;
		}

		return true;
	}

	public bool IsWearable () {
		int maxWeight = Mathf.RoundToInt (GameData.gameData.playerData.talent.backPack * GameData.gameData.playerData.talent.backPackPointValue);
		if (GameData.gameData.playerData.equipment.totalWeight + weight <= maxWeight)
			return true;

		return false;
	}

	void Awake () {
		equipment = GameData.gameData.playerData.equipment;

		Init ();
	}

	public void Init () {
		itemNumber = 100 * levelNumber + levelMode;
		itemBG = GetComponent<Image> ();
	}

	void Start () {
		if (!IsAvailable ())
			DisableItem ();
		else
			ActivateItem ();
	}

	void OnEnable () {
		DeselectItem ();

		if (IsEquipped ())
			itemBG.color = equippedBGColor;
	}

	public void OnClick () {
		if (!IsAvailable ()) {
			string nameMode = levelMode == 1 ? "Hard" : "Hell";
			// Affichage du texte par défaut en fonction du type d'objet
			switch (itemType) {
			case ItemType.weapon:
				MainMenuManager.mainMenuManager.DisplayItemDescription ("???", string.Format ("You have to finish level {0} on {1} mode to unlock this", levelNumber, nameMode));
				break;
			case ItemType.shield:
				MainMenuManager.mainMenuManager.DisplayItemDescription ("???", string.Format ("You have to finish level {0} on {1} mode without falling under 75% HP to unlock this", levelNumber, nameMode));
				break;
			case ItemType.helm:
				MainMenuManager.mainMenuManager.DisplayItemDescription ("???", string.Format ("You have to finish level {0} on {1} mode with at least 80% of all leaves to unlock this", levelNumber, nameMode));
				break;
			case ItemType.perfume:
				MainMenuManager.mainMenuManager.DisplayItemDescription ("???", string.Format ("You have to travel more than 5000m in level {0} on Arcade mode to unlock this", levelNumber));
				break;
			}

			// On désélectionne les objets
			foreach (ItemButton item in transform.parent.GetComponentsInChildren<ItemButton> ()) {
				item.DeselectItem ();
			}
			ResetDeltaStat ();

			return;
		}
		
		// S'il n'est pas déjà sélectionné, on le sélectionne
		if (!IsSelected ()) {
			SelectItem ();
			MainMenuManager.sfxSound.ChangeClick ();

			// On supprime le delta des stats potentiellement affiché
			ResetDeltaStat ();
			// On affiche le delta des stats selon que l'objet est à équiper ou qu'il soit déjà équipé
			DeltaStat ();

			// Affichage de la description
			MainMenuManager.mainMenuManager.DisplayItemDescription (itemName, textDescription);
		}
		// S'il est déjà sélectionné et que le poids le permet
		else if (!IsEquipped () && IsWearable ()) {
			EquipItem ();
			DeselectItem ();
		}
		// S'il est déjà équipé, on le déséquipe
		else if (IsEquipped ()) {
			DesequipItem ();
			DeselectItem ();
		}
	}

	private void DeltaStat () {
		for (int i = 0; i < bonusStatName.Length; i++) {
			// Sélection de la valeur courante selon la stat
			int currentValue;
			currentValue = (int)equipment.GetType ().GetField (bonusStatName [i].ToString ()).GetValue (equipment);

			// Sélection du texte à modifier selon la stat
			Text deltaText = null;

			switch (bonusStatName [i]) {
			case ItemStatName.attack:
				deltaText = MainMenuManager.mainMenuManager.tDeltaAttack;
				break;
			case ItemStatName.criticalHit:
				deltaText = MainMenuManager.mainMenuManager.tDeltaCriticalHit;
				break;
			case ItemStatName.criticalPower:
				deltaText = MainMenuManager.mainMenuManager.tDeltaCriticalPower;
				break;
			case ItemStatName.healthPoint:
				deltaText = MainMenuManager.mainMenuManager.tDeltaHealthPoint;
				break;
			case ItemStatName.dodge:
				deltaText = MainMenuManager.mainMenuManager.tDeltaDodge;
				break;
			case ItemStatName.reflect:
				deltaText = MainMenuManager.mainMenuManager.tDeltaReflect;
				break;
			case ItemStatName.defense:
				deltaText = MainMenuManager.mainMenuManager.tDeltaDefense;
				break;
			case ItemStatName.thorn:
				deltaText = MainMenuManager.mainMenuManager.tDeltaThorn;
				break;
			case ItemStatName.regeneration:
				deltaText = MainMenuManager.mainMenuManager.tDeltaRegeneration;
				break;
			case ItemStatName.speed:
				deltaText = MainMenuManager.mainMenuManager.tDeltaSpeed;
				break;
			}

			if (deltaText != null) {
				// Valeur utilisée pour le calcul de la différence avec la valeur courante
				float itemValue = 0;
				if (!IsEquipped ())
					itemValue = bonusStatValue [i]; // Si on veut équiper un nouvel objet, on prend ses stats, sinon 0
				
				// Calcul de la différence entre la valeur actuelle et la valeur de l'objet sélectionné
				float deltaValue = itemValue - currentValue;
				// Spécificité pour la régénération
				if (bonusStatName [i] == ItemStatName.regeneration)
					deltaValue /= 10f;

				// Si la différence est positive (donc nouvelle stat PLUS puissante)
				if (deltaValue > 0) {
					// Spécificité pour la regen et le speed
					if (bonusStatName [i] == ItemStatName.regeneration) {
						deltaText.color = textBonusColor;
						deltaText.text = deltaValue.ToString ("+0.0");
					} else if (bonusStatName [i] == ItemStatName.speed) {
						deltaText.color = textMalusColor;
						deltaText.text = Mathf.Abs(deltaValue).ToString ("+0");
					} else {
						deltaText.color = textBonusColor;
						deltaText.text = deltaValue.ToString ("+0");
					}
				}
				// Si la différence est négative (donc nouvelle stat MOINS puissante)
				else if (deltaValue < 0) {
					// Spécificité pour la regen et le speed
					if (bonusStatName [i] == ItemStatName.regeneration) {
						deltaText.color = textMalusColor;
						deltaText.text = deltaValue.ToString ("0.0");
					} else if (bonusStatName [i] == ItemStatName.speed) {
						deltaText.color = textBonusColor;
						deltaText.text = Mathf.Abs(deltaValue).ToString ("-0");
					} else {
						deltaText.color = textMalusColor;
						deltaText.text = deltaValue.ToString ();
					}
				}
				// Si la différence est nulle, on affiche "-" en couleur neutre
				else {
					deltaText.color = textNeutralColor;
					deltaText.text = "-";
				}
			}

			// Affichage du poids de l'objet
			int newTotalWeight;
			if (!IsEquipped ())
				newTotalWeight = GameData.gameData.playerData.equipment.totalWeight + weight;
			else
				newTotalWeight = GameData.gameData.playerData.equipment.totalWeight;

			switch (itemType) {
			case ItemType.weapon:
				newTotalWeight -= equipment.weaponWeight;
				break;
			case ItemType.shield:
				newTotalWeight -= equipment.shieldWeight;
				break;
			case ItemType.helm:
				newTotalWeight -= equipment.helmWeight;
				break;
			case ItemType.perfume:
				newTotalWeight -= equipment.perfumeWeight;
				break;
			}

			MainMenuManager.mainMenuManager.ChangeWeightValue (newTotalWeight);
		}
	}

	public static void ResetDeltaStat () {
		MainMenuManager.mainMenuManager.tDeltaAttack.text = "";
		MainMenuManager.mainMenuManager.tDeltaCriticalHit.text = "";
		MainMenuManager.mainMenuManager.tDeltaCriticalPower.text = "";
		MainMenuManager.mainMenuManager.tDeltaHealthPoint.text = "";
		MainMenuManager.mainMenuManager.tDeltaDodge.text = "";
		MainMenuManager.mainMenuManager.tDeltaReflect.text = "";
		MainMenuManager.mainMenuManager.tDeltaDefense.text = "";
		MainMenuManager.mainMenuManager.tDeltaThorn.text = "";
		MainMenuManager.mainMenuManager.tDeltaRegeneration.text = "";
		MainMenuManager.mainMenuManager.tDeltaSpeed.text = "";

		MainMenuManager.mainMenuManager.ChangeWeightValue (GameData.gameData.playerData.equipment.totalWeight);
	}

	public void EquipItem () {
		if (!IsAvailable ())
			return;
		
		equipped = true;
		itemBG.color = equippedBGColor;

		// Nettoyage des stats avant d'appliquer les nouvelles (pour ne rien oublier)
		// Gestion du poids par type d'objet
		switch (itemType) {
		case ItemType.weapon:
			equipment.weapon = itemNumber;
			equipment.attack = 0;
			equipment.criticalHit = 0;
			equipment.criticalPower = 0;
			// Soustraction de l'objet actuellement équipé au total
			equipment.totalWeight -= equipment.weaponWeight;
			// Ajout du poids du nouvel objet
			equipment.weaponWeight = weight;
			break;
		case ItemType.shield:
			equipment.shield = itemNumber;
			equipment.defense = 0;
			equipment.dodge = 0;
			equipment.reflect = 0;
			// Soustraction de l'objet actuellement équipé au total
			equipment.totalWeight -= equipment.shieldWeight;
			// Ajout du poids du nouvel objet
			equipment.shieldWeight = weight;
			break;
		case ItemType.helm:
			equipment.helm = itemNumber;
			equipment.healthPoint = 0;
			equipment.thorn = 0;
			// Soustraction de l'objet actuellement équipé au total
			equipment.totalWeight -= equipment.helmWeight;
			// Ajout du poids du nouvel objet
			equipment.helmWeight = weight;
			break;
		case ItemType.perfume:
			equipment.perfume = itemNumber;
			equipment.regeneration = 0;
			equipment.speed = 100; // Valeur par défaut particulière
			// Soustraction de l'objet actuellement équipé au total
			equipment.totalWeight -= equipment.perfumeWeight;
			// Ajout du poids du nouvel objet
			equipment.perfumeWeight = weight;
			break;
		}

		// Ajout du poids du nouvel objet au total
		equipment.totalWeight += weight;

		// Ajout des stats dans GameData.gameData.playerData.equipment
		for (int i = 0; i < bonusStatName.Length; i++) {
			equipment.GetType ().GetField (bonusStatName [i].ToString ()).SetValue (equipment, bonusStatValue [i]);
		}

		// Ajout de l'objet dans la case associée (selon le type, testé dans MainMenuManager)
		MainMenuManager.mainMenuManager.EquipItem (itemImage.sprite, itemType);

		// On désequippe les autres objets
		foreach (ItemButton item in transform.parent.GetComponentsInChildren<ItemButton> ()) {
			if (item.GetInstanceID () != GetInstanceID ()) {
				item.equipped = false;
				item.DeselectItem ();
			}
		}

		ResetDeltaStat ();

		MainMenuManager.sfxSound.ButtonYesClick ();

		_StaticFunction.Save ();
	}

	private void DesequipItem () {
		if (!IsAvailable ())
			return;

		equipped = false;
		itemBG.color = activeBGColor;

		// Nettoyage des stats avant d'appliquer les nouvelles (pour ne rien oublier)
		// Gestion du poids par type d'objet
		switch (itemType) {
		case ItemType.weapon:
			equipment.weapon = itemNumber;
			equipment.attack = 0;
			equipment.criticalHit = 0;
			equipment.criticalPower = 0;
			// Soustraction de l'objet actuellement équipé au total
			equipment.totalWeight -= equipment.weaponWeight;
			// Suppression du poids du nouvel objet
			equipment.weaponWeight = 0;
			break;
		case ItemType.shield:
			equipment.shield = itemNumber;
			equipment.defense = 0;
			equipment.dodge = 0;
			equipment.reflect = 0;
			// Soustraction de l'objet actuellement équipé au total
			equipment.totalWeight -= equipment.shieldWeight;
			// Suppression du poids du nouvel objet
			equipment.shieldWeight = 0;
			break;
		case ItemType.helm:
			equipment.helm = itemNumber;
			equipment.healthPoint = 0;
			equipment.thorn = 0;
			// Soustraction de l'objet actuellement équipé au total
			equipment.totalWeight -= equipment.helmWeight;
			// Suppression du poids du nouvel objet
			equipment.helmWeight = 0;
			break;
		case ItemType.perfume:
			equipment.perfume = itemNumber;
			equipment.regeneration = 0;
			equipment.speed = 100; // Valeur par défaut particulière
			// Soustraction de l'objet actuellement équipé au total
			equipment.totalWeight -= equipment.perfumeWeight;
			// Suppression du poids du nouvel objet
			equipment.perfumeWeight = 0;
			break;
		}

		// Ajout des stats dans GameData.gameData.playerData.equipment
		for (int i = 0; i < bonusStatName.Length; i++) {
			equipment.GetType ().GetField (bonusStatName [i].ToString ()).SetValue (equipment, 0); // On enlève l'objet, donc 0
		}

		// Ajout de l'item par défaut dans la case associée (selon le type, testé dans MainMenuManager)
		MainMenuManager.mainMenuManager.DesequipItem (itemType);

		// On désequippe les autres objets
		foreach (ItemButton item in transform.parent.GetComponentsInChildren<ItemButton> ()) {
			if (item.GetInstanceID () != GetInstanceID ()) {
				item.equipped = false;
				item.DeselectItem ();
			}
		}

		ResetDeltaStat ();

		MainMenuManager.sfxSound.ButtonNoClick ();

		_StaticFunction.Save ();
	}

	private void ActivateItem () {
		itemImage.color = Color.white;
	}

	private void DisableItem () {
		itemImage.color = disableColor;
	}

	private void SelectItem () {
		if (IsAvailable ()) {
			selected = true;
			itemBG.color = selectBGColor;
		}

		// On désélectionne les autres objets
		foreach (ItemButton item in transform.parent.GetComponentsInChildren<ItemButton> ()) {
			if (item.GetInstanceID () != GetInstanceID ())
				item.DeselectItem ();
		}
	}

	private void DeselectItem () {
		selected = false;

		if (IsAvailable ()) {
			if (IsEquipped ())
				itemBG.color = equippedBGColor;
			else
				itemBG.color = activeBGColor;
		}
		else
			itemBG.color = disableBGColor;
	}
}
