using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Mirror;

/// <summary>
/// Класс оружия имеющий полуавтоматическую стрельбу
/// </summary>
public class SemiAutoWeapon: Weapon
{
    private bool shooted = false;
    public override void StartShooting()
    {
        if(!shooted)
        {
            canShoot = true;
        }
    }

    public override void EndShooting()
    {
        shooted = false;
        canShoot = false;
    }

    protected override void Shoot()
    {
        base.Damage(Ray());
    }

    private Collider[] Ray()
    {
        var collider = new Collider[1];
        Physics.Raycast(shootPoint.position, RandomRay(), out RaycastHit hit, range);
        if (hit.collider!=null)
        {
            collider[0] = hit.collider;
        }
        return collider;
    }

    private void AfterShoot()
    {
        canShoot = false;
        shooted = true;
    }

    public override void Start()
    {
        base.Start();
        if(shootEvent!=null)
        {
            shootEvent.AddListener(AfterShoot);
        }
        shooted = false;
    }
}
