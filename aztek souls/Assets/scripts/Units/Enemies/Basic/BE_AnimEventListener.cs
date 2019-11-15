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
    public void StartUp()
    {
        owner.LookTowardsPlayer = true;
        owner.SetCurrentVulnerabilityCombo(0);
        owner.vulnerabilityWindow.Start();
        owner.ShowVulnerability();
    }

    public void Active()
    {
        AttackCollider.enabled = true;
        owner.LookTowardsPlayer = false;
    }

    public void Recovery()
    {
        //owner.LookTowardsPlayer = true;
        AttackCollider.enabled = false;
    }

    public void AttackFinished()
    {
        owner.AP_SimpleAttack = false;
        owner.FeedFSM(BasicEnemyStates.think);
    }

    //============================= Hurt Animation ==========================================

    public void HurtAnimationEnded()
    {
        print("Final de la animación.");
        owner.AP_SimpleAttack = false;
        owner.AP_GetHit = false;
        owner.FeedFSM(BasicEnemyStates.think);
    }

}
