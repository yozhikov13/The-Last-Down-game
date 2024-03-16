using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SensorController : MonoBehaviour
{


	public bool[] obstacle;
	public bool enemy;
	public bool isGrounded;

	private void Start()
	{
		obstacle = new bool[3];
		this.transform.root.gameObject.GetComponent<PlayerClass>().pSensors = this;

	}

	public bool GetJump
	{

		get
		{

			return (!obstacle[0] && obstacle[1] && obstacle[2]) ||
				   (!obstacle[0] && !obstacle[1] && obstacle[2]);

		}

	}

	public bool GetEnemy	
	{

		get 
		{

			return enemy;

		}

	}

	public bool GetGrounded
	{

		get
		{

			return isGrounded;
  
		}

	}


}
