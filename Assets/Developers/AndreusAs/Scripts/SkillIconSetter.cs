using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillIconSetter : MonoBehaviour
{

	public Object Anarchist;
	public Object Safari;
	public Object Antiquarian;
	public Object Chief;
	public Object Tech;
	public Object Verdun;

	public void SetSkillIcon(GameObject skillButton)
	{
		SkillComponent skillComponent = this.GetComponent<SkillComponent>();


		switch (skillComponent.GetSkillName())
		{
			case "Chief":
				Instantiate(Chief, skillButton.transform);
				Destroy(this);
				break;
			case "Safari":
				Instantiate(Safari, skillButton.transform);
				Destroy(this);
				break;
			case "Antiquarian":
				Instantiate(Antiquarian, skillButton.transform);
				Destroy(this);
				break;
			case "Anarchist":
				Instantiate(Anarchist, skillButton.transform);
				Destroy(this);
				break;
			case "Tech":
				Instantiate(Tech, skillButton.transform);
				Destroy(this);
				break;
			case "Verdun":
				Instantiate(Verdun, skillButton.transform);
				Destroy(this);
				break;

		}
	}


	


}
