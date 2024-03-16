using UnityEngine;
using Mirror;
using System.Collections;

public class ExpPlayerMovement : NetworkBehaviour
{

	//	public int playerID;


	private float serverCharRotationX = 0;
	private float serverCharRotationY = 0;

	private PlayerClass pClass;
	private CharacterController charController;
//	private Transform groundSensor;

	private Vector3 lastLateUpdateRotation;
	public float jumpHeight;
	public float jumpLength;
	public float jumpTime;



	public bool FPAvatarActive;

	public SphereCaster[] frontalSensors;

	public Vector2 inputMovementVector { get; private set; }

	public Vector3 crouchPos;
	public Vector3 standPos;
	public Vector3 CrouchFPArmsPos;
	public Vector3 StandFPArmsPos;

	[SyncVar]
	public bool isJumping;
	[SyncVar]
	public bool isCrouching;
	[SyncVar]
	public bool canJump;
	[SyncVar]
	public bool canMove;
	[SyncVar]
	public bool knifeAttackAvailable;



	public bool isMoving { get; private set; }
	public bool isAutoSprinting { get; private set; }
	public bool isSprinting { get; private set; }

	public bool isRotating;


	void Start()
	{
		
		if(isLocalPlayer || isServer)
		{
			pClass = this.GetComponent<PlayerClass>();
			charController = this.GetComponent<CharacterController>();
			//groundSensor = transform.Find("groundSensor");
		}
	}


	private void Update()
	{


		if (isServer)
		{
			if (pClass.pSensors.GetGrounded)
			{
				if(pClass.CharAnimator.GetCurrentAnimatorStateInfo(0).IsName ("JumpLoop"))
				{

					pClass.CharAnimator.SetBool("isJumping", false);

				}

				canMove = true;
			}
			else
			{
				if (!pClass.CharAnimator.GetCurrentAnimatorStateInfo(0).IsName("JumpLoop"))
				{

					pClass.CharAnimator.SetBool("isJumping", true);
					pClass.CharAnimator.Play("JumpLoop");

				}

				canMove = false;
				
			}



			if (!isJumping)
            {
				this.charController.Move(new Vector3(0, -10, 0) * Time.deltaTime);
			}
		}

		if (isServer)
		{
			canJump = (pClass.pSensors.GetJump && !isCrouching) ? true : false;
			knifeAttackAvailable = pClass.pSensors.GetEnemy;
		}


		if (isMoving)
		{

			ClientMove(inputMovementVector);
			CmdMove(inputMovementVector);

		}

	}



	/// <summary>
	/// Функция отвечает за передачу вектора движения из InputManager и за включение/выключение флага движения.
	/// </summary>
	/// <param name="val"></param>
	/// <param name="moveVec"></param>
	public void Move(bool val, Vector2 moveVec)
	{
		if (isMoving != val)
		{
			isMoving = val;
		}


		inputMovementVector = moveVec;

	}


	/*
	 * Функция должна иметь атрибут Server и исполняться в функции Update
	 * 
	/// <summary>
	/// Функция, ответственная за притяжение персонажа к земле////TODO
	/// </summary>
	[Command]
	public void CmdGravity()
	{

			this.charController.Move(new Vector3(0, -10, 0)*Time.deltaTime);

	}
	*/

