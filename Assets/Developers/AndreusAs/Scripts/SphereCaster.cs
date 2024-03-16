using UnityEngine;

public class SphereCaster : MonoBehaviour
{
	public int index;
	public SensorController controller;

	private void Start()
	{
		if(index != 3)
			controller = this.transform.parent.gameObject.GetComponent<SensorController>();

	}

	private void OnTriggerEnter(Collider other)
	{

		if (other.gameObject != this.transform.root.gameObject)
		{
			if(index == 4)
				Debug.Log("notSelf");

			if (!other.gameObject.CompareTag("Character") && !other.gameObject.CompareTag("Hitbox")&&!other.gameObject.CompareTag("Sensor") && index == 4)
			{
				Debug.Log("isGroundedTrue");
				controller.isGrounded = true;

			}
			else if (other.gameObject.CompareTag("Character") && !other.gameObject.CompareTag("Hitbox") && index == 3)
			{

				controller.enemy = true;

			}
			else if (!other.gameObject.CompareTag("Hitbox") && !other.gameObject.CompareTag("Sensor") && index != 3)
			{

				controller.obstacle[index] = true;

			}
		}
		
	}

	private void OnTriggerStay(Collider other)
	{
		if (!other.gameObject.CompareTag("Character") && !other.gameObject.CompareTag("Hitbox") && !other.gameObject.CompareTag("Sensor") && index == 4)
		{
			Debug.Log("isGroundedTrue");
			controller.isGrounded = true;

		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject != this.transform.root.gameObject)
		{

			if (!other.gameObject.CompareTag("Character") && !other.gameObject.CompareTag("Hitbox") && index == 4)
			{
				Debug.Log("isGroundedFalse");
				controller.isGrounded = false;

			}
			else if (other.gameObject.CompareTag("Character") && !other.gameObject.CompareTag("Hitbox") && index == 3)
			{

				controller.enemy = false;

			}
			else if (!other.gameObject.CompareTag("Hitbox") && !other.gameObject.CompareTag("Sensor") && index != 3)
			{

				controller.obstacle[index] = false;

			}

		}
	}

}
