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
    public Attack Current = null;

    public Dictionary<Inputs, Attack> entryPoints = new Dictionary<Inputs, Attack>();
    event Action OnBeginAttack = delegate { };
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
        if (Current == null)
        {
            if (Input.GetButtonDown("LighAttack"))
                Current = entryPoints[Inputs.light];

            if (Input.GetButtonDown("StrongAttack"))
                Current = entryPoints[Inputs.strong];

            OnBeginAttack();
        }

        currentDuration = Current.AttackDuration;
        Current.OnExecute();
    }

    public void Update()
    {
        currentDuration -= Time.deltaTime;

        if (nextAttack == Inputs.none)
        {
            if (Input.GetButtonDown("LighAttack"))
            {
                MonoBehaviour.print("Input Light CONFIRMADO");
                nextAttack = Inputs.light;
                return;
            }

            if (Input.GetButtonDown("StrongAttack"))
                nextAttack = Inputs.light;
        }

        if (currentDuration < 0)
        {
            var nextCurrent = Current.getConnectedAttack(nextAttack);

            nextAttack = Inputs.none;
            if (nextCurrent != null)
            {
                Current = nextCurrent;
                StartAttack();
            }
            else
            {
                Current = null;
                MonoBehaviour.print("FIN DE CADENA");
                OnExitAttack();
            }
        }
    }

    public void AddEntryPoint(Inputs type, Attack attack)
    {
        if (entryPoints == null)
            entryPoints = new Dictionary<Inputs, Attack>();

        if (entryPoints.ContainsKey(type))
            entryPoints[type] = attack;
        else
            entryPoints.Add(type, attack);
    }
}
