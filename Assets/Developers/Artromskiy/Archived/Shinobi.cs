using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.Events;

public class Shinobi : SkillComponent
{
    BuffManager manager;

    private void Start()
    {
        manager = gameObject.GetComponent<BuffManager>();
    }

    public override void Skill()
    {
        var buff = new ConditionBuff
        {
            noiseness = -1,
            speed = 2,
            callers = new List<UnityEvent>()
        };
        buff.condition = () =>
        {
            return manager.Player.Health <= 35;
        };
        buff.onStart = () =>
        {
            manager.Player.onDamage.AddListener(buff.Condition);
        };
        buff.onTrue = () =>
        {
            manager.Player.onDamage.RemoveListener(buff.Condition);
            manager.RemoveBuff(buff);
        };
        manager.AddBuff(buff);
    }
}