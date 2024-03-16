using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public  class SteelArms : Weapon
{





    bool canAttack;

    List<GameObject> Attacked = new List<GameObject>();
    PlayerClass playerClass;
    //string Author;
    public override void Start()
    {
      base.Start();
    }



    public override void StartShooting()
    {
        canAttack = true;
        Attacked.Clear();
    }

    public override void EndShooting()
    {
        canAttack = false;
    }

    protected override void Shoot()
    {

    }

    void Damage(Collider col)
    {
        if(col !=null)
        {
            var CharHB = col.GetComponent<CharacterHitBox>();
            if(CharHB!=null&& transform.root.gameObject != CharHB.gameObject.transform.root.gameObject)
            {
                CharHB.TransportDamage(damage, shootInfo, true);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.transform.root.GetComponentInChildren<CharacterHitBox>()!=null && !Attacked.Contains(other.transform.root.gameObject)&&canAttack)
        {
            Attacked.Add(other.transform.root.gameObject);
            Damage(other);
        }
    }



}
