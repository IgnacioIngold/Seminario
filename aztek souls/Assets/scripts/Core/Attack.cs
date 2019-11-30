using System;
using System.Collections.Generic;

[Serializable]
public class Attack
{
    Dictionary<Inputs, Attack> ConnectedAttacks = new Dictionary<Inputs, Attack>();

    public event Action OnStart = delegate { };
    public event Action OnEnd = delegate { };
    public event Action OnHit = delegate { };
    public event Action OnEnableInput = delegate { };
    public event Action AttackEffects = delegate { };

    public int ID = 0;
    public string Name = "";
    //public int ChainIndex = 0;
    //public int maxChainIndex;
    public Inputs attackType;

    public float Cost = 0f;
    public float AttackDuration = 1f;

    public float Damage = 0f;

    public Attack()
    {
        ConnectedAttacks = new Dictionary<Inputs, Attack>();
        ConnectedAttacks.Add(Inputs.light, null);
        ConnectedAttacks.Add(Inputs.strong, null);
        ConnectedAttacks.Add(Inputs.none, null);
    }

    public void StartAttack()
    {
        OnStart();
    }
    public void EndAttack()
    {
        OnEnd();
    }
    public void EnableInput()
    {
        OnEnableInput();
    }
    public void Hit()
    {
        OnHit();
    }
    public void ActivateAttackEffects()
    {
        AttackEffects();
    }
    /// <summary>
    /// Devuelve el siguiente ataque encadenado.
    /// </summary>
    /// <param name="input">Tipo de input del ataque encadenado</param>
    /// <returns>Null si el ataque encadenado requerido no existe.</returns>
    public Attack getConnectedAttack(Inputs input)
    {
        return ConnectedAttacks[input];
    }
    /// <summary>
    /// Añade o reemplaza un ataque por otro identificado por el mismo tipo (parametro "type").
    /// </summary>
    /// <param name="type">Tipo de input que identifica a la conección.</param>
    /// <param name="ConnectedAttack">Ataque a conectar. </param>
    public Attack AddConnectedAttack(Inputs type, Attack ConnectedAttack)
    {
        if (!ConnectedAttacks.ContainsKey(type))
            ConnectedAttacks.Add(type, ConnectedAttack);
        else
            ConnectedAttacks[type] = ConnectedAttack;

        return this;
    }
}
