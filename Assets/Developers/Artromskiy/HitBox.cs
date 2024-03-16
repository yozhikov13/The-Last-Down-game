using UnityEngine;
using Mirror;
[RequireComponent(typeof(Collider))]
public abstract class HitBox: MonoBehaviour
{
    public float dmgMult;
    public float critMult;
    public int critChance;
    public int deathChance;
    private MatchManager damageManager;
    private string owner;

    void Start()
    {
        owner = GetComponentInParent<NetworkIdentity>()?.netId.ToString("G");
        damageManager = FindObjectOfType<MatchManager>();
        Debug.Log(owner);
        Debug.Log(damageManager);
    }

    public abstract void TransportDamage(float damage, Weapon.ShootInfo shootInfo, bool ignoreArmor);

    protected void SendDamageInfo(float damage, Weapon.ShootInfo shootInfo)
    {
        damageManager?.LogDamage(damage, owner, shootInfo);
    }

    protected void SendKillInfo(Weapon.ShootInfo shootInfo)
    {
        damageManager?.LogKill(owner, shootInfo);
    }

    protected float CalculateDamage(float damage)
    {
        var rnd = Random.Range(0, 100);
        float res;
        if (rnd < critChance)
        {
            res = damage * critMult;
        }
        else if (rnd < critChance + deathChance)
        {
            res = damage * 10;
        }
        else
        {
            res = damage * dmgMult;
        }
        return res;
    }

    protected void SendHeadShotInfo(Weapon.ShootInfo shootInfo)
    {
        if (deathChance == 100)
            damageManager?.LogHeadShot(owner, shootInfo);
    }
}
