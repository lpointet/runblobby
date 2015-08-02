using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PoolingManager : MonoBehaviour {
	
	public static PoolingManager current;

	public List<PoolingScript> poolCollection;
	private static Dictionary<string, PoolingScript> pools = new Dictionary<string, PoolingScript>();
	
	void Awake() {
		current = this;
		
		foreach(PoolingScript pool in poolCollection)
		{
			pool.Init();
			pools.Add(pool.poolName, pool);
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
	public string RandomNameOfPool(string subname){
		int random = Random.Range (0, pools.Count);

		if (poolCollection[random].poolName.ToLower().Contains (subname.ToLower()))
			return poolCollection[random].poolName;
		return "";
	}
}
