using MatchMaking;
using UnityEngine;

namespace MatchMaking
{
public class ReticlePositioner : MonoBehaviour
{
	[SerializeField]
	private InputManager im;

	private void Start()
	{


		RectTransform rTrans = this.GetComponent<RectTransform>();
		Vector3 vec = new Vector3(Screen.width/2, Screen.height/2, 0);


		rTrans.position = vec;


	}

	private void Update()
	{
		if (im && Camera.main != null)
		{
			if(im.player.isTps)
			{
				if(Physics.Raycast(im.player.weaponSet.shootPoint.position, im.player.weaponSet.shootPoint.forward, out RaycastHit hit, 500))
				{
					transform.position = Camera.main.WorldToScreenPoint(hit.point);
				}
				else
				{
					transform.position = Camera.main.WorldToScreenPoint(im.player.weaponSet.shootPoint.position + im.player.weaponSet.shootPoint.forward * 500);
				}
			}
			else
			{
				transform.position = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);
			}
		}
	}
}
}
