using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthHitBox : HitBox
{
    HealthSystem HS;

    public void Start()
    {
        HS = gameObject.GetComponentInParent<HealthSystem>();
    }

    public override void TransportDamage(float damage, Weapon.ShootInfo shootInfo, bool ignoreArmor)
    {

        var res = CalculateDamage(damage);
        if (!ignoreArmor)
        {
            res -= HS.Armor;
        }
        res = Mathf.Max(res, 1);
        res = HS.Health - res;
        HS.Health = res;
    }
}
