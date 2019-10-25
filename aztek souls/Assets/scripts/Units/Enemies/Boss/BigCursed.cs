using System.Collections;
using UnityEngine;
using IA.StateMachine.Generic;
using IA.RandomSelections;
using Core;
using Core.Entities;

public enum BossStates
    {
        idle,
        think,
        charge,
        reposition,
        pursue,
        basicCombo,
        killerJump,
        closeJump,
        dead
    }

public class BigCursed : BaseUnit
{
    //Estados
#if UNITY_EDITOR
    public BossStates CurrentState;
#endif
    GenericFSM<BossStates> sm;
    State<BossStates> idle;

    [Header("Settings del Boss")]
    public float[] BasicAttackDamages;
    public int AttackPhase = 0;
    Vector3 _lastEnemyPositionKnown = Vector3.zero;

    [Header("Layers")]
    public int Layer_walls;
    public int Layer_Killeable;

    [Header("Deciciones")]
    public float MinimunRange;
    public float MinimunRangeExtraAdd;
    public float JumpWeight;
    public float AttackWeight;
    public float KillerJumpWeight;
    public float PursueWeight;

    [Header("Charge")]
    public ParticleSystem OnChargeParticle;
    public ParticleSystem OnSmashParticle;
    public ParticleSystem.EmissionModule ChargeEmission;
    public ParticleSystem.EmissionModule SmashEmission;
    public float ChargeDamage = 50f;
    public float chargeSpeed = 30f;
    public float ChargeCollisionForce = 20f;
    public float maxChargeDistance = 100f;

    [Header("Jump")]
    public float maxJumpDistance = 10f;
    public bool canPerformSimpleJump = true;
    public bool canPerformKIllerJump = true;
    public float SimpleJumpAttackCooldown = 5f;
    public float killerJumpAttackCooldown = 10f;

    //Private:

    float thinkTime = 0f;
    bool charging = false;
    Vector3 _initialChargePosition;
    Vector3 _chargeDir = Vector3.zero;

    //============================== INTERFACES ===============================================

    /// <summary>
    /// Permite recibir daño e Informar al agresor del resultado de su ataque.
    /// </summary>
    /// <param name="DamageStats">Las estadísticas que afectan el "Daño" recibído.</param>
    public override HitResult Hit(HitData HitInfo)
    {
        HitResult result = HitResult.Empty();
        Health -= HitInfo.Damage;

        if (IsAlive)
        {
            result.bloodEarned = BloodPerHit;
            result.HitConnected = true;

            var particle = Instantiate(OnHitParticle, transform.position, Quaternion.identity);
            Destroy(particle, 3f);
            EnemyHealthBar.FadeIn();
        }
        else
        {
            result.TargetEliminated = true;
            result.bloodEarned = BloodForKill;
            sm.Feed(BossStates.dead);
        }
        return result;
    }
    /// <summary>
    /// Retorna las estadísticas de combate de esta Unidad.
    /// </summary>
    /// <returns>Una estructura que contiene las stats involucradas en el ataque.</returns>
    public override HitData GetDamageStats()
    {
        return new HitData() { Damage = attackDamage, BreakDefence = false };
    }
    /// <summary>
    /// Informa a esta entidad del resultado del lanzamiento y conección de un ataque.
    /// </summary>
    /// <param name="result">Resultado del ataque lanzado.</param>
    public override void FeedHitResult(HitResult result)
    {
        print(string.Format("{0} ha conectado un ataque.", gameObject.name));
    }

    //============================= Funciones Unity ===========================================

