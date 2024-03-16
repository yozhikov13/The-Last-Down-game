using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barricade : HitBox
{
    float health = 200;
    float armor = 5;
    public override void TransportDamage(float damage, Weapon.ShootInfo shootInfo, bool ignoreArmor)
    {
        damage = CalculateDamage(damage);
        if(!ignoreArmor)
        {
            damage -= armor;
        }
        damage = Mathf.Max(damage, 1);
        health -= CalculateDamage(damage);
        if(health<=0)
        {
            Destroy(gameObject);
        }
    }
}
