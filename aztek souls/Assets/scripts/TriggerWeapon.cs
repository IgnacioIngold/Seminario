using UnityEngine;
using Core.Entities;

public abstract class Weapon : MonoBehaviour
{
    public GameObject Owner;
    protected object[] getOwnerStats()
    {
        return Owner.GetComponent<IAttacker<object[]>>() != null ?
            Owner.GetComponent<IAttacker<object[]>>().GetDamageStats()
            : new object[1];
    }
}

[AddComponentMenu("Core/Trigger Weapon"), RequireComponent(typeof(Collider))]
public class TriggerWeapon : Weapon
{
    Collider col;

    private void Awake()
    {
        col = GetComponent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        IKilleable KilleableObject = other.GetComponent<IKilleable>();

        if (other.gameObject == Owner) return;

        if (KilleableObject != null && KilleableObject.IsAlive)
            KilleableObject.GetDamage(getOwnerStats());

        col.enabled = false;
    }
}
