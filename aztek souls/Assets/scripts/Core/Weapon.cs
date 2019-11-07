using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Entities;

public enum Inputs
{
    light,
    strong,
    none
}
[Serializable]
public class Weapon
{
    public Attack CurrentAttack = null;
    Animator _anims;

    public Dictionary<Inputs, Attack> entryPoints = new Dictionary<Inputs, Attack>();
    /// <summary>
    /// Determina las fuentes por las que la cadena completa puede ser interrumpida.
    /// </summary>
    public Func<bool> canContinueAttack = delegate { return false; };

    public event Action OnBegginChain = delegate { };
    public event Action OnEndChain = delegate { };

    public event Action DuringAttack = delegate { };

    public bool LastChainAttack = false;

    bool canGetInput = true;
    float currentDuration = 0f;

    public Weapon(Animator anims)
    {
        _anims = anims;

        _anims = anims;

        entryPoints = new Dictionary<Inputs, Attack>();
        entryPoints.Add(Inputs.light, null);
        entryPoints.Add(Inputs.strong, null);
        entryPoints.Add(Inputs.none, null);
    }
    public Weapon AddEntryPoint(Inputs type, Attack attack)
    {
        if (entryPoints == null)
            entryPoints = new Dictionary<Inputs, Attack>();

        if (entryPoints.ContainsKey(type))
            entryPoints[type] = attack;
        else
            entryPoints.Add(type, attack);

        return this;
    }

    public void BegginCombo(Inputs beggining)
    {
        if (beggining == Inputs.none) return;

        CurrentAttack = entryPoints[beggining];
        OnBegginChain();
        StartAttack();
    }

    void StartAttack()
    {
        if (CurrentAttack.ChainIndex == CurrentAttack.maxChainIndex)
            LastChainAttack = true;

        canGetInput = false;
        currentDuration = CurrentAttack.AttackDuration;
        CurrentAttack.StartAttack();
    }
    public void Update()
    {
        DuringAttack();

        currentDuration -= Time.deltaTime;

        if (currentDuration <= 0)
        {
            if (CurrentAttack != null) CurrentAttack.EndAttack();
            EndChainCombo();
        }
    }

    void EndChainCombo()
    {
        OnEndChain();
        //CurrentAttack = null;
    }
    public void CanGetInput(bool enabled)
    {
        if (enabled && CurrentAttack != null)
        {
            canGetInput = true;
            CurrentAttack.EnableInput();
        }
        else
            canGetInput = false;
    }
    public void ConfirmHit()
    {
        CurrentAttack.Hit();
    }

    public void InterruptAttack()
    {
        //MonoBehaviour.print("Ataque Interrumpido.");
        OnEndChain();
    }
    public void FeedInput(Inputs input)
    {
        if (canContinueAttack() && canGetInput)
        {
            Attack posible = CurrentAttack.getConnectedAttack(input);

            if (posible != null)
            {
                //MonoBehaviour.print("Input CONFIRMADO.");

                CurrentAttack.EndAttack();
                CurrentAttack = posible;
                currentDuration = CurrentAttack.AttackDuration;
                StartAttack();
            }
        }
    }
}