    protected override void Awake()
    {
        base.Awake();
        OnDie += () => {  };

        ChargeEmission = OnChargeParticle.emission;
        ChargeEmission.enabled = false;
        SmashEmission = OnSmashParticle.emission;

        //State Machine.
        idle = new State<BossStates>("Idle");
        var think = new State<BossStates>("Thinking");
        var pursue = new State<BossStates>("Pursue");
        var reposition = new State<BossStates>("Reposition");
        var charge = new State<BossStates>("Charge");
        var BasicCombo = new State<BossStates>("Attack_BasicCombo");
        var JumpAttack = new State<BossStates>("Attack_SimpleJump");
        var KillerJump = new State<BossStates>("Attack_KillerJump");
        var dead = new State<BossStates>("Dead");

        /*
         * .OnEnter += (previousState) => { };
         * .OnUpdate += () => { };
         * .OnExit += (nextState) => { };
        */

        #region Estados

        #region Idle State

        idle.OnEnter += (x) => { anims.SetFloat("Movement", 0f); };
        idle.OnUpdate += () =>
        {
            //print("Enemy is OnIdle");
            var toDamage = sight.target.GetComponent<IKilleable>();
            if (!toDamage.IsAlive) return;

            if (sight.IsInSight() || sight.distanceToTarget < minDetectionRange)
                _targetDetected = true;

            //transitions
            if (_targetDetected)
            {
                if (sight.distanceToTarget > HighRange)
                    sm.Feed(BossStates.charge);
                else
                    sm.Feed(BossStates.pursue);
            }
        };
        idle.OnExit += (x) => { anims.SetFloat("Movement", 0f); };

        #endregion

        #region ThinkState

        think.OnEnter += (previousState) => 
        {
            print("Thinking...");
            anims.SetFloat("Movement", 0f);
            agent.isStopped = true;
            LookTowardsPlayer = true;
        };
        think.OnUpdate += () => 
        {
            if (thinkTime > 0)
                thinkTime -= Time.deltaTime;
            else
            {
                thinkTime = 0f;
                //Chequeo si el enemigo esta vivo.
                var toDamage = sight.target.GetComponent<IKilleable>();
                if (!toDamage.IsAlive)
                    sm.Feed(BossStates.idle);                        // Si el enemigo no esta Vivo, vuelvo a idle
                else                                                 // Si esta vivo...
                {
                    if (sight.angleToTarget > 45f) sm.Feed(BossStates.reposition);
                    else
                    {
                        if (sight.distanceToTarget <= AttackRange)
                        {
                            if (canPerformSimpleJump)
                            {
                                //Genero los pesos relevantes.

                                float[] posibilities = new float[2]
                                {
                                    (AttackWeight * ((AttackRange - (AttackRange - sight.distanceToTarget)) / AttackRange)),
                                    (JumpWeight * ((AttackRange - sight.distanceToTarget) / AttackRange))
                                };

                                if (RoulleteSelection.Roll(posibilities) == 1)
                                {
                                    print("Decidí hacer un salto corto.");
                                    sm.Feed(BossStates.closeJump);
                                    return;
                                }
                            }

                            print("Decidí Atacar.");
                            sm.Feed(BossStates.basicCombo);
                        }
                        if (sight.distanceToTarget > AttackRange)
                        {
                            //Si la distancia es mayor al Highrange.
                            if (sight.distanceToTarget > HighRange)
                            {
                                print("Decidí hacer un Charge.");
                                sm.Feed(BossStates.charge);
                                return;
                            }

                            float[] posibilities = new float[]
                            {
                                (KillerJumpWeight * ((HighRange - sight.distanceToTarget) / HighRange)),
                                PursueWeight
                            };

                            //Si la distancia es mayor al mediumRange.
                            if (RoulleteSelection.Roll(posibilities) == 0)
                            {
                                print("Decidí hacer un Killer Jump.");
                                sm.Feed(BossStates.killerJump);
                            }

                            // si esta visible pero fuera del rango de ataque...
                            print("Decidí perseguir al enemigo.");
                            sm.Feed(BossStates.pursue);                  // paso a pursue.
                        }
                    }
                }
            }
        };
        think.OnExit += (nextState) =>  
        {
            agent.isStopped = false;
            LookTowardsPlayer = false;
        };

        #endregion

        #region SimpleJumpAttack

        JumpAttack.OnEnter += (previousState) => 
        {
            //Animación we.
            anims.SetInteger("Attack", 5);
            anims.SetFloat("Movement", 0f);
            LookTowardsPlayer = false;
        };
        //JumpAttack.OnUpdate += () => { };
        JumpAttack.OnExit += (nextState) => 
        {
            anims.SetInteger("Attack", 0);
            LookTowardsPlayer = false;
        };

        #endregion

        #region KillerJumpState

        KillerJump.OnEnter += (previousState) => 
        {
            //Animación we.
            anims.SetInteger("Attack", 4);
            anims.SetFloat("Movement", 0f);
            LookTowardsPlayer = false;
        };
        KillerJump.OnExit += (nextState) => { LookTowardsPlayer = true; };

        #endregion

        #region Pursue State
        pursue.OnEnter += (x) =>
        {
            //print("Chasing After Player...");
            anims.SetFloat("Movement", 1f);
            LookTowardsPlayer = true;
        };
        pursue.OnUpdate += () =>
        {
            //transitions
            if (!IsAlive) sm.Feed(BossStates.dead);

            if (sight.distanceToTarget < AttackRange) //Si entra dentro del rango de ataque.
                sm.Feed(BossStates.basicCombo);

            //if (sight.distanceToTarget > sight.range) //Si el objetivo se va afuera del rango de visión.
            //    sm.Feed(BossStates.idle);

            //Actions.
            agent.Move(sight.dirToTarget * MovementSpeed * Time.deltaTime);
        };

        #endregion

        #region Charge
        charge.OnEnter += (previousState) => 
        {
            //Primero me quedo quieto.
            agent.isStopped = true;
            //Empiezo la animación del rugido.
            anims.SetBool("Roar", true);
        };
        charge.OnUpdate += () => 
        {
            float distance = Vector3.Distance(transform.position, _initialChargePosition);

            if (charging && distance < maxChargeDistance)
            {
                //Me muevo primero
                agent.Move(_chargeDir * chargeSpeed * Time.deltaTime);

                //Sino...
                //Voy calculando la distancia en la que me estoy moviendo
                distance = Vector3.Distance(transform.position, _initialChargePosition);
                //print("Charging distance" + distance);
            }
            //Si la distancia es mayor al máximo
            else if (distance > maxChargeDistance)
                sm.Feed(BossStates.think); //Me detengo.
        };
        charge.OnExit += (nextState) =>
        {
            attackDamage = BasicAttackDamages[0];
            ChargeEmission.enabled = false;
            charging = false;
            LookTowardsPlayer = true;
        };
        #endregion

        #region Attack State

        BasicCombo.OnEnter += (x) =>
        {
            print("Enemy started AttackMode");
            SetAttackState(1);
            agent.isStopped = true;
            LookTowardsPlayer = false;
            //StartCoroutine(AttackCombo());
        };
        BasicCombo.OnUpdate += () =>
        {
            //Seteo el primer ataque.
            if (AttackPhase == 0)
            {
                thinkTime = 2f;
                sm.Feed(BossStates.think);
            }
        };
        BasicCombo.OnExit += (x) =>
        {
            agent.isStopped = false;
            LookTowardsPlayer = true;
        };

        #endregion

        #region Reposition State

        reposition.OnEnter += (previousState) => 
        {
            print("Reposicionando we");
            LookTowardsPlayer = true;
        };
        reposition.OnUpdate += () => 
        {
            if (sight.angleToTarget < 5f)
                sm.Feed(BossStates.think);
            else
                agent.isStopped = true;
        };

        #endregion

        #region Dead State
        dead.OnEnter += (x) =>
        {
            print("Enemy is dead");
            anims.SetTrigger("Dead");
            StopAllCoroutines();
            Die();
            // Posible Spawneo de cosas.
        };
        #endregion

        #endregion

        #region Transiciones
        //Transiciones posibles.
        idle.AddTransition(BossStates.pursue, pursue)
            .AddTransition(BossStates.charge, charge)
            .AddTransition(BossStates.dead, dead);

        pursue.AddTransition(BossStates.dead, dead)
              .AddTransition(BossStates.basicCombo, BasicCombo)
              .AddTransition(BossStates.think, think);

        charge.AddTransition(BossStates.dead, dead)
              .AddTransition(BossStates.think, think);

        think.AddTransition(BossStates.dead, dead)
             .AddTransition(BossStates.pursue, pursue)
             .AddTransition(BossStates.reposition, reposition)
             .AddTransition(BossStates.charge, charge)
             .AddTransition(BossStates.closeJump, JumpAttack)
             .AddTransition(BossStates.killerJump, KillerJump)
             .AddTransition(BossStates.basicCombo, BasicCombo);

        JumpAttack.AddTransition(BossStates.dead, dead)
                  .AddTransition(BossStates.think, think);

        KillerJump.AddTransition(BossStates.dead, dead)
                  .AddTransition(BossStates.think, think);


        reposition.AddTransition(BossStates.dead, dead)
                  .AddTransition(BossStates.think, think);

        BasicCombo.AddTransition(BossStates.dead, dead)
              .AddTransition(BossStates.think, think); 
        #endregion

        sm = new GenericFSM<BossStates>(idle);
    }
    void Update()
    {
#if UNITY_EDITOR
        CurrentState = sm.currentState; 
#endif

        sight.Update();

        if (LookTowardsPlayer)
            transform.forward = Vector3.Lerp(transform.forward, sight.dirToTarget, _rotationLerpSpeed * Time.deltaTime);

        sm.Update();
    }

