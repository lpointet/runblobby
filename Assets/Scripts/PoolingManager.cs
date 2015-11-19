using UnityEngine;
using System.Collections.Generic;

public class PoolingManager : MonoBehaviour {
	
	public static PoolingManager current;
	public Transform pooledObjectParent; // TODO : corriger sa disparition quand on relance le level

	public List<PoolingScript> poolCollection;
	private Dictionary<string, List<PoolingScript>> indexedPools = new Dictionary<string, List<PoolingScript>>();
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
				List<PoolingScript> list;

				// On vérifie si la valeur existe déjà, et on ajoute dans la bonne key
				if( !indexedPools.TryGetValue( pool.poolIndex, out list ) ) {
					list = new List<PoolingScript>();
					indexedPools.Add( pool.poolIndex, list );
				}
				list.Add( pool );
			}
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
	public string RandomNameOfPool(string subname, string index = null) {
		List<PoolingScript> list;
		if( null == index || !indexedPools.TryGetValue( index, out list ) ) {
			// Si on n'a pas spécifié d'index ou qu'on a pas trouvé la liste correspondante, on prend dans toute la collection
			list = poolCollection;
		}

		int random = Random.Range (0, list.Count);

		if (list[random].poolName.ToLower().Contains (subname.ToLower()))
			return list[random].poolName;
		return "";
	}
}
