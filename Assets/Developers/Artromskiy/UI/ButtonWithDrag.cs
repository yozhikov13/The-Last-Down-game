using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonWithDrag : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public Color pressedColor;
    private Color simpleColor;
    public UnityEvent onButtonDown;
    public UnityEvent onButtonUp;
    public Vector2Event onDrag;

    private Image image;

    private void Start()
    {
      image = GetComponent<Image>();
      if(image)
        simpleColor = image.color;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
      onButtonDown?.Invoke();
      if(image)
        image.color = pressedColor;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
      onButtonUp?.Invoke();
    }

    public void OnDrag(PointerEventData eventData)
    {
      onDrag?.Invoke(eventData.delta);
      if(image)
        image.color = simpleColor;
    }
}