    //================================= Animation Events ======================================

    /// <summary>
    /// Cambio la animación a Charge! y realizo los seteos importantes.
    /// </summary>
    public void SetCharge()
    {

        //Activo la animación.
        anims.SetBool("Roar", false);
        anims.SetFloat("Movement", 2f);
        ChargeEmission.enabled = true;
        agent.isStopped = false;
        charging = true;
        LookTowardsPlayer = false;
        attackDamage = ChargeDamage;

        //Guardo la posición inicial.
        _initialChargePosition = transform.position;

        //Calculo la dirección a la que me voy a mover.
        _chargeDir = (Target.position - transform.position).normalized;
    }
    /// <summary>
    /// Setea el final de la animación del Salto Simple.
    /// </summary>
    public void SetSimpleJumpEnd()
    {
        thinkTime = 1f;
        sm.Feed(BossStates.think);
    }
    /// <summary>
    /// Setea el final de la animación del Killer Jump.
    /// </summary>
    public void KillerJumpEnd()
    {
        thinkTime = 2f;
        StartCoroutine(JumpAttackCoolDown());
        sm.Feed(BossStates.think);
    }

    //============================= Corrutinas ================================================

    IEnumerator simpleJumpAttackCoolDown()
    {
        canPerformSimpleJump = false;
        yield return new WaitForSeconds(SimpleJumpAttackCooldown);
        canPerformSimpleJump = true;
    }
    IEnumerator JumpAttackCoolDown()
    {
        canPerformKIllerJump = false;
        yield return new WaitForSeconds(killerJumpAttackCooldown);
        canPerformKIllerJump = true;
    }

