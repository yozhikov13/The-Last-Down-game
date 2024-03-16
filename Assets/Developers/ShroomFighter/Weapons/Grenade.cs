using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class Grenade : Shell
{
    Grenade(Vector3 throwvector)
    {
        ThrowVector = throwvector;
    }
    Coroutine countdown;
    public Vector3 ThrowVector;
    // Start is called before the first frame update
    public override void Start()
    {
        GetComponent<Rigidbody>()?.AddForce(ThrowVector* speed,ForceMode.Impulse);
        countdown = StartCoroutine(Countdown()); 
    }

    IEnumerator Countdown()
    {
        yield return new WaitForSeconds(5f);
        Explode();
    }
    
    void Explode()
    {
        var colliders = Physics.OverlapSphere(transform.position, range);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] == null)
                continue;
            if (Physics.Raycast(transform.position, colliders[i].transform.position - transform.position, range))
                colliders[i].GetComponent<CharacterHitBox>()?.TransportDamage(damage, shootInfo, false);
        }
        Debug.Log("Before destroy");
        NetworkServer.Destroy(gameObject);
    }
}
