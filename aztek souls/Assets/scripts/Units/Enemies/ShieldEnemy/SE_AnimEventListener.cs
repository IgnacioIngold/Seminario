using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SE_AnimEventListener : MonoBehaviour
{
    public Collider coll;
    ShieldEnemy Owner;

    int lastIndex = 0;

    private void Awake()
    {
        //coll = GetComponent<Collider>();
        Owner = GetComponentInParent<ShieldEnemy>();
        Owner.OnDie += () => { coll.enabled = false; };
        Owner.onGetHit += () => { coll.enabled = false; };
    }

    //================================= Move Stages ==============================================

    /// <summary>
    /// Avisa al Owner que esta iniciando un Ataque.
    /// </summary>
    /// <param name="index">Valores de Index.</param>
    public void Stage_StartUp(int index)
    {
        //Por ahora no requiero hacer nada en el StartUp.
        lastIndex = index;
        Owner.SetAttackState(index, AttackStage.StartUp);
    }
    /// <summary>
    /// Habilita el Hitbox de la unidad.
    /// </summary>
    public void Stage_Active()
    {
        coll.enabled = true;
        Owner.SetAttackState(lastIndex, AttackStage.Active);
    }
    /// <summary>
    /// Deshabilita el Hitbox de la unidad.
    /// </summary>
    public void Stage_EndActive()
    {
        coll.enabled = false;
    }
    /// <summary>
    /// Avísa al Owner que entró en la fase de recovery.
    /// </summary>
    /// <param name="index"></param>
    public void Stage_Recovery(int index)
    {
        Owner.SetAttackState(index, AttackStage.Recovery);
    }

    //================================= Parry ====================================================

    public void ParryStartAttack()
    {
        Owner.StartParryAttack();
    }

    //================================= Ritmo ====================================================

    // Marcamos cuando esta vulnerable.
    public void MarkVulnerable()
    {
        //marker.Play();
    }

    //================================= Attaques =================================================

    /// <summary>
    /// Avisa al Owner que esta listo para para pasar al siguiente Ataque.
    /// </summary>
    /// <param name="index"></param>
    public void ChangeAttack(int index)
    {
        Owner.ChangeAttack(index);
    }
    /// <summary>
    /// Avisa al Owner que esta terminando el Ataque.
    /// </summary>
    /// <param name="index"></param>
    public void EndAtk(int index)
    {
        Owner.AttackEnded(index);
    }
}
