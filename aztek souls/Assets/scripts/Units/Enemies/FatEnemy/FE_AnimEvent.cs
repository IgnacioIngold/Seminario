using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CombatAnimationState
{
    StartUp,
    Active,
    Recover,
    inactive
}

public class FE_AnimEvent : MonoBehaviour
{
    public FatEnemy Owner;
    public Collider DamageCollider;
    CombatAnimationState currentState = CombatAnimationState.inactive;

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
        currentState = CombatAnimationState.StartUp;
    }

    public void Active()
    {
        currentState = CombatAnimationState.Active;
        DamageCollider.enabled = true;
        Owner.LookTowardsPlayer = false;
    }

    public void Recover()
    {
        currentState = CombatAnimationState.Recover;
        DamageCollider.enabled = false;
    }

    //=============================== RITMO ================================================

    public void StartVulnerableDisplay()
    {
        Owner.SetCurrentVulnerabilityCombo(0);
        print("Setting Vulnerability");
        Owner.ShowVulnerability();
    }
    public void EndVulnerableDisplay()
    {
        print("Hiding Vulnerability");
        Owner.HideVulnerability();
    }
}
