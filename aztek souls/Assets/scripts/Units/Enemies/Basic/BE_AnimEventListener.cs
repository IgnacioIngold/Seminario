using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BE_AnimEventListener : MonoBehaviour
{
    public Collider AttackCollider;
    public ParticleSystem ableHitmarket;

    public void AE_AttackEnable()
    {
        //print("AnimEvent basicEnemy ON START ATTACK activado");
        AttackCollider.enabled = true;
    }
    public void AE_AttackDisable()
    {
        //print("AnimEvent basicEnemy ON FINIT ATTACK activado");
        AttackCollider.enabled = false;
    }
    public void AE_EnableMarker()
    {
        ableHitmarket.Play();
    }
}