	/// <summary>
	/// Эта функция запускает анимацию движения на стороне клиента////TODO
	/// </summary>
	/// <param name="vec"></param>
	public void ClientMove(Vector2 vec)
	{
		if (!canMove)
			return;

		// Нормализация, если длина вектора больше 1
		if (vec.magnitude > 1)
			vec = vec.normalized;



		if (vec.y >= 0.8)
		{

			if (!pClass.CharAnimator.GetBool("Run"))
			{
				if (pClass.CharAnimator.GetBool("Walk"))
				{
					pClass.CharAnimator.SetBool("Walk", false);
					pClass.CharAnimator.SetFloat("WalkSpeed", 0);
					pClass.CharAnimator.SetBool("Run", true);

				}
				else
				{
					pClass.CharAnimator.SetBool("Run", true);
				}
			}

			isSprinting = true;


			charController.Move(Vector3.ClampMagnitude(transform.TransformDirection(new Vector3(vec.x * pClass.walkSpeed * Time.deltaTime, 0, vec.y * pClass.runSpeed * Time.deltaTime)), pClass.walkSpeed));

		}
		else
		{

			if (!pClass.CharAnimator.GetBool("Walk"))
			{
				if (pClass.CharAnimator.GetBool("Run"))
				{
					pClass.CharAnimator.SetBool("Run", false);
					isSprinting = false;

					if (Mathf.Abs(vec.x) >= Mathf.Abs(vec.y))
					{

						pClass.CharAnimator.SetFloat("WalkSpeed", Mathf.Abs(vec.x));

					}
					else
					{

						pClass.CharAnimator.SetFloat("WalkSpeed", Mathf.Abs(vec.y));

					}

					pClass.CharAnimator.SetBool("Walk", true);

				}
				else
				{
					if (Mathf.Abs(vec.x) >= Mathf.Abs(vec.y))
					{

						pClass.CharAnimator.SetFloat("WalkSpeed", Mathf.Abs(vec.x));

					}
					else
					{

						pClass.CharAnimator.SetFloat("WalkSpeed", Mathf.Abs(vec.y));

					}

					pClass.CharAnimator.SetBool("Walk", true);

				}
			}
			else
			{

				if (Mathf.Abs(vec.x) >= Mathf.Abs(vec.y))
				{

					pClass.CharAnimator.SetFloat("WalkSpeed", Mathf.Abs(vec.x));

				}
				else
				{

					pClass.CharAnimator.SetFloat("WalkSpeed", Mathf.Abs(vec.y));

				}

			}

			charController.Move(Vector3.ClampMagnitude(transform.TransformDirection(new Vector3(vec.x * pClass.walkSpeed * Time.deltaTime, 0, vec.y * pClass.walkSpeed * Time.deltaTime)), pClass.walkSpeed));
		}
	}

	/// <summary>
	/// Эта функция двигает игрока на сервере.
	/// </summary>
	/// <param name="vec"></param>
	[Command]
	public void CmdMove(Vector2 vec)
	{
		if (!canMove)
			return;
		{
			// Нормализация, если длина вектора больше 1
			if (vec.magnitude > 1)
				vec = vec.normalized;


			pClass.CharAnimator.SetFloat("Horizontal", vec.x);
			pClass.CharAnimator.SetFloat("Vertical", vec.y);

			if (vec.y >= 0.8)
			{


				isSprinting = true;

				charController.Move(Vector3.ClampMagnitude(transform.TransformDirection(new Vector3(vec.x * pClass.walkSpeed * Time.deltaTime, 0, vec.y * pClass.runSpeed * Time.deltaTime)), pClass.walkSpeed));
			}
			else
			{

				isSprinting = false;

				charController.Move(Vector3.ClampMagnitude(transform.TransformDirection(new Vector3(vec.x * pClass.walkSpeed * Time.deltaTime, 0, vec.y * pClass.walkSpeed * Time.deltaTime)), pClass.walkSpeed));

			}
		}
	}

	/// <summary>
	/// Эта фнукция включает флаг автобега на стороне клиента
	/// </summary>
	public void ClientSetAutoSprint (bool val)
	{

		isAutoSprinting = val;

	}

	/// <summary>
	/// Эта фнукция включает флаг автобега на стороне сервера
	/// </summary>
	[Command]
	public void CmdSetAutoSprint(bool val)
	{

		isAutoSprinting = val;

	}

	/// <summary>
	/// Эта функция выключает анимацию движения на стороне клиента///TODO
	/// </summary>
	public void ClientStopMovingAnim()
	{

		if (pClass.CharAnimator.GetBool("Walk"))
		{

			pClass.CharAnimator.SetBool("Walk", false);
			isSprinting = false;
			isAutoSprinting = false;
			pClass.CharAnimator.SetFloat("WalkSpeed", 0);

		}

		if (pClass.CharAnimator.GetBool("Run"))
		{

			pClass.CharAnimator.SetBool("Run", false);
			isSprinting = false;
			isAutoSprinting = false;
			pClass.CharAnimator.SetFloat("WalkSpeed", 0);

		}


	}

	/// <summary>
	/// Эта функция выключает анимации ходьбы и бега на стороне сервера после того как пользователь отпускает стик движения
	/// </summary>
	[Command]
	public void CmdStopMovingAnim()
	{

		pClass.CharAnimator.SetFloat("Horizontal", 0);
		pClass.CharAnimator.SetFloat("Vertical", 0);
		isSprinting = false;
		isAutoSprinting = false;


	}

