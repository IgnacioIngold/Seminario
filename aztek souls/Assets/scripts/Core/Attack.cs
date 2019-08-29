using System;
using System.Collections.Generic;
using Core.Entities;

public class Attack : IAttacker<object[]>
{
    public Action OnExecute = delegate { };
    public Action AttackEffects = delegate { };
    Dictionary<Inputs, Attack> ConnectedAttacks = new Dictionary<Inputs, Attack>();

    public string IDName = "";
    public float Cost = 0f;
    public float Damage = 0f;
    public float AttackDuration = 1f;

    public Attack()
    {
        InitBegginingAttacksDict();
    }
    public Attack(Action OnExecute)
    {
        this.OnExecute += OnExecute;

        InitBegginingAttacksDict();
    }
    public Attack(Action OnExecute, Action AttackEffects)
    {
        this.OnExecute += OnExecute;
        this.AttackEffects += AttackEffects;

        InitBegginingAttacksDict();
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

    public Attack getConnectedAttack(Inputs input)
    {
        return ConnectedAttacks[input];
    }

    public object[] GetDamageStats()
    {
        //Acá retorno todas las estadísticas del ataque.
        return new object[] { Damage };
    }

    private void InitBegginingAttacksDict()
    {
        ConnectedAttacks = new Dictionary<Inputs, Attack>();
        ConnectedAttacks.Add(Inputs.light, null);
        ConnectedAttacks.Add(Inputs.strong, null);
        ConnectedAttacks.Add(Inputs.none, null);
    }
}
