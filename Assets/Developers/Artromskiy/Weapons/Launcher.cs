using Mirror;
using UnityEngine;

public class Launcher : SemiAutoWeapon
{
    public Shell rocket;
    protected override void Shoot()
    {
        Launch();
    }

    private void Launch()
    {
        if(rocket!=null)
        {
            var r = Instantiate(rocket, shootPoint.position, Quaternion.LookRotation(RandomRay()));
            NetworkServer.Spawn(r.gameObject);
            r.speed = range;
            r.damage = damage;
            r.range = Mathf.Max(range, 1);
            r.shootInfo = base.shootInfo;
        }
    }
}
