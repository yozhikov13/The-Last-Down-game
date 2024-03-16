using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Rewired;

public class AutoSprintElement : MonoBehaviour, IEndDragHandler, IPointerClickHandler
{
	[SerializeField]
	private InputManager im;
	private ExpPlayerMovement pMove;

	private Vector3 initialPosition;

	public void Start()
	{
		initialPosition = this.transform.position;
		pMove = im.pClass.pMove;
	}
	/// <summary>
	/// Проверяем, где игрок отпустил палец после того, как начал спринт. Если отпусти палец над объектом, к которому прикреплен компонент - включается
	/// автобег.
	/// </summary>
	/// <param name="eventData"></param>
	public void OnEndDrag(PointerEventData eventData)
	{
		Debug.Log("PointerEndDrag");
		RaycastResult rRes = eventData.pointerCurrentRaycast;

		if (rRes.gameObject != null)
		{
			if (rRes.gameObject.CompareTag("SprintIndicator"))
			{
				if (!pMove.isAutoSprinting && pMove.isSprinting)
				{
					pMove.ClientSetAutoSprint(true);
					pMove.CmdSetAutoSprint(true);
				}
				else
				{
					im.MoveTouchEnded();
				}
			}
			else
			{
				if (pMove.isAutoSprinting)
				{
					pMove.ClientSetAutoSprint(false);
					pMove.CmdSetAutoSprint(false);
				}
				im.MoveTouchEnded();
			}
		}
		else
		{
			if (pMove.isAutoSprinting)
			{
				
				pMove.ClientSetAutoSprint(false);
				pMove.CmdSetAutoSprint(false);
			}
			im.MoveTouchEnded();
		}
	}

	/// <summary>
	/// Если мы тапаем по джойстику (в любом его месте) во время автобега, автобег прерывается
	/// </summary>
	/// <param name="eventData"></param>
	public void OnPointerClick(PointerEventData eventData)
	{
		//Debug.Log("Click");
		RaycastResult rRes = eventData.pointerCurrentRaycast;

		if (rRes.gameObject.CompareTag("MoveJoystick") && pMove.isAutoSprinting)
		{
			//Debug.Log("StopAutoSprint");
			pMove.ClientSetAutoSprint(false);
			pMove.CmdSetAutoSprint(false);
			im.MoveTouchEnded();
		}
	}

	public void SetPosition(Vector2 vec)
	{
		if (transform.position != initialPosition)
		{
			transform.position = initialPosition;
		}
		else
		{
			transform.position = new Vector3(vec.x, vec.y, 0);
		}
	}
}
