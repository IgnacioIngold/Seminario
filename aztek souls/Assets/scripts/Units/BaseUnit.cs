using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using IA.LineOfSight;
using Core.Entities;
using Core;

[RequireComponent(typeof(NavMeshAgent)), RequireComponent(typeof(Collider)), RequireComponent(typeof(Rigidbody))]
public abstract class BaseUnit : MonoBehaviour, IDamageable<HitData, HitResult>, IKilleable
{
    public Action OnDie = delegate { };

    [Header("Obligatory Settings")]
    protected Transform Target;
    public GameObject OnHitParticle;
    public FillBar    EnemyHealthBar;
    [SerializeField] protected LineOfSight sight = null;

    //Sistema de ritmo. --> Este es el combo al cual es "vulnerable"
    [Header("Sistema de Ritmo")]
    public float ComboBonus;
    public ParticleSystem VulnerableMarker;
    public ParticleSystem ButtonHitConfirm;
    public Color LightColor;
    public Color HeavyColor;
    public Dictionary<int, Inputs[]> vulnerabilityCombos;
    public float comboVulnerabilityCountDown = 0f;
    public bool isVulnerableToAttacks = false;
    protected int _attacksRecieved = 0;

    [Header("Vulnerability")]
    public float vulnerableTime = 1.5f;
    public float incommingDamageReduction = 0.3f;
    public float ComboWindow = 1f;

    [Header("Recompensas")]
    public int BloodPerHit = 100;
    public int BloodForKill = 300;

    protected float _hp = 0f;
    protected float _minForwardAngle = 40f;
    protected bool _targetDetected = false;
    protected bool _invulnerable = false;
    protected bool _isMoving = false;
    protected bool _alertFriends = true;
    protected Vector3 _viewDirection = Vector3.zero;

    [Header("Estadísticas BASE")]
    public float MaxHP = 100f;
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
    public float MovementSpeed = 1f;
    public float attackRate = 1f;
    public float AttackRange = 1f;
    public float attackDamage = 1f;
    protected float _rotationLerpSpeed = 0.1f;
    public float NormalRotationLerpSeed = 0.1f;
    public float AttackRotationLerpSpeed = 10f;

    public float minDetectionRange = 8f;
    public float MediumRange       = 40f;
    public float HighRange         = 60f;

    public bool LookTowardsPlayer = true;

    #region Componentes de Unity

    protected Animator anims;
    protected NavMeshAgent agent;
    protected Collider MainColl;
    protected Rigidbody rb;

    #endregion

    #region DebuggVariables
#if UNITY_EDITOR
    [Header("Debug")]
    public bool Debug_Gizmos          = false;
    public bool Debug_LineOFSight     = false;
    public bool Debug_Attacks         = false;
    public bool Debug_DetectionRanges = false;
#endif
    #endregion

    //============================== INTERFACES ===============================================

    public bool IsAlive => _hp > 0;
    public bool invulnerable => _invulnerable;

    public virtual HitData GetDamageStats() { return HitData.Empty(); }
    public virtual HitResult Hit(HitData EntryData) { return HitResult.Empty(); }
    public virtual void FeedHitResult(HitResult result) { }

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

    /// <summary>
    /// Permite setear por evento el momento en el que el enemigo es vulnerable a un ataque enemigo.
    /// </summary>
    /// <param name="vulnerable">Si el enemigo entro en su fase vulnerable.</param>
    public virtual void SetVulnerabity(bool vulnerable)
    {
        comboVulnerabilityCountDown = vulnerableTime;

        ConfirmButtonHit();

        //Muestro el siguiente ataque.
        ShowNextVulnerability(0);
    }

    protected void ShowNextVulnerability(int index)
    {
        if (index < vulnerabilityCombos[1].Length)
        {
            var nextAttackType = vulnerabilityCombos[1][index];
            var ParticleSystem = VulnerableMarker.main;

            if (nextAttackType == Inputs.light)
                ParticleSystem.startColor = LightColor;
            if (nextAttackType == Inputs.strong)
                ParticleSystem.startColor = HeavyColor;
        }
    }
    protected void ConfirmButtonHit()
    {
        if (!ButtonHitConfirm.gameObject.activeSelf)
            ButtonHitConfirm.gameObject.SetActive(true);
        else
            ButtonHitConfirm.Play();
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
