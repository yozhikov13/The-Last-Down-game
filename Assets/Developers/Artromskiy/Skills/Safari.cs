using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Safari : SkillComponent
{
    private PlayerWeapon pw;
    private PlayerWeapon Pw
    {
        get
        {
            if (pw == null)
                pw = GetComponent<PlayerWeapon>();
            return pw;
        }
    }

	private void OnEnable()
	{
		this.SetSkillName("Safari");
	}

	public override void Skill()
    {

    }

    private void CmdSkill()
    {
        InvokeFX();
    }
}
