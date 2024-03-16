using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager : MonoBehaviour

{


	[SerializeField]
	private GameObject MainCanvas;
	[SerializeField]
	private GameObject MovementJoystick;
	[SerializeField]
	public GameObject sprintIndicator;
	[SerializeField]
	public PlayerClass pClass;
	public GameObject jumpButton;
	public GameObject knifeButton;
	public CameraController camController;

	/// <summary>
	/// Функция включает/выключает игровой UI.
	/// </summary>
	/// <param name="val"></param>
	public void ToogleUI(bool val)
	{
		MainCanvas.SetActive(val);
	}

	#region Move_functions
	public void MoveInfo (Vector2 vec)
	{
		if (pClass.pMove != null && pClass.netID.hasAuthority)
		{

			if (pClass.pMove.isAutoSprinting)
			{

				pClass.pMove.ClientSetAutoSprint(false);
				pClass.pMove.CmdSetAutoSprint(false);

			}

			if (vec.magnitude > 0)
			{

				if (pClass.netID.hasAuthority && pClass.netID.isLocalPlayer)
				{
					pClass.pMove.Move(true, vec);
				}
			}
		}
	}

	public void ChangeVision()
    {
		//camController?.ChangeVision();
    }

	public void MoveTouchEnded()
	{
		if (pClass.pMove != null && pClass.netID.hasAuthority && !pClass.pMove.isAutoSprinting && pClass.pMove.isSprinting)
		{
				pClass.pMove.Move(false, Vector2.zero);
				pClass.pMove.CmdStopMovingAnim();
				pClass.pMove.ClientStopMovingAnim();
		}
		else if (pClass.pMove != null && pClass.netID.isClient)
		{
				pClass.pMove.Move(false, Vector2.zero);
				pClass.pMove.CmdStopMovingAnim();
				pClass.pMove.ClientStopMovingAnim();
		}

	}

	public void Jump()
	{
		if (pClass.pMove != null && pClass.netID.hasAuthority)
		{
			pClass.pMove.CmdJump();
		}
	}

	public void Crouch()
	{
		if (pClass.pMove != null && pClass.netID.hasAuthority)
		{
			pClass.pMove.ClientCrouch();
			pClass.pMove.CmdCrouch();
		}
	}
#endregion Move_functions

#region Rotate_camera_functions


	public void RotateCommand (Vector2 vec)
	{


		if (pClass.pMove != null && pClass.hasAuthority && pClass.isLocalPlayer)
		{
			pClass.pMove.ClientRotateCamera(vec);
		}

	}



#endregion Rotate_camera_functions

#region Weapon_functions

	public void Shoot()
	{
		pClass.pWeapon.ClientShoot();
	}

	public void StopShoot()
	{
		pClass.pWeapon.ClientStopShoot();
	}

	public void Reload()
	{
		pClass.pWeapon.ClientReload();
	}

	public void ChangeWeapon(bool right)
	{
		if (pClass.pWeapon != null && pClass.pWeapon.hasAuthority)
		{
			pClass.pWeapon.ClientChangeWeapon(right);
			pClass.pWeapon.CmdChangeWeapon(right);
		}
	}

	public void KnifeSlash()
	{
		pClass.pWeapon.ClientKnifeSlash();
	}


	#endregion Weapon_functions
}
