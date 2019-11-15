using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IA.StateMachine.Generic;
using IA.RandomSelections;
using Core;
using Core.Entities;
using System;

public enum BossStates
    {
        idle,
        think,
        charge,
        reposition,
        Smashed,
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

    [Header("Vulnerabilidad")]
    private bool _Smashed;
    private float _remainingSmashTime = 0f;
    public float SmashDuration = 4f;
    public float DamageMultiplier = 2f;


    [Header("Settings del Boss")]
    public float[] BasicAttackDamages;
    public int AttackPhase = 0;
    public float RepositionLerpSpeed = 20f;
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

    //TiempoActual - Duración - indiceDelAtaque
    Tuple<float , int> vulnerabilityDeMrda;
    public float[] times = new float[0];

    //============================== INTERFACES ===============================================
    //void ActualizarMrda()
    //{
    //    if (vulnerabilityDeMrda != null)
    //    {
    //        var TiempoActual = vulnerabilityDeMrda.Item1 - Time.deltaTime;
    //        print(string.Format("Actualizando esta poronga, tiempo actual {0}", TiempoActual));

    //        if (TiempoActual <= 0)
    //            reiniciarMRda();
    //    }
    //}
    //void contarMRda()
    //{
    //    int acumulación = vulnerabilityDeMrda.Item2 + 1; //Acumento la acumulación.
    //    vulnerabilityDeMrda = Tuple.Create(times[acumulación], acumulación);
    //}
    //void reiniciarMRda()
    //{
    //    var FirstTime = times[0];
    //    vulnerabilityDeMrda = Tuple.Create(0f, 0);
    //}

    /// <summary>
    /// Permite recibir daño e Informar al agresor del resultado de su ataque.
    /// </summary>
    /// <param name="DamageStats">Las estadísticas que afectan el "Daño" recibído.</param>
    public override HitResult Hit(HitData HitInfo)
    {
        HitResult result = HitResult.Default();

        float Damage = HitInfo.Damage;

        if (isVulnerableToAttacks && !_Smashed)
        {
            bool completedCombo = false;
            //Ahora si el ataque que recibimos coincide con el combo al que somos vulnerable.
            if (vulnerabilityCombos[1][_attacksRecieved] == HitInfo.AttackType)
            {
                //contarMRda();

                //if (_attacksRecieved == 3)
                //{
                //    comboVulnerabilityCountDown = 0f;
                //    //FinalDamage = HitInfo.Damage * CriticDamageMultiplier;
                //    completedCombo = true;

                //    print(string.Format("Reducido a 0 segundos la vulnerabilidad, tiempo de vulnerabilidad es {0}", comboVulnerabilityCountDown));
                //}
                //else if (_attacksRecieved == 2)
                //{
                //    comboVulnerabilityCountDown += 4f;

                //    print(string.Format("Añadido {0} segundos al combo, tiempo de vulnerabilidad es {1}", 4f, comboVulnerabilityCountDown));
                //}
                //else comboVulnerabilityCountDown += 1f;

                Display_CorrectButtonHitted();

                //Muestro el siguiente ataque.
                //ShowNextVulnerability(_attacksRecieved);
            }
            //else
            //    reiniciarMRda();

            if (completedCombo)
                sm.Feed(BossStates.Smashed);
        }
        else if (_Smashed)
        {
            Damage *= DamageMultiplier;
        }
        else
        {
            Damage *= incommingDamageReduction;
        }

        Health -= Damage;

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

            var particle = Instantiate(OnHitParticle, transform.position, Quaternion.identity);
            Destroy(particle, 3f);

            sm.Feed(BossStates.dead);
        }
        return result;
    }
    /// <summary>
    /// Retorna las estadísticas de combate de esta Unidad.
    /// </summary>
    /// <returns>Una estructura que contiene las stats involucradas en el ataque.</returns>
    public override HitData DamageStats()
    {
        return new HitData() { Damage = attackDamage, BreakDefence = false };
    }
    /// <summary>
    /// Informa a esta entidad del resultado del lanzamiento y conección de un ataque.
    /// </summary>
    /// <param name="result">Resultado del ataque lanzado.</param>
    public override void GetHitResult(HitResult result)
    {
        print(string.Format("{0} ha conectado un ataque.", gameObject.name));
    }

    //============================= Funciones Unity ===========================================

    protected override void Awake()
    {
        //reiniciarMRda();

        base.Awake();
        OnDie += () => {  };

        ChargeEmission = OnChargeParticle.emission;
        ChargeEmission.enabled = false;
        SmashEmission = OnSmashParticle.emission;

        //Vulnerabilidad
        vulnerabilityCombos = new Dictionary<int, Inputs[]>();
        vulnerabilityCombos.Add(0, new Inputs[] { Inputs.light, Inputs.light, Inputs.strong });

        #region State Machine

        idle = new State<BossStates>("Idle");
        var think = new State<BossStates>("Thinking");
        var pursue = new State<BossStates>("Pursue");
        var reposition = new State<BossStates>("Reposition");
        var charge = new State<BossStates>("Charge");
        var BasicCombo = new State<BossStates>("Attack_BasicCombo");
        var JumpAttack = new State<BossStates>("Attack_SimpleJump");
        var Smashed = new State<BossStates>("VulnerableToAttacks");
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
            charging = true;
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
            _rotationLerpSpeed = RepositionLerpSpeed;
            LookTowardsPlayer = true;
        };
        reposition.OnUpdate += () =>
        {
            if (sight.angleToTarget < 5f)
                sm.Feed(BossStates.think);
            else
                agent.isStopped = true;
        };
        reposition.OnExit += (NextState) =>
        {
            _rotationLerpSpeed = NormalRotationLerpSeed;
        };

        #endregion

        #region Vulnerable State

        Smashed.OnEnter += (previousState) =>
        {
            _Smashed = true;
            _remainingSmashTime = SmashDuration;
            anims.SetBool("Smashed", true);
        };
        Smashed.OnUpdate += () => { };
        Smashed.OnExit += (nextState) =>
        {
            _Smashed = false;
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
             .AddTransition(BossStates.Smashed, Smashed)
             .AddTransition(BossStates.basicCombo, BasicCombo);

        JumpAttack.AddTransition(BossStates.dead, dead)
                  .AddTransition(BossStates.think, think);

        KillerJump.AddTransition(BossStates.dead, dead)
                  .AddTransition(BossStates.think, think);


        reposition.AddTransition(BossStates.dead, dead)
                  .AddTransition(BossStates.think, think);

        BasicCombo.AddTransition(BossStates.dead, dead)
                  .AddTransition(BossStates.Smashed, Smashed)
                  .AddTransition(BossStates.think, think);

        Smashed.AddTransition(BossStates.think, think)
               .AddTransition(BossStates.dead, dead);

        #endregion

        sm = new GenericFSM<BossStates>(idle); 
        #endregion
    }
    void Update()
    {
#if UNITY_EDITOR
        CurrentState = sm.currentState;
#endif

        //ActualizarMrda();

        #region Tiempo de recuperación del estado de Derribo (Smashed)

        if (_remainingSmashTime > 0)
            _remainingSmashTime -= Time.deltaTime;
        else if (_Smashed && _remainingSmashTime <= 0)
        {
            _remainingSmashTime = 0;
            anims.SetBool("Smashed", false);
        }

        #endregion

        #region Tiempo de Vulnerabilidad contra Combo.
//        if (comboVulnerabilityCountDown > 0)
//            comboVulnerabilityCountDown -= Time.deltaTime;
//        else if (comboVulnerabilityCountDown <= 0)
//        {
//            //print("Se acabó el tiempo de vulnerabilidad");
//            _attacksRecieved = 0;
////            SetVulnerabity(false);
//            comboVulnerabilityCountDown = 0;
//            ButtonHitConfirm.gameObject.SetActive(false);
//        }
        #endregion

        #region Exposición de Vulnerabilidad
        if (isVulnerableToAttacks)
            VulnerableMarker.gameObject.SetActive(true);
        else if (VulnerableMarker.gameObject.activeSelf)
            VulnerableMarker.gameObject.SetActive(false); 
        #endregion

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
    public void RiseFromSmash()
    {
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
                HitData data = new HitData() { Damage = ChargeDamage, BreakDefence = false };
                Target.Hit(data);
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
