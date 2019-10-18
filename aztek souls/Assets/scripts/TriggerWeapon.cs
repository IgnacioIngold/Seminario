using UnityEngine;
using System.Collections.Generic;
using System;
using Core;
using Core.Entities;

//[AddComponentMenu("Core/Trigger Weapon"), RequireComponent(typeof(Collider))]
public class TriggerWeapon : MonoBehaviour
{
    [SerializeField, Tooltip("El collider es desactivado al producirse el primer impacto.")]
    protected Collider col;
    public GameObject Owner;
    public bool debugThisUnit;

    IDamageable<HitData, HitResult> _owner;

    void Awake()
    {
        var getted = Owner.TryGetComponent(out _owner);
        if (getted)
            Debug.LogWarning("Encontrado. " + Owner.gameObject.name);
        else
            Debug.LogWarning("No encontrado");

        if (col == null)
            col = GetComponent<Collider>();
    }

    HitData getOwnerStats()
    {
        return _owner.GetDamageStats();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == Owner) return;

        IDamageable<HitData, HitResult> Target = other.GetComponent<IDamageable<HitData, HitResult>>();
        if (Target != null)
        {
            if (debugThisUnit)
                print(string.Format("Owner: {0}, Colisionó con el siguiente Objeto: {1}", Owner.name, other.gameObject.name));

            _owner.FeedHitResult(Target.Hit(getOwnerStats()));
            return;
        }
    }
}
