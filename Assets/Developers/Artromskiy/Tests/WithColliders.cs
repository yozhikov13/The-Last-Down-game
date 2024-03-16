using UnityEngine;

public class WithColliders : MonoBehaviour
{
    public bool canJump
    {
		get
        {
			return (!obstacle[0] && !obstacle[1] && obstacle[2]) ||
			(!obstacle[0] && obstacle[1] && obstacle[2]);
		}
    }
    public bool knifeAttackAvailable
    {
		get
        {
			return enemy[1];
        }
    }
	public bool[] obstacle = new bool[3];
	public bool[] enemy = new bool[3];
}
