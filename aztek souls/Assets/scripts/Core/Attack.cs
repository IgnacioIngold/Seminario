using System;
using System.Collections.Generic;
using Core.Entities;

public class Attack : IAttacker<object[]>
{
    public Action OnStart = delegate { };
    public Action AttackEffects = delegate { };
    public Action OnHit = delegate { };
    Dictionary<Inputs, Attack> ConnectedAttacks = new Dictionary<Inputs, Attack>();

    public string IDName = "";
    public string AnimClipName = "";
    public int ChainIndex = 0;
    public int maxChainIndex;

    public float Cost = 0f;
    public float Damage = 0f;
    public float AttackDuration = 1f;

    //============================================== INTERFACES =======================================================================================================

    public object[] GetDamageStats()
    {
        //Acá retorno todas las estadísticas del ataque.
        return new object[] { Damage };
    }

    //=================================================================================================================================================================

    public Attack()
    {
        ConnectedAttacks = new Dictionary<Inputs, Attack>();
        ConnectedAttacks.Add(Inputs.light, null);
        ConnectedAttacks.Add(Inputs.strong, null);
        ConnectedAttacks.Add(Inputs.none, null);
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

    ///// <summary>
    ///// Retorna el tiempo que falta para que la animación correspondiente termine, si la animación no coincide con el nombre específicado retorna la duración del ataque.
    ///// </summary>
    ///// <param name="anims">El componente Animator que contiene las animaciones.</param>
    ///// <param name="transitionPassed">[Opcional] Posible tiempo de transición hacia este estado que ya haya pasado.</param>
    //public float getRemainingAnimTime(Animator anims, float transitionPassed = 0f)
    //{
    //    AnimatorClipInfo[] clipInfo = anims.GetCurrentAnimatorClipInfo(0);
    //    float AnimTime = AttackDuration;

    //    if (clipInfo != null && clipInfo.Length > 0)
    //    {
    //        AnimationClip currentClip = clipInfo[0].clip;
    //        MonoBehaviour.print("Attack Clip Searched: " + AnimClipName + " ClipGetted: " + currentClip.name);

    //        if (currentClip.name == AnimClipName)
    //        {
    //            //print("currentClip is Correct!");
    //            AnimTime = currentClip.length;
    //            float passed = AnimTime - (AnimTime * transitionPassed);
    //            return passed;
    //        }
    //    }

    //    return AnimTime;
    //}
}
