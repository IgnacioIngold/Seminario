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

    public Dictionary<Inputs, Attack> entryPoints = new Dictionary<Inputs, Attack>();
    public Func<bool> canContinueAttack = delegate { return false; };

    event Action OnBeginAttack = delegate { };
    public event Action DuringAttack = delegate { };
    event Action OnExitAttack = delegate { };
    float currentDuration = 0f;
    Inputs nextAttack = Inputs.none;

    public Weapon( Action OnBeginAttack, Action OnExitAttack)
    {
        this.OnBeginAttack += OnBeginAttack;
        this.OnExitAttack += OnExitAttack;

        entryPoints = new Dictionary<Inputs, Attack>();
        entryPoints.Add(Inputs.light, null);
        entryPoints.Add(Inputs.strong, null);
        entryPoints.Add(Inputs.none, null);
    }

    public void StartAttack()
    {
        if (CurrentAttack == null)
        {
            if (Input.GetButtonDown("LighAttack"))
                CurrentAttack = entryPoints[Inputs.light];

            if (Input.GetButtonDown("StrongAttack"))
                CurrentAttack = entryPoints[Inputs.strong];

            OnBeginAttack();
        }

        if (!canContinueAttack())
        {
            OnExitAttack();
            return;
        }

        currentDuration = CurrentAttack.AttackDuration;
        CurrentAttack.OnExecute();
    }

    public void Update()
    {
        currentDuration -= Time.deltaTime;

        DuringAttack();

        if (nextAttack == Inputs.none)
        {
            if (Input.GetButtonDown("LighAttack"))
            {
                MonoBehaviour.print("Input LIGHT CONFIRMADO");
                nextAttack = Inputs.light;
                return;
            }

            if (Input.GetButtonDown("StrongAttack"))
            {
                MonoBehaviour.print("Input STRONG CONFIRMADO");
                nextAttack = Inputs.strong;
            }
        }

        if (currentDuration < 0)
        {
            var nextCurrent = CurrentAttack.getConnectedAttack(nextAttack);

            nextAttack = Inputs.none;
            if (nextCurrent != null)
            {
                CurrentAttack = nextCurrent;
                StartAttack();
            }
            else
            {
                CurrentAttack = null;
                MonoBehaviour.print("FIN DE CADENA");
                OnExitAttack();
            }
        }
    }

    public void InterruptAttack()
    {
        MonoBehaviour.print("FIN DE CADENA");
        OnExitAttack();
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
}
