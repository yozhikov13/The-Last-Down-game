#if (UNITY_EDITOR)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Weapon),true)]
public class WeaponEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Weapon myScript = (Weapon)target;
        if (GUILayout.Button("Start Shoot"))
        {
            myScript.StartShooting();
        }
        if (GUILayout.Button("End Shoot"))
        {
            myScript.EndShooting();
        }
        if(GUILayout.Button("Save To DataBase"))
        {
            myScript.SaveToDB();
        }
        DrawDefaultInspector();
    }
}
#endif
