using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shell : NetworkBehaviour
{
    public float damage;
    public float range;
    public float speed;
    public Weapon.ShootInfo shootInfo;

    public virtual void Start()
    {
        GetComponent<Rigidbody>()?.AddForce(transform.forward * speed, ForceMode.Impulse);
    }

    public virtual void OnCollisionEnter(Collision collision)
    {
        var colliders = Physics.OverlapSphere(transform.position, range);
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] == null)
                continue;
            if (Physics.Raycast(transform.position, colliders[i].transform.position - transform.position, range))
                colliders[i].GetComponent<HitBox>()?.TransportDamage(damage, shootInfo, false);
        }
        Debug.Log("Before destroy");
        NetworkServer.Destroy(gameObject);
    }
}
