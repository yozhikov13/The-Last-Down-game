using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shotgun : SemiAutoWeapon
{
    public override void Start()
    {
        base.Start();
    }


    protected override void Shoot()
    {
        base.Damage(Ray());
    }

    private Collider[] Ray()
    {
        var collider = new Collider[10];
        for (int i = 0; i < 10; i++)
        {
            Physics.Raycast(shootPoint.position, RandomRay(), out RaycastHit hit, range);
            collider[i] = hit.collider;
        }
        return collider;
    }
    public override void StartShooting()
    {
        base.StartShooting();
    }
    public override void EndShooting()
    {
        base.EndShooting();
    }
}
