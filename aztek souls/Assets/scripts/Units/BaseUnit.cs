﻿using UnityEngine;
using UnityEngine.AI;
using IA.LineOfSight;
using System;
using System.Collections;
using Core.Entities;
using Core;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent)), RequireComponent(typeof(Collider)), RequireComponent(typeof(Rigidbody))]
public abstract class BaseUnit : MonoBehaviour, IDamageable<HitData, HitResult>, IKilleable
{
    #region Eventos

    public Action OnDie = delegate { };

    #endregion

    #region Settings Obligatorios

    [Header("Obligatory Settings")]
    protected Transform Target;
    public GameObject OnHitParticle;
    public FillBar EnemyHealthBar;
    [SerializeField] protected LineOfSight sight = null;

    #endregion

    #region Sistema de Ritmo

    //Sistema de ritmo. --> Este es el combo al cual es "vulnerable"
    [Header("Sistema de Ritmo")]
    public float ComboBonus;
    public ParticleSystem VulnerableMarker;       // Indica la tecla que debemos presionar.
    public ParticleSystem ButtonHitConfirm;       // Confirma que la tecla fue presionada.
    public Color LightColor;                      // Indica que se debe presionar el input Light.
    public Color HeavyColor;                      // Indica que se debe presionar el input Strong.
    public Dictionary<int, Inputs[]> vulnerabilityCombos;
    public float comboVulnerabilityCountDown = 0f;
    public bool isVulnerableToAttacks = false;
    protected int _currentVulnerabilityCombo = 0;
    protected int _attacksRecieved = 0;

    #endregion

    #region Vulnerabilidad

    [Header("Vulnerability")]
    public float vulnerableTime = 1.5f;
    public float incommingDamageReduction = 0.3f;
    public float ComboWindow = 1f;

    #endregion

    #region Recompensas

    [Header("Recompensas")]
    public int BloodPerHit = 100;
    public int BloodForKill = 300;

    #endregion

    #region Variables Hereditarias

    protected float _hp = 0f;
    protected bool _targetDetected = false;
    protected bool _invulnerable = false;
    protected bool _alertFriends = true;
    protected Vector3 _viewDirection = Vector3.zero;

    #endregion

    #region Estadísticas Base

    [Header("Estadísticas BASE")]
    public float MaxHP = 100f;                       //Vida Máxima de esta unidad.
    public float Health
    {
        get { return _hp; }
        set
        {
            _hp = value;
            if (_hp < 0) _hp = 0;
            EnemyHealthBar.UpdateDisplay(_hp);
        }
    }
    public float MovementSpeed = 1f;                 // Velocidad de desplazamiento de esta Unidad.
    public float attackRate = 1f;                    // El ratio de ataque.
    public float AttackRange = 1f;                   // El rango de Ataque.
    public float attackDamage = 1f;                  // El daño básico.
    public float NormalRotationLerpSeed = 0.1f;      // La velocidad de rotación Normal.
    public float AttackRotationLerpSpeed = 10f;      // La velocidad de rotación durante el combate.
    protected float _rotationLerpSpeed = 0.1f;       // La velocidad de rotación actual.

    [Header("Rangos de Detección")]
    public float minDetectionRange = 8f;             // Rango de detección Mínima.
    public float MediumRange = 40f;                  // Rango de detección Medio.
    public float HighRange = 60f;                    // Rango de detección Máximo.

    public bool LookTowardsPlayer = true;            // Si la unidad debe rotar hacia el jugador.

    #endregion

    #region Componentes de Unity

    protected Animator anims;
    protected NavMeshAgent agent;
    protected Collider MainColl;
    protected Rigidbody rb;

    #endregion

#if UNITY_EDITOR
    #region Variables Debugg
    [Header("Debug")]
    public bool Debug_Gizmos = false;
    public bool Debug_LineOFSight = false;
    public bool Debug_Attacks = false;
    public bool Debug_DetectionRanges = false;
    #endregion
#endif

    //============================== INTERFACES ===============================================

    public bool IsAlive => _hp > 0;
    public bool invulnerable => _invulnerable;

    public virtual HitResult Hit(HitData EntryData) { return HitResult.Default(); }
    public virtual HitData DamageStats() { return HitData.Default(); }
    public virtual void GetHitResult(HitResult result) { }

    //============================= DEBUGG GIZMOS =============================================

