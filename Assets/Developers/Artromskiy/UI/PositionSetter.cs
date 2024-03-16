using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Image))]
public class PositionSetter : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
	public Vector2Event objectToMove;
	public void OnPointerDown(PointerEventData eventData)
	{
		objectToMove?.Invoke(eventData.position);
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		objectToMove?.Invoke(eventData.position);
	}
}
