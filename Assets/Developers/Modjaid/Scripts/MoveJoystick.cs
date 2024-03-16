using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class MoveJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public float Horizontal { get { return (SnapX) ? SnapFloat(input.x, AxisOptions.Horizontal) : input.x; } }
    public float Vertical { get { return (SnapX) ? SnapFloat(input.y, AxisOptions.Vertical) : input.y; } }
    public Vector2 Direction { get { return new Vector2(Horizontal, Vertical); } }

    public float HandleRange
    {
        get { return handleRange; }
        set { handleRange = Mathf.Abs(value); }
    }
    public float DeadZone
    {
        get { return deadZone; }
        set { deadZone = Mathf.Abs(value); }
    }

    private AxisOptions AxisOptions { get { return AxisOptions; } set { axisOptions = value; } }
    private bool SnapX; //public bool SnapX{ get { return snapX; } set { snapX = value; } }
    private bool SnapY; //public bool SnapY{ get { return snapY; } set { snapY = value; } }


    [SerializeField] private float handleRange = 1;
    [SerializeField] private float deadZone = 0;
    //[SerializeField] private bool snapX = false;
    //[SerializeField] private bool snapY = false;

    [SerializeField] private RectTransform background = null;
    [SerializeField] private RectTransform handle = null;
    private RectTransform baseRect = null;
    private Vector3 BackgroundStartPos;
    private AxisOptions axisOptions = AxisOptions.Both;
    private Canvas canvas;
    private Camera cam;

    private Vector2 input = Vector2.zero;
    private bool OnSprint;
    [SerializeField] private JoyEvent vectorChanged;
    [Range(0, 100)] public float angleAutoSprintDiapason;



    public void Start()
    {
        OnSprint = false;
        HandleRange = handleRange;
        DeadZone = deadZone;
        baseRect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
            Debug.LogError("The Joystick is not placed inside a canvas");

        Vector2 center = new Vector2(0.5f, 0.5f);
        background.pivot = center;
        handle.anchorMin = center;
        handle.anchorMax = center;
        handle.pivot = center;
        handle.anchoredPosition = Vector2.zero;
        BackgroundStartPos = background.localPosition;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        background.anchoredPosition = ScreenPointToAnchoredPosition(eventData.position);
        OnSprint = false;
        transform.GetChild(0).GetChild(1).gameObject.SetActive(false); // Показательный значок что Автоспринт Включен (можно удалить)
    }

    public void OnPointerUp(PointerEventData eventData)
    {

        if(OnSprint == false)
        {
            
            input = Vector2.zero;
            handle.anchoredPosition = Vector2.zero;
            vectorChanged?.Invoke(Direction);

        }
        background.anchoredPosition = BackgroundStartPos;

    }

    public Vector2 ScreenPointToAnchoredPosition(Vector2 screenPosition)
    {
        Vector2 localPoint = Vector2.zero;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(baseRect, screenPosition, cam, out localPoint))
        {
            Vector2 pivotOffset = baseRect.pivot * baseRect.sizeDelta;
            return localPoint - (background.anchorMax * baseRect.sizeDelta) + pivotOffset;
        }
        return Vector2.zero;
    }

    public void OnDrag(PointerEventData eventData)
    {
        cam = null;
        if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            cam = canvas.worldCamera;

        Vector2 position = RectTransformUtility.WorldToScreenPoint(cam, background.position);
        Vector2 radius = background.sizeDelta / 2;
        input = (eventData.position - position) / (radius * canvas.scaleFactor);
      //  FormatInput();
        HandleInput(input.magnitude, input.normalized);
        handle.anchoredPosition = input * radius * handleRange;
        vectorChanged?.Invoke(Direction);

        float angleAutoSprint = ((HandleRange / 100f) * ((HandleRange) - (angleAutoSprintDiapason * 2))) + HandleRange;
        if (Direction.magnitude == 1 && Direction.y > angleAutoSprint)
        {
            OnSprint = true;
            transform.GetChild(0).GetChild(1).gameObject.SetActive(true); // Показательный значок что Автоспринт Включен (можно удалить)
        }
        else
        {
            OnSprint = false;
            transform.GetChild(0).GetChild(1).gameObject.SetActive(false); // Показательный значок что Автоспринт Включен (можно удалить)
        }
    }

  //  private void FormatInput()
  //  {
  //      if (axisOptions == AxisOptions.Horizontal)
  //          input = new Vector2(input.x, 0f);
  //      else if (axisOptions == AxisOptions.Vertical)
  //          input = new Vector2(0f, input.y);
  //  }

    private void HandleInput(float magnitude, Vector2 normalised)
    {
        if (magnitude > deadZone)
        {
            if (magnitude > 1)
                input = normalised;
        }
        else
            input = Vector2.zero;
    }

    private float SnapFloat(float value, AxisOptions snapAxis)
    {
        if (value == 0)
            return value;

        if (axisOptions == AxisOptions.Both)
        {
            float angle = Vector2.Angle(input, Vector2.up);
            if (snapAxis == AxisOptions.Horizontal)
            {
                if (angle < 22.5f || angle > 157.5f)
                    return 0;
                else
                    return (value > 0) ? 1 : -1;
            }
            else if (snapAxis == AxisOptions.Vertical)
            {
                if (angle > 67.5f && angle < 112.5f)
                    return 0;
                else
                    return (value > 0) ? 1 : -1;
            }
            return value;
        }
        else
        {
            if (value > 0)
                return 1;
            if (value < 0)
                return -1;
        }
        return 0;
    }
}
public enum AxisOptions { Both, Horizontal, Vertical }

[Serializable]
public class JoyEvent : UnityEvent<Vector2>
{

}
#if UNITY_EDITOR
[CustomEditor(typeof(MoveJoystick))]
public class MoveJoystickEditorGUI : Editor
{
    public override void OnInspectorGUI()
    {
        MoveJoystick joystick = (MoveJoystick)target;
        EditorGUILayout.BeginVertical("Box", GUILayout.Height(80));
        base.OnInspectorGUI();
        EditorGUILayout.HelpBox("Настройка углового диапазона для активации авто спринт " +
            "\nПолный ноль отключает автоСпринт полностью" +
            "\nОтсчет идет с самой верхней точки", MessageType.Info);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }
}
#endif
