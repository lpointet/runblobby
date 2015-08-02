using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PoolingScript : MonoBehaviour {

	public string poolName;
	public GameObject pooledObject;
	public int pooledAmount;

	public bool willGrow = true;
	public float despawnTimer = -1f;

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
			pooledObjects.Add (obj);
		}
	}

	public GameObject GetPooledObject(){
		for (int i = 0; i < pooledObjects.Count; i++) {
			if(!pooledObjects[i].activeInHierarchy){
				if (despawnTimer > 0) // on appelle une coroutine pour désactiver après le délai de la pool
					StartCoroutine(DespawnAfterDelay(pooledObjects[i]));
				return pooledObjects[i];
			}
		}
		// si on permet à la pool de s'agrandir, on instantie des nouveaux objets
		if (willGrow) {
			GameObject obj = (GameObject)Instantiate (pooledObject);
			pooledObjects.Add (obj);
			if (despawnTimer > 0) // on appelle une coroutine pour désactiver après le délai de la pool
				StartCoroutine(DespawnAfterDelay(obj));
			return obj;
		}

		return null;
	}

	private IEnumerator DespawnAfterDelay(GameObject obj)
	{
		float delay = despawnTimer; // on prend le délai de la pool
		while (delay > 0) {
			delay -= Time.deltaTime;
			yield return null;
		}
		obj.SetActive(false);
	}
}
