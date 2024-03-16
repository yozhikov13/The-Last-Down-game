using UnityEngine;
using System.Collections;
using System;

public class Scientist : SkillComponent
{

    public override void Skill()
    {
        // основной бафф
        var manager = gameObject.GetComponent<BuffManager>();
        ConditionBuff buff = new ConditionBuff
        {
            armor = 1,
            noiseness = 0,
            speed = 1
        };
        buff.onStart = () =>
        {
            StartCoroutine(Timer(buff.Condition));
        };
        buff.condition = () =>
        {
            return true;
        };
        // баф, который добавляется при удалении первого бафа
        buff.onTrue = () =>
        {
            var buff2 = new CharacterBuff()
            {
                speed = -1,
                armor = -1,
                noiseness = 1
            };
            manager.AddBuff(buff2);
            manager.RemoveBuff(buff);
        };
        manager.AddBuff(buff);

    }

    public IEnumerator Timer(Action cond)
    {
        yield return new WaitForSeconds(7);
        cond.Invoke();
    }
}
