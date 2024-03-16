using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using MatchMaking;

public class Raggdoll_Controller : NetworkBehaviour
{

	private Player pClass;
	public Rigidbody[] rb;

	private void Start()
	{

		pClass = this.GetComponent<Player>();
		pClass.OnDead += EnableRagdoll;

	}

	private void EnableRagdoll (Player pCl)
	{

		pCl.tpsAnim.enabled = false; 
		switchAllKinematic(false);
	}

	private void DisableRagdoll(Player pCl)
	{

		pCl.tpsAnim.enabled = true;
		switchAllKinematic(true);
	}
	private void switchAllKinematic(bool isKinematic)
    {
		foreach(Rigidbody item in rb)
        {
			item.isKinematic = isKinematic;
        }
    }

}
