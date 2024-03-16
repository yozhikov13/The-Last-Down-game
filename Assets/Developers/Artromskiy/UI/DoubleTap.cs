using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class DoubleTap : MonoBehaviour, IPointerClickHandler
{
    public UnityEvent onDoubleTap;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.clickCount == 2)
            onDoubleTap.Invoke();
    }
}
