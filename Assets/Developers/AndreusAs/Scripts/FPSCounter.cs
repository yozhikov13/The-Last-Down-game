using System.Collections;
using System;
using UnityEngine;
using UnityEngine.UI;

public class FPSCounter : MonoBehaviour
{

	private Text txt;

	private void Awake()
	{
		txt = this.GetComponent<Text>();
	}

	private void Start()
	{
		StartCoroutine(FPSCicle());
	}

	IEnumerator FPSCicle()
	{
		while(true)
		{
			try
			{
				txt.text = (1 / Time.smoothDeltaTime).ToString("0");
			}
			catch(Exception e)
			{

			}
			yield return new WaitForSeconds(1f);
		}
	}
}
