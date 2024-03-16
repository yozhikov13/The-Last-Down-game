using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretShooting :  Weapon
{
    [HideInInspector]
    public bool CanShoot, flag;
    [Header("Дальность полета пули")]
    public float BulletRange = 20f;
    [SerializeField]
    private Transform LeftTrunk, RightTrunk;
    // Start is called before the first frame update
    public override void Start()
    {
        flag = false;
        base.Start();
        base.shootEvent.AddListener(change);
    }



    private void change()
    {
        flag = !flag;
        if(flag)
        {
            shootPoint = LeftTrunk;

        }
        else
        {
            shootPoint = RightTrunk;
        }
    }
    public Collider[] Ray()
    {
        Collider[] collider = new Collider[1];
        Physics.Raycast(shootPoint.position, RandomRay(), out RaycastHit hit, range);
        if (hit.collider != null)
        {
            collider[0] = hit.collider;
        }
        return collider;
    }
    protected override void Shoot()
    {
        base.Damage(Ray());
    }

    public override void StartShooting()
    {
        canShoot = true;
    }

    public override void EndShooting()
    {
        canShoot = false;
    }
}
