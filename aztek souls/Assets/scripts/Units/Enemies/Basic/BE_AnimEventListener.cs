using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BE_AnimEventListener : MonoBehaviour
{
    public BasicEnemy owner;
    public Collider AttackCollider;
    

    private void Awake()
    {
        owner = GetComponentInParent<BasicEnemy>();
    }

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
        owner.SetVulnerabity(true);
    }
}
