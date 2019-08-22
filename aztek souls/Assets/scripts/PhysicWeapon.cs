﻿using UnityEngine;
using Core.Entities;

[AddComponentMenu("Core/Physic Weapon"), RequireComponent(typeof(Collider))]
public class PhysicWeapon : Weapon
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject == Owner) return;

        IKilleable KilleableObject = collision.gameObject.GetComponent<IKilleable>();
        if (KilleableObject != null && KilleableObject.IsAlive)
            KilleableObject.GetDamage(getOwnerStats());
    }
}
