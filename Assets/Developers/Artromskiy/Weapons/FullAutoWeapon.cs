using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/// <summary>
/// Класс оружия имеющий автоматическую стрельбу
/// </summary>
public class FullAutoWeapon : Weapon
{


	[ContextMenu("Shoot")]
    public override void StartShooting()
    {
        canShoot = true;
    }

    [ContextMenu("EndShoot")]
    public override void EndShooting()
    {
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
        if(hit.collider != null)
        {
						Debug.Log(hit.collider.gameObject);
            collider[0] = hit.collider;
        }
        return collider;
    }
}