    /// <summary>
    /// Permite Sincronizar el parámetro del animator y del boss simultáneamente.
    /// </summary>
    /// <param name="stateID">ID del estado.</param>
    public void SetAttackState(int stateID)
    {
        AttackPhase = stateID;
        anims.SetInteger("Attack", stateID);
    }

    //============================= Colisiones ================================================

    private void OnCollisionEnter(Collision collision)
    {
        //Detectamos collisiones con otros cuerpos.
        if (collision.gameObject.tag == "Player")
        {
            if (!charging) return;

            IKilleable killeable = collision.gameObject.GetComponent<IKilleable>();
            if (killeable != null && killeable.IsAlive && !killeable.invulnerable)
            {
                print("EL colisionador choco con algo KILLEABLE: " + collision.gameObject.name);
                sm.Feed(BossStates.think);

                var Target = collision.gameObject.GetComponent<IDamageable<HitData, HitResult>>();
                Target.Hit(GetDamageStats());
                collision.rigidbody.AddForce(_chargeDir * ChargeCollisionForce, ForceMode.Impulse);
                return;
            }
        }

        if (collision.gameObject.layer == Layer_walls)
        {
            if (charging) sm.Feed(BossStates.think);
            print("EL colisionador choco con algo que no es KILLEABLE: " + collision.gameObject.name);
        }
    }

    //============================= Debugg ====================================================

#if UNITY_EDITOR

    [Header("DEBUG (Boss)")]
    public bool Debug_MinimunRange = true;

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        if (Debug_MinimunRange)
        {
            Gizmos.color = Color.cyan;
            Gizmos.matrix *= Matrix4x4.Scale(new Vector3(1, 0, 1));
            Gizmos.DrawWireSphere(transform.position, MinimunRange);
        }
    }

#endif
}
