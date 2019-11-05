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
    public Attack NextAttack = null;
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
        Debug.LogWarning("============ INICIO DEL COMBO ============");
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
        NextAttack = null;
        currentDuration = CurrentAttack.AttackDuration;
        CurrentAttack.StartAttack();
    }
    public void Update()
    {
        if (CurrentAttack != null)
        {
            DuringAttack();

            currentDuration -= Time.deltaTime;
            //MonoBehaviour.print("Current Attack: " + CurrentAttack.Name +" currentTime is:" + currentDuration);

            if (currentDuration <= 0)
            {
                MonoBehaviour.print(string.Format("Duración del ataque terminado\nEl ultimo ataque fue {0}", CurrentAttack.Name));

                if (NextAttack == null)
                {
                    CurrentAttack.EndAttack();
                    EndChainCombo();
                }
                else
                {
                    //Cambio el ataque al nuevo ataque.
                    CurrentAttack = NextAttack;
                    currentDuration = CurrentAttack.AttackDuration;
                    StartAttack();
                }
            } 
        }
    }

    void EndChainCombo()
    {
        OnEndChain();
        CurrentAttack = null;
        NextAttack = null;
        Debug.LogWarning("============ FINAL DEL COMBO ============");
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
        if (CurrentAttack != null && canContinueAttack() && canGetInput && NextAttack == null)
        {
            Attack posible = CurrentAttack.getConnectedAttack(input);
            MonoBehaviour.print(string.Format("Recibido comando Input\nEl tipo pedido es {0} y el resultado es {1}.", input.ToString(), posible != null ? posible.Name : "Nulo"));

            if (posible != null)
            {
                MonoBehaviour.print("Input CONFIRMADO.");
                NextAttack = posible;
            }
        }
    }
}
