using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEditor;
using Mirror.Examples;
using MatchMaking;
using TMPro;

public class HealthIndicator : MonoBehaviour
{
    public Color indicateColor;
    public Image image;
    public MatchMaking.InputManager input;


    public void Update()
    {
        ChangeColor();
    }


    private void ChangeColor()
    {
        var c = indicateColor;
        c.a = (1 - input.player.Health / 100) / 2;
        image.color = c;
    }

}

#if UNITY_EDITOR
[CustomEditor(typeof(HealthIndicator))]
public class TEST_HealthIndicatorEditorGUI : Editor
{
    public override void OnInspectorGUI()
    {
        HealthIndicator indicator = (HealthIndicator)target;
        DrawDefaultInspector();
        EditorGUILayout.BeginVertical("Box");

        if (Application.isPlaying)
        {
            EditorGUILayout.LabelField("Здоровье персонажа = " + indicator.input.player.Health, GUILayout.Height(30));
        }

        if (GUILayout.Button("Health +20", GUILayout.Height(30)))
        {

            indicator.input.player.Health += 20;
            Debug.Log("Здоровье персонажа: " + indicator.input.player.Health);

        }

        if (GUILayout.Button("Health -20", GUILayout.Height(30)))
        {
            indicator.input.player.Health -= 20;
            Debug.Log("Здоровье персонажа: " + indicator.input.player.Health);
        }

        if (GUILayout.Button("Death", GUILayout.Height(30)))
        {
            indicator.input.player.Health -= 120;
            Debug.Log("Здоровье персонажа: " + indicator.input.player.Health);
        }
        
        EditorGUILayout.EndVertical();


    }
}
#endif

