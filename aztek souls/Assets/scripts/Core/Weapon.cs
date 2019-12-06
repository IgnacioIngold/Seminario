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
    public event Action<Inputs> OnInputConfirmed = delegate { };
    public event Action DuringAttack = delegate { };

    bool _inputLock = true;                      //El arma actual puede recibir input.
    bool _inputConfirm = false;                  // El arma actual recibió un input correcto.

    public Weapon(Animator anims)
    {
        _anims = anims;

        entryPoints = new Dictionary<Inputs, Attack>
        {
            { Inputs.light, null },
            { Inputs.strong, null },
            { Inputs.none, null }
        };
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
        //Debug.LogWarning("============ INICIO DEL COMBO ============");
        if (beggining == Inputs.none) return;

        CurrentAttack = entryPoints[beggining];
        OnBegginChain();
        StartAttack();
    }

    void StartAttack()
    {
        _inputLock = false;
        _inputConfirm = false;

        CurrentAttack.StartAttack();
    }
    public void EndCurrentAttack()
    {
        if (_inputConfirm)
            StartAttack();
        else
            EndChainCombo();
    }
    void EndChainCombo()
    {
        OnEndChain();
        CurrentAttack = null;
        //Debug.LogWarning("============ FINAL DEL COMBO ============");
    }
    public void CanGetInput()
    {
        _inputLock = true;
        if (_inputLock) CurrentAttack.EnableInput();
    }
    public void ConfirmHit()
    {
        CurrentAttack.Hit();
    }

    //public void InterruptAttack()
    //{
    //    //MonoBehaviour.print("Ataque Interrumpido.");
    //    OnEndChain();
    //}
    public void FeedInput(Inputs input)
    {
        if (CurrentAttack.isChainFinale || !_inputLock) return;

        if (_inputLock)
        {
            Attack posible = CurrentAttack.GetConnectedAttack(input);

            if (posible != null)
            {
                _anims.SetInteger("combat", posible.ID);
                CurrentAttack = posible;
                _inputConfirm = true;
                _inputLock = false;
            }
        }
    }
}
