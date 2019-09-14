using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public event Action OnStartAttack = delegate { };
    public event Action DuringAttack = delegate { };
    public event Action OnInputConfirmed = delegate { };
    public event Action OnEndAttack = delegate { };

    Attack nextCurrent;
    float currentDuration = 0f;
    bool _onLastChainAttack = false;
    Inputs nextAttack = Inputs.none;

    public Weapon(Animator anims)
    {
        _anims = anims;

        entryPoints = new Dictionary<Inputs, Attack>();
        entryPoints.Add(Inputs.light, null);
        entryPoints.Add(Inputs.strong, null);
        entryPoints.Add(Inputs.none, null);
    }

    public void BegginCombo(Inputs beggining)
    {
        if (beggining == Inputs.none)
        {
            OnEndChain();
            return;
        }

        OnBegginChain();
        CurrentAttack = entryPoints[beggining];
        StartAttack();
    }
    void StartAttack()
    {
        //Si la estamina es menor a 0, el ataque se corta. Esto va a cambiar a ser un modificador.
        if (!canContinueAttack())
        {
            OnEndChain();
            return;
        }

        if (CurrentAttack.ChainIndex == CurrentAttack.maxChainIndex)
            _onLastChainAttack = true;

        currentDuration = CurrentAttack.AttackDuration;
        CurrentAttack.OnStart();
    }
    public void Update()
    {
        currentDuration -= Time.deltaTime;

        DuringAttack();

        if (!_onLastChainAttack && nextAttack == Inputs.none)
        {
            Attack posible;
            if (Input.GetButtonDown("LighAttack"))
            {
                MonoBehaviour.print("Input LIGHT Pedido.");
                posible = CurrentAttack.getConnectedAttack(Inputs.light);

                if (nextCurrent != null)
                {
                    MonoBehaviour.print("Input LIGHT CONFIRMADO.");
                    nextCurrent = posible;
                    OnInputConfirmed();
                }
                return;
            }

            if (Input.GetButtonDown("StrongAttack"))
            {
                MonoBehaviour.print("Input STRONG Pedido.");
                posible = CurrentAttack.getConnectedAttack(Inputs.strong);

                if (nextCurrent != null)
                {
                    MonoBehaviour.print("Input STRONG CONFIRMADO.");
                    nextCurrent = posible;
                    OnInputConfirmed();
                }
            }
        }
        else
            OnInputConfirmed();

        if (currentDuration <= 0)
        {
            if (_onLastChainAttack)
            {
                CurrentAttack = null;
                OnEndChain();
                return;
            }

            if (nextCurrent != null)
            {
                CurrentAttack = nextCurrent;
                nextCurrent = null;
                nextAttack = Inputs.none;
                StartAttack();
            }
        }
    }

    public void InterruptAttack()
    {
        //MonoBehaviour.print("Ataque Interrumpido.");
        OnEndChain();
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
    
    float getCurrentTransitionScaledTime()
    {
        return _anims.GetAnimatorTransitionInfo(0).duration;
    }
}
