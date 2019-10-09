using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SE_AnimEventListener : MonoBehaviour
{
    public Collider coll;
    public ParticleSystem marker;
    public ParticleSystem ShieldSparks;

    ShieldEnemy Owner;

    private void Awake()
    {
        //coll = GetComponent<Collider>();
        Owner = GetComponentInParent<ShieldEnemy>();
        Owner.OnDie += () => { coll.enabled = false; };
        Owner.onGetHit += () => { coll.enabled = false; };
    }

    public void EnableDamage()
    {
        coll.enabled = true;
    }

    public void DisableDamage()
    {
        coll.enabled = false;
    }
    public void Parryed()
    {
       
        ShieldSparks.Play();
    }
    
    public void hiteable()
    {
        marker.Play();
    }
    public void EndAtk(int index)
    {
       //1= primer ataque
       //2 =segundo
       //3 = tercero
    }
   
}
