using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(WaterPlaneGenerator))]
public class WaterGeneratorEditor : Editor
{

    public override void OnInspectorGUI()
    {
        WaterPlaneGenerator mapGen = (WaterPlaneGenerator)target;

        DrawDefaultInspector();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate"))
        {
            mapGen.GenerateWater();
        }
        GUILayout.EndHorizontal();
    }
}