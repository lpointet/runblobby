using UnityEngine;
using System.Collections.Generic;

public class OrderedPools {
	public int currentIndex;
	public List<PoolingScript> pools;

	public OrderedPools (List<PoolingScript> list, int index = 0) {
		currentIndex = index;
		pools = list;
	}
}

public class PoolingManager : MonoBehaviour {
	
	public static PoolingManager current;
	public Transform pooledObjectParent; // TODO : corriger sa disparition quand on relance le level

	public List<PoolingScript> poolCollection;
	//private Dictionary<string, List<PoolingScript>> indexedPools = new Dictionary<string, List<PoolingScript>>();
	private Dictionary<string, OrderedPools> indexedPools = new Dictionary<string, OrderedPools>();
	private static Dictionary<string, PoolingScript> pools = new Dictionary<string, PoolingScript>();

	void Awake() {
		if (current == null) {
			current = this;
			DontDestroyOnLoad (gameObject);
		} else if (current != this) {
			Destroy (gameObject);
		}

		indexedPools.Clear ();
		pools.Clear ();

		foreach(PoolingScript pool in poolCollection)
		{
			pool.Init();

			pools.Add(pool.poolName, pool);

			// S'il y a un index, on ajoute dans un dictionnaire de listes
			if( "" != pool.poolIndex ) {
				OrderedPools orderedPools;

				// On vérifie si la valeur existe déjà, et on ajoute dans la bonne key
				if( !indexedPools.TryGetValue( pool.poolIndex, out orderedPools ) ) {
					orderedPools = new OrderedPools (new List<PoolingScript>());
					//indexedPools.Add( pool.poolIndex, list );
					indexedPools.Add( pool.poolIndex, orderedPools );
				}
				orderedPools.pools.Add (pool);
			}
		}
			
		// On mélange une première fois les listes indexées
//		foreach (KeyValuePair<string, List<PoolingScript>> entry in indexedPools) {
//			ShuffleList (entry.Value, true);
//		}
		foreach (KeyValuePair<string, OrderedPools> entry in indexedPools) {
			ShuffleList (entry.Value.pools, true);
		}
	}

	// Retourne le GameObject correspondant à la PoolingScript "name"
	public GameObject Spawn(string name) {
		PoolingScript pool;

		if (pools.TryGetValue (name, out pool))
			return pool.GetPooledObject ();
		else
			return null;
	}
	
	// Retourne le nom de la PoolingScript si celle-ci correspond au type recherché
	// TODO à améliorer vu que ça peut potentiellement rater sa pool pendant un bon moment...
//	public string RandomNameOfPool(string subname, string index = null) {
//		List<PoolingScript> list;
//		if( null == index || !indexedPools.TryGetValue( index, out list ) ) {
//			// Si on n'a pas spécifié d'index ou qu'on a pas trouvé la liste correspondante, on prend dans toute la collection
//			list = poolCollection;
//		}
//
//		int random = Random.Range (0, list.Count);
//
//		if (list [random].poolName.ToLower ().Contains (subname.ToLower ())) {
//			Debug.Log ("Nouveau block depuis : " + list [random].poolName);
//			return list [random].poolName;
//		}
//		return "";
//	}

	public string RandomPoolName(string subname, string index = null) {
		OrderedPools list;

		if (null == index || !indexedPools.TryGetValue (index, out list)) {
			// Si on n'a pas spécifié d'index ou qu'on a pas trouvé la liste correspondante à l'index, on prend dans toutes les pools
			list = new OrderedPools (poolCollection);

			// On choisit un élément au hasard correspondant au subname et le retourne
			int random = Random.Range (0, list.pools.Count);

			if (list.pools [random].poolName.ToLower ().Contains (subname.ToLower ())) {
				Debug.Log ("Nouveau block depuis : " + list.pools [random].poolName);
				return list.pools [random].poolName;
			}
		} else {
			// Si on trouve une liste dans le test précédent

			// On prend le prochain élément dans la liste
			// ex : index = difficulty_1, subname = block -- on prend dans les pools du dico indexedPools avec key = difficulty_1
			string listElement;
			listElement = list.pools [list.currentIndex].poolName;
			list.currentIndex++; // On passe à l'élément suivant

			// Mélange de la liste si on a atteint le bout
			// On aurait pu le faire à la prochaine itération, mais en le faisant ici on permet d'éviter une surcharge du CPU si le shuffle intervient au mauvais moment
			// Ici, un block est déjà créé
			if (list.currentIndex >= list.pools.Count) {
				ShuffleList (list.pools);
				list.currentIndex = 0;
			}

			Debug.Log ("Nouveau block depuis : " + listElement);
			return listElement;
		}

		return "";
	}

	// Mélange d'une liste de PoolingScript
	private void ShuffleList(List<PoolingScript> list, bool firstTime = false) {
		PoolingScript temp;
		int longueur = list.Count;
		int interdit = firstTime ? 0 : Mathf.RoundToInt (longueur * 0.35f); // Tous les nombres sont mélangés la première fois | Sinon, on empêche 35% de la liste d'êtré réutilisée

		if (longueur > 1) { // On ne procède a un mélange que s'il y a au moins 2 valeurs
			for (int i = 0; i < longueur; i++) {
				int randNumber = Random.Range (i, longueur - interdit); // Borne inf incluse, borne sup excluse

				temp = list [i];
				list [i] = list [randNumber];
				list [randNumber] = temp;

				if (interdit > 0)
					interdit--;
			}
		}

		string listName = "";
		foreach (PoolingScript item in list) {
			listName += item.poolName + " ";
		}
		Debug.Log ("Nouveau tirage pour la Pool d'index \"" + list[0].poolIndex + "\" : " + listName);
	}
}
