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
        killerJump,
        closeJump,
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

    float thinkTime = 0f;
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
                        if (sight.distanceToTarget > HighRange)
                            sm.Feed(BossStates.charge);
                        if (sight.distanceToTarget > MediumRange)
                            sm.Feed(BossStates.killerJump);
                        if (sight.distanceToTarget > AttackRange)        // si esta visible pero fuera del rango de ataque...
                            sm.Feed(BossStates.pursue);                  // paso a pursue.
                        if (sight.distanceToTarget <= AttackRange)
                            sm.Feed(BossStates.pursue);
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

        #region KillerJumpState

        KillerJump.OnEnter += (previousState) => 
        {
            //Animación we.
            anims.SetInteger("Attack", 4);
            anims.SetFloat("Movement", 0f);
            StartCoroutine(KillerJumpAttack());
        };
        KillerJump.OnUpdate += () => { };
        KillerJump.OnExit += (nextState) =>  { };

        #endregion

        #region Pursue State
        pursue.OnEnter += (x) =>
        {
            print("Chasing After Player...");
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
        charge.OnEnter += (previousState) => { StartCoroutine(Charge()); };
        charge.OnUpdate += () => {};
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
            agent.isStopped = true;
            StartCoroutine(AttackCombo());
        };
        BasicCombo.OnExit += (x) =>
        {
            agent.isStopped = false;
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
    // Update is called once per frame
    void Update()
    {
        sight.Update();

        if (LookTowardsPlayer)
            transform.forward = Vector3.Lerp(transform.forward, sight.dirToTarget, rotationLerpSpeed * Time.deltaTime);

        sm.Update();
    }

    //=========================================================================================

    IEnumerator KillerJumpAttack()
    {
        yield return null;
    }

    IEnumerator AttackCombo()
    {
        //Inicio el primer ataque.
        anims.SetInteger("Attack",1);
        anims.SetFloat("Movement", 0f);
        LookTowardsPlayer = false;
        yield return null;

        float currentTransitionTime = getCurrentTransitionScaledTime();
        yield return new WaitForSeconds(currentTransitionTime);

        float remainingTime = getRemainingAnimTime("RightPunch", currentTransitionTime);
        anims.SetInteger("Attack", 2);
        yield return new WaitForSeconds(remainingTime);
        
        currentTransitionTime = getCurrentTransitionScaledTime();
        yield return new WaitForSeconds(currentTransitionTime);

        remainingTime = getRemainingAnimTime("LeftPunch", currentTransitionTime);
        anims.SetInteger("Attack", 3);
        yield return new WaitForSeconds(remainingTime);

        //Inicio el tercer ataque.
        currentTransitionTime = getCurrentTransitionScaledTime();
        yield return new WaitForSeconds(currentTransitionTime);

        remainingTime = getRemainingAnimTime("Great Sword Casting", currentTransitionTime);
        yield return new WaitForSeconds(remainingTime + 1f);

        //Cambio a pensar.
        thinkTime = 2f;
        sm.Feed(BossStates.think);
    }

    float getCurrentTransitionScaledTime()
    {
        return anims.GetAnimatorTransitionInfo(0).duration;
    }
    float getRemainingAnimTime(string ClipName, float transitionPassed = 0f)
    {
        AnimatorClipInfo[] clipInfo = anims.GetCurrentAnimatorClipInfo(0);
        float AnimTime = 0f;

        if (clipInfo != null && clipInfo.Length > 0)
        {
            AnimationClip currentClip = clipInfo[0].clip;
            //print("Clip Searched: " + ClipName + " ClipGetted: " + currentClip.name);

            if (currentClip.name == ClipName)
            {
                //print("currentClip is Correct!");
                AnimTime = currentClip.length;
                float passed = AnimTime - (AnimTime * transitionPassed);
                return passed;
            }
        }

        return AnimTime;
    }

    IEnumerator Charge()
    {
        //Primero me quedo quieto.
        agent.isStopped = true;

        //Empiezo haciendo un Roar.
        anims.SetBool("Roar", true);
        yield return new WaitForEndOfFrame();
        float currentTransitionTime = getCurrentTransitionScaledTime();
        yield return new WaitForSeconds(currentTransitionTime);
        float remainingTime = getRemainingAnimTime("Roar", currentTransitionTime);
        anims.SetBool("Roar", false);
        yield return new WaitForSeconds(remainingTime);

        //Ahora me puedo mover.
        agent.isStopped = false;

        //Activo la animación.
        print("Charging");
        anims.SetFloat("Movement", 2f);
        ChargeEmission.enabled = true;

        charging = true;
        LookTowardsPlayer = false;
        attackDamage = ChargeDamage;

        //Guardo la posición inicial.
        _initialChargePosition = transform.position;

        //Calculo la dirección a la que me voy a mover.
        _chargeDir = (Target.position - transform.position).normalized;

        float distance = Vector3.Distance(transform.position, _initialChargePosition);

        //Update
        do
        {
            //Me muevo primero
            agent.Move(_chargeDir * chargeSpeed * Time.deltaTime);

            //Sino...
            //Voy calculando la distancia en la que me estoy moviendo
            distance = Vector3.Distance(transform.position, _initialChargePosition);

            //Si la distancia es mayor al máximo
            if (distance > maxChargeDistance)
                sm.Feed(BossStates.think); //Me detengo.

            yield return new WaitForEndOfFrame();
        } while (charging && distance < maxChargeDistance);
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
