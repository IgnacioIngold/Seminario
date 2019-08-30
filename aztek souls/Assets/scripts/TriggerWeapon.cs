﻿using UnityEngine;
using Core.Entities;
using System.Collections.Generic;

public abstract class HitTrigger : MonoBehaviour
{
    [SerializeField]
    protected Collider col;
    public GameObject Owner;
    public GameObject OnHitParticle;
    protected object[] getOwnerStats()
    {
        return Owner.GetComponent<IAttacker<object[]>>() != null ?
            Owner.GetComponent<IAttacker<object[]>>().GetDamageStats()
            : new object[1];
    }
}

//[AddComponentMenu("Core/Trigger Weapon"), RequireComponent(typeof(Collider))]
public class TriggerWeapon : HitTrigger
{
    public bool debugThisUnit;

    //private void Awake()
    //{
    //    col = GetComponent<Collider>();
    //}

    private void OnTriggerEnter(Collider other)
    {
        IKilleable KilleableObject = other.GetComponent<IKilleable>();

        if (other.gameObject == Owner) return;

        if (KilleableObject != null && KilleableObject.IsAlive)
        {
            if (debugThisUnit)
                print("Colisiono con algo we: " + other.gameObject.name);

            KilleableObject.GetDamage(getOwnerStats());
        }

        if (KilleableObject != null && other.gameObject != Owner)
        {
            GameObject particle = Instantiate(OnHitParticle, transform.position, Quaternion.identity);
            Destroy(particle, 3f);
        }

        col.enabled = false;
    }
}
