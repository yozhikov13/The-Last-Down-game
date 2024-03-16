using System.Collections;
using System;
using UnityEngine;
using Mirror;

public class Chief : SkillComponent
{
    PlayerClass player;
    PlayerClass Player
    {
        get
        {
            if(player == null)
                player = GetComponent<PlayerClass>();
            return player;
        }
    }

	private void OnEnable()
	{
		this.SetSkillName("Chief");
	}

	[ContextMenu("Skill")]
    public override void Skill()
    {
        CmdSkill();
    }

    private void CmdSkill()
    {
        if (curCount > 0)
        {
            curCount--;
            StartCoroutine(RollBack());
            StartCoroutine(Action());
        }
    }

    private IEnumerator Action()
    {
        for (int i = 0; i < useTime; i++)
        {
            InvokeFX();
            if(Player!=null)
                Player.Health += 10;
            yield return new WaitForSeconds(1);
        }
    }

    private IEnumerator RollBack()
    {
        yield return new WaitForSeconds(rollbackTime);
        curCount = Mathf.Min(curCount + 1, maxCount);
    }
}
