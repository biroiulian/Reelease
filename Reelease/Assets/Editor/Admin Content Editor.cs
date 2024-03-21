using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AdminContentCreator))]
public class AdminContentEditor : Editor
{
    public override void OnInspectorGUI()
    {
        AdminContentCreator admin = (AdminContentCreator)target;

        DrawDefaultInspector();

        GUILayout.BeginVertical();
        if (GUILayout.Button("Add shop item"))
        {
            admin.AddShopItem();
        }
        if (GUILayout.Button("Add challenge"))
        {
            admin.AddChallengeItem();
        }
        if (GUILayout.Button("Add inventory item"))
        {
            admin.AddInventoryItem();
        }
        GUILayout.EndVertical();
    }
}
