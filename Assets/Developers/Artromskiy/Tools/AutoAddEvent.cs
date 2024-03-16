using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AutoAddEvent : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var eff1 = GetComponentsInChildren<ParticleSystem>();
        var w = GetComponent<FullAutoWeapon>();
        if(w)
        {
            if(w.shootEvent != null)
            {
                for (int i = 0; i < eff1.Length; i++)
                {
                    w.shootEvent.AddListener(eff1[i].Play);
                }
            }
            else
            {
                w.shootEvent = new UnityEvent();
                for (int i = 0; i < eff1.Length; i++)
                {
                    w.shootEvent.AddListener(eff1[i].Play);
                }
            }
            w.StartShooting();
        }
        Destroy(this);
    }
}