    #region Snippet for Debugg

#if (UNITY_EDITOR) 
    protected virtual void OnDrawGizmosSelected()
    {
        if (Debug_Gizmos)
        {
            var currentPosition = transform.position;

            if (Debug_LineOFSight)
            {
                if (sight.target != null)
                {
                    Gizmos.color = sight.IsInSight() ? Color.green : Color.red;   //Target In Sight es un bool en una clase Externa.
                    float distanceToTarget = sight.positionDiference.magnitude;   //mySight es una instancia de la clase LineOfSight.
                    if (distanceToTarget > sight.range) distanceToTarget = sight.range;
                    sight.dirToTarget.Normalize();
                    Gizmos.DrawLine(currentPosition, currentPosition + sight.dirToTarget * distanceToTarget); 
                }

                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(currentPosition, currentPosition + Quaternion.Euler(0, sight.angle + 1, 0) * transform.forward * sight.range);
                Gizmos.DrawLine(currentPosition, currentPosition + Quaternion.Euler(0, -sight.angle - 1, 0) * transform.forward * sight.range);

                //Gizmos.color = Color.gray;
                //Gizmos.DrawLine(currentPosition, currentPosition + Quaternion.Euler(0, minForwardAngle + 1, 0) * transform.forward * sight.range);
                //Gizmos.DrawLine(currentPosition, currentPosition + Quaternion.Euler(0, -minForwardAngle - 1, 0) * transform.forward * sight.range);

                Gizmos.color = Color.white;
                Gizmos.matrix *= Matrix4x4.Scale(new Vector3(1, 0, 1));
                Gizmos.DrawWireSphere(currentPosition, sight.range);
            }

            if (Debug_Attacks)
            {
                Gizmos.color = Color.red;
                Gizmos.matrix *= Matrix4x4.Scale(new Vector3(1, 0, 1));
                Gizmos.DrawWireSphere(currentPosition, AttackRange);
            }

            if (Debug_DetectionRanges)
            {
                Gizmos.color = Color.yellow;
                Gizmos.matrix *= Matrix4x4.Scale(new Vector3(1, 0, 1));
                Gizmos.DrawWireSphere(currentPosition, minDetectionRange);

                Gizmos.color = Color.magenta;
                Gizmos.matrix *= Matrix4x4.Scale(new Vector3(1, 0, 1));
                Gizmos.DrawWireSphere(currentPosition, MediumRange);

                Gizmos.color = Color.magenta;
                Gizmos.matrix *= Matrix4x4.Scale(new Vector3(1, 0, 1));
                Gizmos.DrawWireSphere(currentPosition, HighRange);
            }
        }
    } 
#endif

#endregion

    //=========================================================================================

    protected virtual void Awake()
    {
        //Seteos iniciales.
        anims = GetComponentInChildren<Animator>();
        agent = GetComponent<NavMeshAgent>();
        MainColl = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();

        EnemyHealthBar.SetApha(0f);

        Target = GameObject.FindGameObjectWithTag("Player").transform;
        if (Target != null)
        {
            //print("Encontré al jugador");
            sight.target = Target;
        }

        agent.speed = MovementSpeed;

        EnemyHealthBar.MaxValue = MaxHP;
        Health = MaxHP;
    }

    //=========================================================================================

    /// <summary>
    /// Retorna la duración de la Transición al siguiente estado de la Animación.
    /// </summary>
    /// <returns>Float: el tiempo de la transición.</returns>
    protected float getCurrentTransitionDuration()
    {
        return anims.GetAnimatorTransitionInfo(0).duration;
    }
    /// <summary>
    /// Retorna el tiempo de Animación restante del estado Actual del Animator.
    /// </summary>
    /// <returns>Float: tiempo de Animación restante del estado actual.</returns>
    protected float getRemainingAnimTime()
    {
        //AnimatorClipInfo[] clipInfo = anims.GetCurrentAnimatorClipInfo(0);
        //float AnimTime = 0f;

        var CurrentState = anims.GetCurrentAnimatorStateInfo(0);

        //if (clipInfo != null && clipInfo.Length > 0)
        //{
        //    AnimationClip currentClip = clipInfo[0].clip;
        //    print("Clip Searched: " + ClipName + " ClipGetted: " + currentClip.name);

        //    if (currentClip.name == ClipName)
        //    {
        //        //print("currentClip is Correct!");
        //        AnimTime = currentClip.length;
        //        float passed = AnimTime - (AnimTime * transitionPassed);
        //        return passed;
        //    }
        //}

        return CurrentState.length - (CurrentState.length * CurrentState.normalizedTime);
    }

    protected void Die()
    {
        EnemyHealthBar.FadeOut(3f);
        agent.enabled = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        rb.isKinematic = true;
        MainColl.enabled = false;

        StartCoroutine(FallAfterDie(3f));
        OnDie();
    }

    public void AllyDiscoversEnemy(Transform Enemy)
    {
        Target = Enemy;
        _targetDetected = true;
        _alertFriends = false;
        EnemyHealthBar.FadeIn();
    }

    IEnumerator FallAfterDie(float delay = 1f)
    {
        float fallTime = 10f;
        yield return new WaitForSeconds(delay + 2f);

        while(fallTime > 0)
        {
            fallTime -= Time.deltaTime;
            transform.position += Vector3.down * 0.5f * Time.deltaTime;
            yield return null;
        }

        gameObject.SetActive(false);
    }
}
