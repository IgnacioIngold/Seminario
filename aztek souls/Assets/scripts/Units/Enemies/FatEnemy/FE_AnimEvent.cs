using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FE_AnimEvent : MonoBehaviour
{
    public FatEnemy Owner;
    public Collider DamageCollider;

    private void Awake()
    {
        Owner = GetComponentInParent<FatEnemy>();
    }

    /// <summary>
    /// Avisa el momento de Animación en el que el Owner efectúa el Disparo.
    /// </summary>
    public void AE_ShootStart()
    {
        Owner.Shoot();
    }

    //============================== Combate ===============================================

    public void StartUp()
    {
        //Esto hay que ver que pasa xD.
    }

    public void Active()
    {
        DamageCollider.enabled = true;
        Owner.LookTowardsPlayer = false;
    }

    public void Recover()
    {
        DamageCollider.enabled = false;
    }
}
