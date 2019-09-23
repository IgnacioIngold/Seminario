using UnityEngine;
using Core.Entities;
using System.Collections.Generic;
using System;

public abstract class HitTrigger : MonoBehaviour
{
    [SerializeField, Tooltip("El collider es desactivado al producirse el primer impacto.")]
    protected Collider col;
    public GameObject Owner;
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

    private void Awake()
    {
        if (col == null)
            col = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == Owner) return;

        IDamageable KilleableObject = other.GetComponent<IDamageable>();
        if (KilleableObject != null)
        {
            if (debugThisUnit)
                print("Colisiono con algo we: " + other.gameObject.name);

            KilleableObject.GetDamage(getOwnerStats());
            return;
        }
    }
}
