using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class ConditionBuff : CharacterBuff
{
    public List<UnityEvent> callers;
    public Func<bool> condition;
    public Action onStart;
    public Action onTrue;
    public Action onFalse;

    public void OnStart()
    {
        if (onStart == null)
            return;
        onStart.Invoke();
    }
    public void Condition()
    {
        if (condition == null)
            return;
        if (condition.Invoke())
            OnTrue();
        else
            OnFalse();


    }
    public void OnTrue()
    {
        if (onTrue != null)
        {
            onTrue.Invoke();
        }
    }
    public void OnFalse()
    {
        if (onFalse != null)
        {
            onFalse.Invoke();
        }
    }
}
