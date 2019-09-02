using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimEventListener : MonoBehaviour
{
    public Collider DamageCollider;

    //Eventos de Animación.

    private void AnimEv_OnAttackStart()
    {
        //print("AnimEvent ENEMY ON START ATTACK activado");
        DamageCollider.enabled = true;
    }

    private void AnimEv_OnAttackHasEnded()
    {
        //print("AnimEvent ENEMY ON FINIT ATTACK activado");
        DamageCollider.enabled = false;
    }
}
