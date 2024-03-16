using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Landmine : Shell
{
    Coroutine checkland;
    bool IsActive;
    
    public override void Start()
    {
        IsActive = false;
        speed = 0;
        checkland = StartCoroutine(CheckLand());
    }
    
    IEnumerator CheckLand()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        yield return new WaitForSeconds(1f);
        while(rb.velocity.x!=0&& rb.velocity.y != 0&& rb.velocity.z != 0)
        {
            yield return null;
        }
        rb.constraints = RigidbodyConstraints.FreezeAll;
        IsActive = true;
    }

    public override void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);
    }

}
