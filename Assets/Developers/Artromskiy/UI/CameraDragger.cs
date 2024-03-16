using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[Serializable]
public class Vector2Event:UnityEvent<Vector2>
{

}
[RequireComponent(typeof(Image))]
public class CameraDragger : MonoBehaviour, IPointerUpHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
	public Vector2Event onStartDrag;
	public Vector2Event onDrag;

	#region EventSystem functions

	public void OnPointerUp(PointerEventData eventData)
	{
		Debug.Log("PointerUp");
	}

	public void OnBeginDrag(PointerEventData eventData)
	{
		Debug.Log("PointerBeginDrag");
		onStartDrag?.Invoke(Vector2.one);
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		onDrag?.Invoke(Vector2.zero);
	}

	public void OnDrag(PointerEventData eventData)
	{
		onDrag?.Invoke(eventData.delta);
	}
	#endregion
}
