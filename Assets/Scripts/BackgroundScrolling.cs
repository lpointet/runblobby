using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.Collections;
using UnityEditorInternal;
using System.Reflection;
using System;
#endif

public class BackgroundScrolling : MonoBehaviour {

	public float scrollSpeed;

	public bool isVariable;
	public float baseCutoff = 0f;
	public float rangeSinCutoff = 0f;
	public float baseYOffset = 1f;
	public float rangeSinYOffset = 0f;
	public float speedYOffset = 1f;

	private float xOffset;
	private float ratioVitesse;
	private Material myMaterial;

	void Start() {
		myMaterial = GetComponent<Renderer> ().material;

		// Adapater l'échelle à la taille de l'écran pour qu'on voit tout à chaque fois
		//transform.localScale = new Vector2 (Camera.main.orthographicSize * Camera.main.aspect * 2, Camera.main.orthographicSize * 2);

		// La vitesse dépend du joueur et de ce ratio = la taille que doit parcourir l'objet et sa distance au joueur (z)
		ratioVitesse = transform.position.z * scrollSpeed;
		xOffset = UnityEngine.Random.Range(0f, 1f);
	}

	void Update () {
		if (!TimeManager.paused && !LevelManager.GetPlayer ().IsDead ()) {
			// Décallage permanent dans le sens inverse du joueur
			xOffset = Mathf.Repeat (xOffset + LevelManager.levelManager.GetLocalDistance() / ratioVitesse, 1);
			//float x = Mathf.Repeat (Time.time * scrollSpeed - decalage, 1);
			float y = myMaterial.mainTextureOffset.y;

			// Si on a décidé que c'était à taille variable, on modifie ici
			if (isVariable) {
				float cutoff = baseCutoff + rangeSinCutoff * Mathf.Sin (TimeManager.time);
				y = baseYOffset + rangeSinYOffset * Mathf.Sin (speedYOffset * TimeManager.time);

				myMaterial.SetFloat ("_Cutoff", cutoff);
			}

			Vector2 offset = new Vector2 (xOffset, y);
			myMaterial.mainTextureOffset = offset;
		}
	}
}

#if UNITY_EDITOR
[CanEditMultipleObjects]
[CustomEditor(typeof(MeshRenderer))]
public class MeshRendererEditor : Editor
{
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();
		serializedObject.Update();
		SerializedProperty sortingLayerID = serializedObject.FindProperty("m_SortingLayerID");
		SerializedProperty sortingOrder = serializedObject.FindProperty("m_SortingOrder");
		Rect firstHoriz = EditorGUILayout.BeginHorizontal();
		EditorGUI.BeginChangeCheck();
		EditorGUI.BeginProperty(firstHoriz, GUIContent.none, sortingLayerID);
		string[] layerNames = GetSortingLayerNames();
		int[] layerID = GetSortingLayerUniqueIDs();
		int selected = -1;
		int sID = sortingLayerID.intValue;
		for (int i = 0; i < layerID.Length; i++)
			if (sID == layerID[i])
				selected = i;
		if (selected == -1)
			for (int i = 0; i < layerID.Length; i++)
				if (layerID[i] == 0)
					selected = i;
		selected = EditorGUILayout.Popup("Sorting Layer", selected, layerNames);
		sortingLayerID.intValue = layerID[selected];
		EditorGUI.EndProperty();
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();
		EditorGUI.BeginChangeCheck();
		EditorGUILayout.PropertyField(sortingOrder, new GUIContent("Order in Layer"));
		EditorGUILayout.EndHorizontal();
		serializedObject.ApplyModifiedProperties();
	}
	public string[] GetSortingLayerNames()
	{
		Type internalEditorUtilityType = typeof(InternalEditorUtility);
		PropertyInfo sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);
		return (string[])sortingLayersProperty.GetValue(null, new object[0]);
	}
	public int[] GetSortingLayerUniqueIDs()
	{
		Type internalEditorUtilityType = typeof(InternalEditorUtility);
		PropertyInfo sortingLayerUniqueIDsProperty = internalEditorUtilityType.GetProperty("sortingLayerUniqueIDs", BindingFlags.Static | BindingFlags.NonPublic);
		return (int[])sortingLayerUniqueIDsProperty.GetValue(null, new object[0]);
	}
}
#endif