using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using Mirror;

public class PlayerMovement : NetworkBehaviour
{
	//#region Singleton_implementation
	//private static PlayerMovement _instance;
	//public static PlayerMovement Instance
	//{

	//	get
	//	{

	//		if (_instance != null)
	//			return _instance;
	//		return null;

	//	}

	//}
	//#endregion Singlton_implementation


	public int playerID;
	private Player player;

	private CharacterController charController;

	[SerializeField]
	private GameObject charArms;
	[SerializeField]
	private Animator charAnimator;

	[SerializeField]
	private float walkSpeed;
	[SerializeField]
	private float runSpeed;

	[SerializeField]
	private float rotateVerSpeed;
	[SerializeField]
	private float rotateHorSpeed;

	private void Awake()
	{
		//_instance = this;
		//InputManager.Instance.CmdSetPlayerMovementComponent(this);

	}


	void Start()
	{

		charController = this.GetComponent<CharacterController>();

	}



	void Update()
	{



	}

	[Command]
	public void CmdGravity ()
	{

		this.charController.Move(new Vector3(0, -1, 0));

	}

	/// <summary>
	/// Эта функция двигает игрока на сервере.
	/// </summary>
	/// <param name="vec"></param>
	[Command]
	public void CmdMove(Vector2 vec)
	{
		if (vec.x > 1 || vec.y > 1)
			vec = new Vector2(1, 1);

		if (vec.x < -1 || vec.y < -1)
			vec = new Vector2(-1, -1);



		if (vec.y >= 0.6)
		{

			if (!charAnimator.GetBool("Run"))
			{
				if (charAnimator.GetBool("Walk"))
				{
					charAnimator.SetBool("Walk", false);
					charAnimator.SetBool("Run", true);
				}
				else 
				{
					charAnimator.SetBool("Run", true);
				}
			}

			charController.Move(Vector3.ClampMagnitude(transform.TransformDirection(new Vector3(vec.x * walkSpeed * Time.deltaTime, 0, vec.y * runSpeed * Time.deltaTime)), runSpeed));
		}
		else if (vec.y < 0.6)
		{

			if (!charAnimator.GetBool("Walk"))
			{
				if (charAnimator.GetBool("Run"))
				{
					charAnimator.SetBool("Run", false);
					charAnimator.SetBool("Walk", true);
				}
				else
				{
					charAnimator.SetBool("Walk", true);
				}
			}

			charController.Move(Vector3.ClampMagnitude(transform.TransformDirection(new Vector3(vec.x * walkSpeed * Time.deltaTime, 0, vec.y * walkSpeed * Time.deltaTime)), walkSpeed));
		}

	}

	/// <summary>
	/// Эта функция выключает анимации ходьбы и бега после того как пользователь отпускает стик движения
	/// </summary>
	public void CmdStopMovingAnim()
	{

		if (charAnimator.GetBool("Walk"))
		{

			charAnimator.SetBool("Walk", false);

		}

		if (charAnimator.GetBool("Run"))
		{

			charAnimator.SetBool("Run", false);

		}

	}


	/// <summary>
	/// Эта функция управляет камерой игрока на сервере.
	/// </summary>
	/// <param name="vec"></param>
	[Command]
	public void CmdRotateCamera (Vector2 vec)
	{

		Vector3 rotVecCam = new Vector3(charArms.transform.localEulerAngles.x + Mathf.Clamp((-vec.y * rotateVerSpeed * Time.deltaTime), -45, 45), 
										charArms.transform.localEulerAngles.y, 
										charArms.transform.localEulerAngles.z);

		charArms.transform.localEulerAngles = (rotVecCam);


		Vector3 rotVecChar = new Vector3(transform.localEulerAngles.x,
										 transform.localEulerAngles.y + (vec.x * rotateHorSpeed * Time.deltaTime),
										 transform.localEulerAngles.z);

		transform.localEulerAngles = (rotVecChar);

	}
}
