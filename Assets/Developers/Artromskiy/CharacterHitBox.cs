using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MatchMaking;

public class CharacterHitBox : HitBox
{

    private Player character;

    private void Start()
    {
        character = gameObject.GetComponentInParent<Player>();
    }

    public override void TransportDamage(float damage, Weapon.ShootInfo shootInfo, bool ignoreArmor)
    {
        if (character == null)
            return;

        var res = CalculateDamage(damage);
        if (!ignoreArmor)
        {
            res -= character.Armor;
        }
        res = Mathf.Max(res, 1);
        var remain = character.Health - res;
        if (remain > 0)
        {
            SendDamageInfo(res, shootInfo);
            character.Health = remain;
        }
        else
        {
            SendDamageInfo(character.Health, shootInfo);
            character.Health = 0;
            SendKillInfo(shootInfo);
        }
        SendHeadShotInfo(shootInfo);
    }
}
