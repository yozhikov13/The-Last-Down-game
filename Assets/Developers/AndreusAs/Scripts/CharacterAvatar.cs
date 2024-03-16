using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CharacterAvatar : NetworkBehaviour
{
	public GameObject clientSideAvatar;
	public GameObject remoteSideAvatar;

	private void Start()
	{
		ActivateClientAvatar(!isLocalPlayer);
	}

	public void ActivateClientAvatar(bool value)
	{
		if (remoteSideAvatar)
		{
			remoteSideAvatar.SetActive(value);
			clientSideAvatar.SetActive(!value);
		}
	}
}

