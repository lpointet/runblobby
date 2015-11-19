using UnityEngine;
using System.Collections.Generic;

public class PoolingScript : MonoBehaviour {

	public string poolName;
	public GameObject pooledObject;
	public int pooledAmount;

	public bool willGrow = true;
	public string poolIndex;

	private List<GameObject> pooledObjects;

	public void Init() {
		if (poolName == "")
			poolName = name;
	}

	public void Awake() {
		Init();

		pooledObjects = new List<GameObject> ();

		for (int i = 0; i < pooledAmount; i++) {
			GameObject obj = (GameObject)Instantiate (pooledObject);
			obj.SetActive (false);
			obj.transform.parent = PoolingManager.current.pooledObjectParent;
			pooledObjects.Add (obj);
		}
	}

	public GameObject GetPooledObject(){
		for (int i = 0; i < pooledObjects.Count; i++) {
			if(!pooledObjects[i].activeInHierarchy){
				return pooledObjects[i];
			}
		}
		// si on permet à la pool de s'agrandir, on instantie des nouveaux objets
		if (willGrow) {
			GameObject obj = (GameObject)Instantiate (pooledObject);
			obj.SetActive (false);
			obj.transform.parent = PoolingManager.current.pooledObjectParent;
			pooledObjects.Add (obj);
			return obj;
		}
		return null;
	}
}
