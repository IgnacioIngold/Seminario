using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BE_AnimEventListener : MonoBehaviour
{
    public BasicEnemy owner;
    public Collider AttackCollider;
    audioManager _AM;
    

    private void Awake()
    {
        owner = GetComponentInParent<BasicEnemy>();
        _AM = GetComponent<audioManager>();
    }

    //============================= Attack ==================================================

    void PlaySound(string Source)
    {
        _AM.Play(Source);
    }
    public void AttackStartUp()
    {
        owner.LookTowardsPlayer = true;
    }

    public void Attack_ActivePhase()
    {
        //print("AnimEvent basicEnemy ON START ATTACK activado");
        if (owner.isAttacking)
        {
            AttackCollider.enabled = true;
            owner.LookTowardsPlayer = false;
        }
    }
    public void Attack_ActivePhaseEnd()
    {
        //print("AnimEvent basicEnemy ON FINIT ATTACK activado");
        if (owner.isAttacking)
            AttackCollider.enabled = false;
    }

    public void AttackRecovery()
    {
        //owner.LookTowardsPlayer = true;
    }

    public void AttackFinished()
    {
        if (owner.isAttacking)
            owner.FeedFSM(BasicEnemyStates.think);
    }
}
