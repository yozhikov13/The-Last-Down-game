using UnityEngine;

public class WithRaycast : MonoBehaviour
{
	public SphereCaster[] frontalSensors;
	public bool canJump;
	public bool knifeAttackAvailable;
	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
	{
		//if ((!frontalSensors[0].obstacle && !frontalSensors[1].obstacle && frontalSensors[2].obstacle) ||
		//	(!frontalSensors[0].obstacle && frontalSensors[1].obstacle && frontalSensors[2].obstacle))
		//{
		//	canJump = true;
		//}
		//else if ((frontalSensors[0].obstacle && frontalSensors[1].obstacle && frontalSensors[2].obstacle) ||
		//		(!frontalSensors[0].obstacle && !frontalSensors[1].obstacle && !frontalSensors[2].obstacle))
		//{
		//	canJump = false;
		//}

		//if (!frontalSensors[1].enemy)
		//{
		//	knifeAttackAvailable = false;
		//}
		//else if (frontalSensors[1].enemy)
		//{
		//	knifeAttackAvailable = true;
		//}

	}
}