	[Command]
	public void CmdJump()
	{

		isJumping = true;
		canMove = false;


		if(charController.isGrounded && canJump)
        {
			pClass.CharAnimator.Play("JumpStart");
			pClass.CharAnimator.SetBool("isJumping", true);
			StartCoroutine(JumpCicle(jumpLength, jumpHeight, jumpTime));

		}





	}

	public void ClientCrouch()
	{

		if (!isCrouching)
		{
			charController.height = 1.07f;
			charController.center = crouchPos;
			pClass.SetFPPosition(CrouchFPArmsPos);
			///TODO добавить анимацию приседания
		}
		else
		{

			charController.height = 1.82f;
			charController.center = standPos;
			pClass.SetFPPosition(StandFPArmsPos);
			///TODO добавить анимацию приседания

		}


	}

	[Command]
	public void CmdCrouch()
	{

		if (!isCrouching)
		{
			isCrouching = true;
			canJump = false;
			charController.height = 1.07f;
			charController.center = crouchPos;
			pClass.CharAnimator.Play("Crouch");
			///TODO добавить анимацию приседания
		}
		else
		{

			isCrouching = false;
			canJump = true;
			charController.height = 1.82f;
			charController.center = standPos;
			pClass.CharAnimator.Play("Idle");
			///TODO добавить анимацию приседания

		}




	}

	/// <summary>
	///
	/// </summary>
	/// <param name="vec"></param>
	public void ClientRotateCamera(Vector2 vec)
	{
		float vecX = serverCharRotationX;
		float vecY = serverCharRotationY;

		Vector3 rotVecCam = new Vector3(pClass.CharArms.transform.localEulerAngles.x + Mathf.Clamp((-vec.y * pClass.RotateVerSpeed * Time.deltaTime), -45, 45),
										pClass.CharArms.transform.localEulerAngles.y,
										pClass.CharArms.transform.localEulerAngles.z);

		pClass.CharArms.transform.localEulerAngles = (rotVecCam);
		pClass.cam.transform.localEulerAngles = (rotVecCam);



		Vector3 rotVecChar = new Vector3(transform.localEulerAngles.x,
										 transform.localEulerAngles.y + (vec.x * pClass.RotateHorSpeed * Time.deltaTime),
										 transform.localEulerAngles.z);

		transform.localEulerAngles = (rotVecChar);

		serverCharRotationX = vec.x * pClass.RotateHorSpeed * Time.deltaTime;
		serverCharRotationY = vec.y * pClass.RotateVerSpeed * Time.deltaTime;

		CmdRotateCamera(vecX, vecY);

	}

	/// <summary>
	/// Эта функция управляет камерой игрока на сервере.
	/// </summary>
	/// <param name="vec"></param>
	///[Command]

	[Command]
	public void CmdRotateCamera(float vecX, float vecY)
	{
		if (pClass.CharAnimator.GetFloat("VerticalCamera") > 45)
		{
			pClass.CharAnimator.SetFloat("VerticalCamera", 45);
		}
		else if (pClass.CharAnimator.GetFloat("VerticalCamera") < -45)
		{
			pClass.CharAnimator.SetFloat("VerticalCamera", -45);
		}
		else
		{
			pClass.CharAnimator.SetFloat("VerticalCamera", pClass.CharAnimator.GetFloat("VerticalCamera") + vecY);
		}


		Vector3 rotVecChar = new Vector3(transform.localEulerAngles.x,
										 transform.localEulerAngles.y + vecX,
										 transform.localEulerAngles.z);

		transform.localEulerAngles = (rotVecChar);

	}
	/// <summary>
	/// Служит основной функцией, описывающей направление прыжка
	/// </summary>
	/// <param name="length"></param>
	/// <param name="height"></param>
	/// <param name="tMain"></param>
	/// <returns></returns>
	private IEnumerator JumpCicle(float length, float height, float tMain)
	{
		float tCurr = 0;
		float prevY = 0;
		isJumping = true;
		while (tCurr < tMain || !charController.isGrounded)
		{
			var currY = (tCurr + Time.deltaTime) / tMain * length;
			charController.Move(transform.rotation * new Vector3(0, Parabola(length, height, prevY) - Parabola(length, height, currY), length / tMain * Time.deltaTime));
			prevY = currY;
			tCurr += Time.deltaTime;
			yield return null;
		}

		if(isServer)
			pClass.CharAnimator.SetBool("isJumping", false);

		isJumping = false;
		canMove = true;
		yield break;
	}

	private float Parabola(float length, float height, float x)
    {
		return ((4 * height * x) / length) * ((x / length) - 1) + height;
	}

}
