using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Avenger : SkillComponent
{
    PlayerClass player;
    public override void Skill()
    {
        if(player == null)
        {
            player = gameObject.GetComponent<PlayerClass>();
        }
        StartCoroutine(Healing());
    }

    IEnumerator Healing()
    {
        for (int i = 0; i < 4; i++)
        {
            if(player!=null)
            {
                player.Health += 10;
            }
            yield return new WaitForSeconds(1);
        }
    }
}
