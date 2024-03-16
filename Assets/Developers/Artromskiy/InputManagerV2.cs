using UnityEngine;
using UnityEngine.EventSystems;
using MatchMaking;

public class InputManagerV2 : MonoBehaviour
{
	public PlayerClass pClass;

	private MoveController mc;
	private MoveController Mc
	{
		get
		{
			if (mc == null)
				mc = pClass.GetComponent<MoveController>();
			return mc;
		}
	}

	private WeaponSet ws;
	private WeaponSet Ws
	{
		get
		{
			if (ws == null)
				ws = pClass.GetComponent<WeaponSet>();
			return ws;
		}
	}

	#region Move_functions
	public void MoveInfo(Vector2 vec)
	{
		if(Mc)
		{
//			mc.CmdSetSpeed(vec);
		}
	}


	public void MoveTouchEnded()
	{
		if(Mc)
		{
//			mc.CmdSetSpeed(Vector2.zero);
		}
	}

	public void Jump()
	{
		/*
		TODO
		*/
	}

	public void Crouch()
	{
		/*
		TODO
		*/
	}
	#endregion Move_functions

	#region Rotate_camera_functions


	public void RotateCommand (Vector2 vec)
	{
		if(Mc)
        {
//			mc.CmdSetRotation(vec);
			var rot = Camera.main.transform.eulerAngles - new Vector3(vec.y, 0, 0) / 2;
			Camera.main.transform.eulerAngles = rot;
        }
	}



	#endregion Rotate_camera_functions

	#region Weapon_functions

	public void Shoot()
	{
		if(Ws)
			ws.ClientShoot();
	}

	public void StopShoot()
	{
		if(Ws)
			ws.ClientStopShoot();
	}

	public void Reload()
	{
		ws.ClientReload();
	}

	public void ChangeWeapon(bool right)
	{
		if(Ws)
        {
			if (right)
				ws.NextWeapon();
			else
				ws.PrevWeapon();
        }
	}

	public void KnifeSlash()
	{
		/*
		TODO
		*/
	}


	#endregion Weapon_functions
}
