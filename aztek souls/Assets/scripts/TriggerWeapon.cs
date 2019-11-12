using UnityEngine;
using Core.Entities;
using System.Collections.Generic;
using System;

//[AddComponentMenu("Core/Trigger Weapon"), RequireComponent(typeof(Collider))]
public class TriggerWeapon : MonoBehaviour
{
    [SerializeField, Tooltip("El collider es desactivado al producirse el primer impacto.")]
    protected Collider col;
    public GameObject Owner;
    public bool debugThisUnit;

    IAttacker<object[]> _owner;

    void Awake()
    {
        var getted = Owner.TryGetComponent(out _owner);
        //if (getted)
        //    Debug.LogWarning("Encontrado. " + Owner.gameObject.name);
        //else
        //    Debug.LogWarning("No encontrado");

        if (col == null)
            col = GetComponent<Collider>();
    }

    object[] getOwnerStats()
    {
        return _owner.GetDamageStats();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == Owner) return;

        IDamageable KilleableObject = other.GetComponent<IDamageable>();
        if (KilleableObject != null)
        {
            if (debugThisUnit)
                print(string.Format("Owner: {0}, Colisionó con el siguiente Objeto: {1}", Owner.name, other.gameObject.name));

            KilleableObject.GetDamage(getOwnerStats());
            return;
        }
    }
}
