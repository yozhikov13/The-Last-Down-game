using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
public class MiniMap : MonoBehaviour
{
    public Transform targetPlayer;
    public Transform locationCenter;
    public GameObject baseMap;
    public GameObject extendedMap;

    public void switchMap(GameObject onMap)
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
        onMap.SetActive(true);
    }

    public void switchMap()
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(false);
        }
    }
    public void initPositionTargets()
    {
        try
        {
            baseMap.transform.GetChild(0).GetComponent<MapCamera>().targetPosition = targetPlayer;
            extendedMap.transform.GetChild(0).GetComponent<MapCamera>().targetPosition = locationCenter;
        }
        catch (Exception e)
        {

        }
    }
}

[CustomEditor(typeof(MiniMap))]
public class MiniMaptEditorGUI : Editor
{
    private int toolBarInt = 0;
    private int changeNum = -1;

    public override void OnInspectorGUI()
    {
        MiniMap miniMap = (MiniMap)target;
        MapCamera mapCamera;
        serializedObject.Update();


        toolBarInt = GUILayout.Toolbar(toolBarInt, new string[] { "Base Map", "Extended Map" }, GUILayout.Height(40));
        if (toolBarInt == 0)
        {
            
            EditorGUILayout.BeginVertical("Box", GUILayout.Height(100));
            DrawPropertiesExcluding(serializedObject, new string[] { "extendedMap", "locationCenter" });
            EditorGUILayout.EndVertical();

            if (miniMap.baseMap == null || miniMap.targetPlayer == null)
            {
                EditorGUILayout.HelpBox("PLEASE FILL IN PREFABS", MessageType.Error);
                serializedObject.ApplyModifiedProperties();
                return;
            }
            mapCamera = miniMap.baseMap.transform.GetChild(0).GetComponent<MapCamera>();

            if (changeNum != toolBarInt)
            {
                changeNum = toolBarInt;
                miniMap.switchMap(miniMap.baseMap);
            }

        }
        else
        {


            EditorGUILayout.BeginVertical("Box", GUILayout.Height(100));
            DrawPropertiesExcluding(serializedObject, new string[] { "baseMap", "targetPlayer" });
            EditorGUILayout.EndVertical();

            if (miniMap.extendedMap == null || miniMap.locationCenter == null)
            {
                EditorGUILayout.HelpBox("PLEASE FILL IN PREFABS", MessageType.Error);
                serializedObject.ApplyModifiedProperties();
                return;
            }

            mapCamera = miniMap.extendedMap.transform.GetChild(0).GetComponent<MapCamera>();

            if (changeNum != toolBarInt)
            {
                changeNum = toolBarInt;
                miniMap.switchMap(miniMap.extendedMap);
            }
        }

        EditorGUILayout.BeginVertical("Box");
        mapCamera.maxHight = EditorGUILayout.FloatField("Max Hight", mapCamera.maxHight);
        mapCamera.minHight = EditorGUILayout.FloatField("Min Hight", mapCamera.minHight);
        if (mapCamera.maxHight == 0)
        {
            EditorGUILayout.HelpBox("Don't Forget to set the range for visibility", MessageType.Warning);
        }
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Zoom", GUILayout.Width(40));
        mapCamera.Zoom = EditorGUILayout.Slider(mapCamera.Zoom, mapCamera.minHight, mapCamera.maxHight);
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        mapCamera.updateZoomCamera();
        miniMap.initPositionTargets();
        mapCamera.autoSetPositionCamera();

        serializedObject.ApplyModifiedProperties();
    }

    void ErrorMessage()
    {

    }
}
#endif


