using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(MapController))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapController mapGen = (MapController)target;

        if (DrawDefaultInspector())
        {
            if (mapGen.autoUpdate)
            {
                mapGen.GenerateEditorMaps();
            }
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate"))
        {
            mapGen.GenerateEditorMaps();
        }
        GUILayout.EndHorizontal();
    }
}