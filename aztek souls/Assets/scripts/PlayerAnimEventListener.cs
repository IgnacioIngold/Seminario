using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimEventListener : MonoBehaviour
{
    public Collider DamageCollider;

    //Eventos de Animación.

    private void AnimEv_OnAttackStart()
    {
        print("AnimEvent PLAYER ON START ATTACK activado");
        DamageCollider.enabled = false;
    }

    private void AnimEv_OnAttackHasEnded()
    {
        print("AnimEvent PLAYER ON FINIT ATTACK activado");
        DamageCollider.enabled = true;
    }
}
