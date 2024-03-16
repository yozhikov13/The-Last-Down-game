using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarInfo : MonoBehaviour
{

	public GameObject charArms;
	public Animator charAnimator;


	public AvatarInfo (GameObject CharArms, Animator CharAnim)
	{

		charArms = CharArms;
		charAnimator = CharAnim;
	
	}
}
