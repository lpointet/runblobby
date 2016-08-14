using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TalentButton : MonoBehaviour {
	
	private Transform myTransform;
	[SerializeField] private Image myImage;
	private Talent talent;								// = GameData.gameData.playerData.talent

	[SerializeField] private Image selectImage;			// Zone qui montre qu'un talent est sélectionné
	private bool selected = false;						// Savoir si le talent a déjà été sélectionné

	[Header("Appearance")]
	[SerializeField] private Color disableColor;		// Tant qu'aucun point n'est dans le talent
	[SerializeField] private Color activeColor;			// Tant que le talent a des points
	[SerializeField] private Color fullColor;			// Quand le talent est full

	[SerializeField] private string title;				// Nom du talent
	[SerializeField][TextArea(3,10)] private string textDescription;	// Description textuelle du talent (eg: Gain attack)
	[SerializeField][TextArea(3,10)] private string mathDescription;	// Description "mathématique" du talent (eg: +{value} Attack)

	[Header("Technical")]
	[SerializeField] private string section;			// Nom de la section dans laquelle se trouve le talent (eg: armory)
	[SerializeField] private string gameValueName;		// Nom de la variable à changer dans le GameData
	public int currentValue { get; private set; }		// Nombre de points actuellement dans le talent
	[SerializeField] private int maxValue;				// Valeur maximale du talent
	private float gainPerPoint;							// Gain par point de talent (eg: 5) dans le GameData
	private bool activated = false;						// TRUE lorsque les prérequis du talent sont validés
	private bool fullActivation = false;				// TRUE lorsque le talent est au maximum
	[SerializeField] private Text textValue;			// Texte contenant la valeur
	private string suffixValue;							// String après la valeur courante (eg: /maxValue)

	[SerializeField] private int[] leafCost;			// Cout en feuilles par point (table size = maxValue)
	[SerializeField] private int levelRequire;			// Niveau du joueur requis pour débloqué le talent
	[SerializeField] private string[] requiredValueName;// Noms des autres talents nécessaires en prérequis
	[SerializeField] private int[] requiredValueCost;	// Nombre de points nécessaires dans les autres talents en prérequis

	void OnValidate () {
		if (leafCost.Length != maxValue)
			leafCost = new int[maxValue];

		if (requiredValueName.Length != requiredValueCost.Length)
			requiredValueCost = new int[requiredValueName.Length];
	}

	public bool IsSelected () { return selected; }
	public bool IsActivated () { return activated; }

	public bool IsAvailable () {
		// Si le talent est déjà actif, il est actif !
		if (activated)
			return true;

		// Contrôle des prérequis sur les autres talents
		for (int i = 0; i < requiredValueName.Length; i++) {
			if ((int)talent.GetType ().GetField (requiredValueName [i]).GetValue (talent) < requiredValueCost [i])
				return false;
		}

		// Contrôle du prérequis sur le level minimum
		if (levelRequire > GameData.gameData.playerData.level)
			return false;

		return true;
	}



	void Awake () {
		myTransform = transform;
	}

	void Start () {
		talent = GameData.gameData.playerData.talent;
		gainPerPoint = (float)talent.GetType ().GetField (gameValueName + "PointValue").GetValue (talent);
		suffixValue = "/" + maxValue.ToString ();

		if (!IsAvailable ())
			DisableTalent ();
		// Sinon, on affiche la valeur courante du talent
		else {
			ActivateTalent ();
			ValueChange (0);
		}
	}

	void OnEnable () {
		DeselectTalent ();
	}

	public void OnClick () {
		// On sélectionne celui qui nous intéresse
		if (!IsSelected ()) {
			for (int i = 0; i < MainMenuManager.listTalent.Length; i++) {
				if (MainMenuManager.listTalent [i].IsSelected ())
					MainMenuManager.listTalent [i].DeselectTalent ();
			}

			DisplayTalent ();
			SelectTalent ();
			return;
		}

		if (fullActivation || !activated) {
			return;
		}

		// Si on n'a pas réussi à acheter le talent, on affiche en rouge le nombre de feuilles restantes temporairement
		if (!BuyTalent ()) {
			StartCoroutine (MainMenuManager.mainMenuManager.ShakeLeaves ());
			return;
		}

		MainMenuManager.mainMenuManager.UpdateTalent ();
		StartCoroutine (PopTalent ());
	}

	// Achète un talent
	private bool BuyTalent () {
		if (GameData.gameData.playerData.leaf >= leafCost [currentValue]) {
			GameData.gameData.playerData.leaf -= leafCost [currentValue];
			ValueChange ();
			DisplayTalent (true);

			return true;
		}
		return false;
	}

	// Ajoute des points à un talent
	private void ValueChange (int addValue = 1) {
		// On ne change rien si c'est au maximum
		if (fullActivation)
			return;

		int talentValue = (int)talent.GetType ().GetField (gameValueName).GetValue (talent);
		// On incrémente les valeurs : affichage et gameData (section + talent)
		currentValue = talentValue + addValue;
		talent.GetType ().GetField (gameValueName).SetValue(talent, talentValue + addValue);

		if (section != "") {
			int sectionValue = (int)talent.GetType ().GetField (section).GetValue (talent);
			talent.GetType ().GetField (section).SetValue (talent, sectionValue + addValue);
		}
		// Affichage
		textValue.text = currentValue.ToString () + suffixValue;
		// Si le talent est au maximum, on active la complétion
		if (currentValue == maxValue)
			FullActivation ();

		// Sauvegarde si la valeur change
		if (addValue != 0)
			_StaticFunction.Save ();
	}

	private IEnumerator PopTalent () {
		Vector3 initialScale = myTransform.localScale;
		Vector3 maxScale = initialScale * 1.25f;
		float timeToComplete = 0.25f;
		float timer = 0;

		while (timer < timeToComplete) {
			myTransform.localScale = Vector3.Lerp (initialScale, maxScale, timer / timeToComplete);

			timer += Time.deltaTime;
			yield return null;
		}

		myTransform.localScale = initialScale;
	}

	private void FullActivation () {
		activated = true;
		fullActivation = true;

		myImage.color = fullColor;
		textValue.color = fullColor;
		textValue.text = "Max";
	}

	private void DisableTalent () {
		activated = false;
		fullActivation = false;

		myImage.color = disableColor;
		textValue.text = "";
	}

	public void ActivateTalent () {
		if (currentValue == maxValue) {
			FullActivation ();
			return;
		}
		
		activated = true;
		fullActivation = false;

		myImage.color = activeColor;
		currentValue = 0;
		textValue.text = currentValue.ToString () + suffixValue;

		StartCoroutine (PopTalent ());
	}

	public void SelectTalent () {
		selected = true;
		selectImage.enabled = true;
	}

	public void DeselectTalent () {
		selected = false;
		selectImage.enabled = false;
	}

	private void DisplayTalent(bool bought = false) {
		if (currentValue < maxValue)
			MainMenuManager.mainMenuManager.DisplayTalent (myTransform, title, textDescription, mathDescription, gainPerPoint, currentValue, maxValue, leafCost [currentValue], bought);
		else
			MainMenuManager.mainMenuManager.DisplayTalent (myTransform, title, textDescription, mathDescription, gainPerPoint, currentValue, maxValue, 0);
	}
}
