using Mirror;
using System;
using UnityEngine;
using UnityEngine.Events;

public class HealthSystem : NetworkBehaviour
{
    [SerializeField]
    private float health;
    [SerializeField]
    private float armor;

    [SerializeField]
    UnityEvent deadUnityEvent;
    delegate void OnDeadDelegate();
    [SyncEvent]
    event OnDeadDelegate EventDead;

    [SerializeField]
    UnityEvent healthChangedUnityEvent;
    delegate void OnHealthChangeDelegate();
    [SyncEvent]
    event OnHealthChangeDelegate EventHealthChange;
    public float Health
    {
        get
        {
            return health;
        }
        set
        {
            if(value!=health)
                EventHealthChange.Invoke();
            if (health <= 0)
                EventDead.Invoke();
            health = value;
        }
    }

    public float Armor
    {
        get
        {
            return armor;
        }
        set
        {
            armor = value;
        }
    }

    public override void OnStartServer()
    {
        var l = healthChangedUnityEvent.GetPersistentEventCount();
        for (int i = 0; i < l; i++)
        {
            var r = healthChangedUnityEvent.GetPersistentTarget(i);
            var action = (Action)r.GetType().GetMethod(healthChangedUnityEvent.GetPersistentMethodName(i)).CreateDelegate(typeof(Action), r);
            EventHealthChange += action.Invoke;
        }
        l = deadUnityEvent.GetPersistentEventCount();
        for (int i = 0; i < l; i++)
        {
            var r = deadUnityEvent.GetPersistentTarget(i);
            var action = (Action)r.GetType().GetMethod(deadUnityEvent.GetPersistentMethodName(i)).CreateDelegate(typeof(Action), r);
            EventDead += action.Invoke;
        }
    }

    public void Destroy()
    {
        NetworkServer.Destroy(gameObject);
    }
}
