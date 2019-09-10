using System.Collections;
using UnityEngine;
using Core.Entities;
using IA.StateMachine.Generic;

public enum BossStates
    {
        idle,
        think,
        charge,
        reposition,
        pursue,
        basicCombo,
        dead
    }

public class BigCursed : BaseUnit
{
    //Estados
    public BossStates MainState;
    GenericFSM<BossStates> sm;
    State<BossStates> idle;
    public Collider AttackCollider;

    [Header("Settings del Boss")]
    public float[] BasicAttackDamages;
    Vector3 _lastEnemyPositionKnown = Vector3.zero;

    [Header("Layers")]
    public int Layer_walls;
    public int Layer_Killeable;

    [Header("Charge")]
    public ParticleSystem OnChargeParticle;
    ParticleSystem.EmissionModule ChargeEmission;
    public float ChargeDamage = 50f;
    public float chargeSpeed = 30f;
    public float ChargeCollisionForce = 20f;
    public float maxChargeDistance = 100f;

    //Private:

    bool charging = false;
    Vector3 _initialChargePosition;
    Vector3 _chargeDir = Vector3.zero;
    private bool LookTowardsPlayer = true;

    //============================== INTERFACES ===============================================

    /// <summary>
    /// Implementación de Ikilleable, permite recibir daño.
    /// </summary>
    /// <param name="DamageStats">Las estadísticas que afectan el "Daño" recibído.</param>
    public override void GetDamage(params object[] DamageStats)
    {
        float Damage = (float)DamageStats[0];
        Health -= Damage;

        if (!IsAlive)
            sm.Feed(BossStates.dead);

        base.GetDamage();
    }
    /// <summary>
    /// Retorna las estadísticas de combate de esta Unidad.
    /// </summary>
    /// <returns>Un array de objetos, donde cada objeto es una Estadística que afecta el daño.</returns>
    public override object[] GetDamageStats()
    {
        return new object[1] { attackDamage };
    }

    //=========================================================================================


