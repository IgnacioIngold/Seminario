using System;
using System.Collections.Generic;
using Core.Entities;
using UnityEngine;

[Serializable]
public class Attack : IAttacker<object[]>
{
    Dictionary<Inputs, Attack> ConnectedAttacks = new Dictionary<Inputs, Attack>();

    public Action OnStart = delegate { };
    public Action OnEnd = delegate { };
    public Action OnHit = delegate { };
    public Action AttackEffects = delegate { };

    public int ID = 0;
    public string Name = "";
    public int ChainIndex = 0;
    public int maxChainIndex;

    public float Cost = 0f;
    public float Damage = 0f;
    public float AttackDuration = 1f;

    public Attack()
    {
        ConnectedAttacks = new Dictionary<Inputs, Attack>();
        ConnectedAttacks.Add(Inputs.light, null);
        ConnectedAttacks.Add(Inputs.strong, null);
        ConnectedAttacks.Add(Inputs.none, null);
    }

    //============================================== INTERFACES =======================================================================================================

    public object[] GetDamageStats()
    {
        //Acá retorno todas las estadísticas del ataque.
        return new object[] { Damage };
    }

    //=================================================================================================================================================================

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
