using UnityEngine;
using Core.Entities;

[AddComponentMenu("Core/Physic Weapon"), RequireComponent(typeof(Collider))]
public class PhysicWeapon : HitTrigger
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == Owner) return;

        IKilleable KilleableObject = collision.gameObject.GetComponent<IKilleable>();
        if (KilleableObject != null)
        {
            if (KilleableObject.IsAlive)
                KilleableObject.GetDamage(getOwnerStats());
        }

        col.enabled = false;
    }
}