    protected override void Awake()
    {
        base.Awake();
        OnDie += () => {  };

        ChargeEmission = OnChargeParticle.emission;
        ChargeEmission.enabled = false;

        //State Machine.
        idle = new State<BossStates>("Idle");
        var think = new State<BossStates>("Thinking");
        var pursue = new State<BossStates>("Pursue");
        var charge = new State<BossStates>("Charge");
        var reposition = new State<BossStates>("Reposition");
        var JumpAttack = new State<BossStates>("AirAttack");
        var BasicCombo = new State<BossStates>("Attack_BasicCombo");
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

                sm.Feed(BossStates.pursue);
            }
        };

        #endregion

        #region ThinkState

        think.OnEnter += (previousState) => 
        {
            anims.SetBool("isWalking", false);

            //Chequeo si el enemigo esta vivo.
            var toDamage = sight.target.GetComponent<IKilleable>();
            if (!toDamage.IsAlive)
                sm.Feed(BossStates.idle);                        // Si el enemigo no esta Vivo, vuelvo a idle
            else                                                 // Si esta vivo...
            {
                if (sight.angleToTarget > 45f) sm.Feed(BossStates.reposition);
                else
                {
                    if (sight.distanceToTarget > AttackRange)        // si esta visible pero fuera del rango de ataque...
                        sm.Feed(BossStates.pursue);                  // paso a pursue.
                    if (sight.distanceToTarget <= AttackRange)
                        sm.Feed(BossStates.pursue);
                }
            }
        };
        //think.OnUpdate += () => {};
        //think.OnExit += (nextState) =>  {};

        #endregion

        #region Pursue State
        pursue.OnEnter += (x) =>
        {
            print("Chasing After Player...");
            anims.SetFloat("Movement", 1f);
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
            print("Charging");
            //Activo la animación.
            anims.SetFloat("Movement", 2f);
            ChargeEmission.enabled = true;

            charging = true;
            LookTowardsPlayer = false;
            attackDamage = ChargeDamage;

            //Guardo la posición inicial.
            _initialChargePosition = transform.position;

            //Calculo la dirección a la que me voy a mover.
            _chargeDir = (Target.position - transform.position).normalized;
        };
        charge.OnUpdate += () =>
        {
            //Me muevo primero
            agent.Move(_chargeDir * chargeSpeed * Time.deltaTime);

            //Sino...
            //Voy calculando la distancia en la que me estoy moviendo
            float distance = Vector3.Distance(transform.position, _initialChargePosition);
            
            //Si la distancia es mayor al máximo
            if (distance > maxChargeDistance)
                sm.Feed(BossStates.think); //Me detengo.
        };
        charge.OnExit += (nextState) =>
        {
            ChargeEmission.enabled = false;

            charging = false;
            LookTowardsPlayer = true;
        }; 
        #endregion

        #region Attack State

        BasicCombo.OnEnter += (x) =>
        {
            print("Enemy started AttackMode");
            agent.isStopped = true;
            anims.SetBool("isWalking", false);
            StartCoroutine(AttackCombo());
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
            anims.SetTrigger("died");
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
             .AddTransition(BossStates.basicCombo, BasicCombo);

        reposition.AddTransition(BossStates.dead, dead)
                  .AddTransition(BossStates.think, think);

        BasicCombo.AddTransition(BossStates.dead, dead)
              .AddTransition(BossStates.think, think); 
        #endregion

        sm = new GenericFSM<BossStates>(idle);
    }
    // Update is called once per frame
    void Update()
    {
        sight.Update();

        if (LookTowardsPlayer)
            transform.forward = Vector3.Lerp(transform.forward, sight.dirToTarget, rotationLerpSpeed * Time.deltaTime);

        sm.Update();
    }

    //=========================================================================================

    IEnumerator AttackCombo()
    {
        //Activa Animación.
        anims.SetInteger("Attack",1);
        LookTowardsPlayer = false;
        yield return null;

        var info = anims.GetAnimatorTransitionInfo(0);
        float transitionTIme = info.duration;
        //print("Transition = " + transitionTIme);
        yield return new WaitForSeconds(transitionTIme);

        bool knowHowMuchIsLeft = false;
        float remainingTime = 0;

        AnimatorClipInfo[] clipInfo = anims.GetCurrentAnimatorClipInfo(0);
        AnimationClip currentClip;
        if (clipInfo != null && clipInfo.Length > 0)
        {
            currentClip = clipInfo[0].clip;
            //print("Current Clip = " + currentClip.name);

            if (!knowHowMuchIsLeft && currentClip.name == "BasicAttack")
            {
                float length = currentClip.length;
                float passed = length - (length * transitionTIme);
                remainingTime = passed;
                knowHowMuchIsLeft = true;

                print("currentClip is Correct!");
                //float normTime = anims.GetCurrentAnimatorStateInfo(0).normalizedTime;
                //print("TimePassed is = " + passed);
            }
            else
                yield return null;

            if (knowHowMuchIsLeft)
                yield return new WaitForSeconds(remainingTime);

            yield return new WaitForSeconds(1f);
        }

        sm.Feed(BossStates.think);
    }

    //=========================================================================================

    //Detectamos collisiones con otros cuerpos.
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == Layer_Killeable)
        {
            if (!charging) return;

            IKilleable killeable = collision.gameObject.GetComponent<IKilleable>();
            if (killeable != null && killeable.IsAlive && !killeable.invulnerable)
            {
                //print("EL colisionador choco con algo KILLEABLE: " + collision.gameObject.name);
                 sm.Feed(BossStates.think);

                killeable.GetDamage(GetDamageStats());
                collision.rigidbody.AddForce(_chargeDir * ChargeCollisionForce, ForceMode.Impulse);
                return;
            }
        }

        if (collision.gameObject.layer == Layer_walls)
        {
            if (charging) sm.Feed(BossStates.think);
            //print("EL colisionador choco con algo que no es KILLEABLE: " + collision.gameObject.name);
        }
    }
}
