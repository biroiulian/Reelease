using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(EnviromentController))]
public class EnviromentGeneratorEditor : Editor
{

    public override void OnInspectorGUI()
    {
        EnviromentController envGen = (EnviromentController)target;

        if (DrawDefaultInspector())
        {
            if (envGen.autoUpdate)
            {
                envGen.DrawEnviroment();
            }
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Generate"))
        {
            envGen.DrawEnviroment();
        }
        GUILayout.EndHorizontal();
    }
}