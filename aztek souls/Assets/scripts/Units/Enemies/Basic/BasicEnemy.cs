using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using IA.StateMachine.Generic;
using Core.Entities;
using Random = UnityEngine.Random;
using Utility.Timers.RunTime;
using Core;

public enum BasicEnemyStates
{
    idle,
    stunned,
    hurted,
    alerted,
    pursue,
    attack,
    blocking,
    think,
    dead
}

public class BasicEnemy : BaseUnit
{
    #region Eventos
    ///// <summary>
    ///// Evento que se llama cuando el Enemigo recibió un golpe directo.
    ///// </summary>
    //public event Action OnGetHit = delegate { };
    /// <summary>
    /// Evento que se llama cuando el enemigo recibió un golpe y lo bloqueó.
    /// </summary>
    public event Action OnBlockHit = delegate { };
    #endregion

    GenericFSM<BasicEnemyStates> sm;
    public void FeedFSM(BasicEnemyStates nextState)
    {
        sm.Feed(nextState);
    }

    public float AlertedTime = 2f;
    public float AlertRadius = 10f;

    [Header("Blocking")]
    public bool canBlock = true;
    public bool isBlocking = false;
    public float blockDuration = 1.467f;
    public float blockExtraTimePerHit = 0.7f;
    float blockTime = 0f;

    private float _alertedTimeRemaining = 0f;
    public bool vulnerability = false;

    //============================= EDITOR ONLY ===============================================

#if UNITY_EDITOR
    public BasicEnemyStates CurrentState;
#endif

    //============================ PROPIEDADES ================================================

    int[] animationParams;
    public bool AP_Walking
    {
        get => anims.GetBool(animationParams[0]);
        set => anims.SetBool(animationParams[0], value);
    }
    public bool AP_Blocking
    {
        get => anims.GetBool(animationParams[1]);
        set => anims.SetBool(animationParams[1], value);
    }
    public bool AP_SimpleAttack
    {
        get => anims.GetBool(animationParams[2]);
        set => anims.SetBool(animationParams[2], value);
    }
    public bool AP_GetHit
    {
        get => anims.GetBool(animationParams[3]);
        set => anims.SetBool(animationParams[3], value);
    }
    public bool AP_Die
    {
        get => anims.GetBool(animationParams[4]);
        set => anims.SetBool(animationParams[4], value);
    }

    public void SetAttackTrigger()
    {
        StartCoroutine(AttackTrigger());
    }
    IEnumerator AttackTrigger()
    {
        AP_SimpleAttack = true;
        yield return new WaitForEndOfFrame();
        AP_SimpleAttack = false;
    }
    IEnumerator HitTrigger()
    {
        AP_GetHit = true;
        yield return new WaitForEndOfFrame();
        AP_GetHit = false;
    }

    //======================== OVERRIDES & INTERFACES =========================================

    public override HitResult Hit(HitData HitInfo)
    {
        HitResult result = HitResult.Default();
        float damage = HitInfo.Damage;

        if (!_targetDetected) _targetDetected = true; //Si golpeo mientras no esta siendo detectado.

        FRitmo.HitRecieved(HitInfo.AttackID, HitInfo.AttackType);

        EnemyHealthBar.FadeIn();

        if (IsAlive && damage > 0)
        {
            //HitNormal.
            sm.Feed(BasicEnemyStates.hurted);
            OnGetHit();

            Health -= damage;

            //Si el enemigo murió x el ataque.
            if (!IsAlive)
            {
                //Si el enemigo es el que mato al Player, entonces le añade el bono acumulado. TO DO.
                print("Se murio el wey");

                result.HitConnected = true;
                result.TargetEliminated = true;
                result.bloodEarned = BloodForKill;
                sm.Feed(BasicEnemyStates.dead);
            }
            //Si el enemigo no murió por el ataque.
            else
            {
                StartCoroutine(HitTrigger());

                result.HitConnected = true;

                if (FRitmo.AttackCoincided)
                    result.bloodEarned = BloodPerHit;

                LookTowardsPlayer = true;
                sm.Feed(BasicEnemyStates.hurted);
            }

            var particle = Instantiate(OnHitParticle, transform.position, Quaternion.identity);
            Destroy(particle, 3f);
        }

        return result;
    }
    public override void GetHitResult(HitResult result)
    {
        print("El enemigo conectó un Hit");
    }
    public override HitData DamageStats()
    {
        return new HitData() { Damage = attackDamage};
    }

    //=========================================================================================

    protected override void Awake()
    {
        base.Awake();

        //Si interrumpo el ataque... voy a stunned.
        FRitmo.OnComboSuccesfullyStart += () => 
        {
            AP_SimpleAttack = false;
            sm.Feed(BasicEnemyStates.stunned);
        };
        FRitmo.OnComboCompleted += () => { Health = 0; };
        FRitmo.TimeEnded += () => { sm.Feed(BasicEnemyStates.think); };
        FRitmo.OnComboFailed += () => { sm.Feed(BasicEnemyStates.think); };

        animationParams = new int[5];
        for (int i = 0; i < animationParams.Length; i++)
            animationParams[i] = anims.GetParameter(i).nameHash;

        Tuple<int, Inputs>[] data = new Tuple<int, Inputs>[3];
        data[0] = Tuple.Create(1, Inputs.light);
        data[1] = Tuple.Create(3, Inputs.light);
        data[2] = Tuple.Create(8, Inputs.strong);

        FRitmo.AddVulnerability(0, data);

        #region State Machine

        var idle = new State<BasicEnemyStates>("Idle");
        var stunned = new State<BasicEnemyStates>("Stunned");
        var alerted = new State<BasicEnemyStates>("Alerted");
        var pursue = new State<BasicEnemyStates>("pursue");
        var attack = new State<BasicEnemyStates>("Attacking");
        var block = new State<BasicEnemyStates>("Blocking");
        var think = new State<BasicEnemyStates>("Thinking");
        var dead = new State<BasicEnemyStates>("Dead");

        #region Transiciones
        idle.AddTransition(BasicEnemyStates.dead, dead)
            .AddTransition(BasicEnemyStates.stunned, stunned)
            .AddTransition(BasicEnemyStates.attack, attack)
            .AddTransition(BasicEnemyStates.alerted, alerted)
            .AddTransition(BasicEnemyStates.blocking, block);

        stunned.AddTransition(BasicEnemyStates.dead, dead)
               .AddTransition(BasicEnemyStates.think, think);

        alerted.AddTransition(BasicEnemyStates.dead, dead)
               .AddTransition(BasicEnemyStates.attack, attack)
               .AddTransition(BasicEnemyStates.pursue, pursue);

        pursue.AddTransition(BasicEnemyStates.dead, dead)
              .AddTransition(BasicEnemyStates.attack, attack)
              .AddTransition(BasicEnemyStates.blocking, block);

        attack.AddTransition(BasicEnemyStates.dead, dead)
              .AddTransition(BasicEnemyStates.stunned, stunned)
              .AddTransition(BasicEnemyStates.pursue, pursue)
              .AddTransition(BasicEnemyStates.idle, idle)
              .AddTransition(BasicEnemyStates.think, think)
              .AddTransition(BasicEnemyStates.blocking, block);

        block.AddTransition(BasicEnemyStates.dead, dead)
             .AddTransition(BasicEnemyStates.attack, attack)
             .AddTransition(BasicEnemyStates.stunned, stunned)
             .AddTransition(BasicEnemyStates.think, think)
             .AddTransition(BasicEnemyStates.blocking, block);

        think.AddTransition(BasicEnemyStates.dead, dead)
             .AddTransition(BasicEnemyStates.pursue, pursue)
             .AddTransition(BasicEnemyStates.stunned, stunned)
             .AddTransition(BasicEnemyStates.idle, idle)
             .AddTransition(BasicEnemyStates.attack, attack)
             .AddTransition(BasicEnemyStates.blocking, block);

        #endregion

        #region Estados

        #region Idle

        idle.OnEnter += (previousState) =>
        {
            //Seteo la animación inicial.
            AP_Walking = false;
            AP_SimpleAttack = false;
        };
        idle.OnUpdate += () =>
        {
            var toDamage = sight.target.GetComponent<IKilleable>();
            if (!toDamage.IsAlive) return;

            if (sight.IsInSight() || sight.distanceToTarget < minDetectionRange)
                _targetDetected = true;

            if (_targetDetected)
                sm.Feed(BasicEnemyStates.alerted);
        };
        //idle.OnExit += (nextState) => { };
        #endregion

        stunned.OnEnter += (previousState) => { print("Quieto como una planta --> STUNNED"); };
        stunned.OnExit += (NextState) => { print("YA NO TAN Quieto como una planta STUNNED OFF"); };
        //#region Hurted

        //hurted.OnEnter += (previousState) => 
        //{
        //    AP_SimpleAttack = false;
        //    AP_GetHit = true;
        //};
        //hurted.OnExit += (nextState) => 
        //{
        //    AP_GetHit = false;
        //};

        //#endregion
        #region Alerted

        alerted.OnEnter += (previousState) =>
        {
            //print("Enemy has been Alerted");
            _alertedTimeRemaining = AlertedTime;
            _targetDetected = true;

            //EnemyHealthBar.FadeIn();

            if (_alertFriends)
            {
                var FriendsToAlert = FindObjectsOfType<BaseUnit>()
                    .Select(x => Tuple.Create(x, Vector3.Distance(transform.position, x.transform.position)))
                    .OrderBy(x => x.Item2)
                    .TakeWhile(x => x.Item2 < AlertRadius)
                    .Select(x => x.Item1);
                foreach (BaseUnit Ally in FriendsToAlert)
                {
                    if (Ally != this)
                    {
                        print("Enemigo alertado: " + Ally.gameObject.name);
                        Ally.AllyDiscoversEnemy(Target);
                    }
                }
            }
        };
        alerted.OnUpdate += () =>
        {
            if (_alertedTimeRemaining > 0)
            {
                _alertedTimeRemaining -= Time.deltaTime;
                transform.forward = Vector3.Slerp(transform.forward, sight.dirToTarget, _rotationLerpSpeed);
            }
            else
                sm.Feed(BasicEnemyStates.pursue);
        };
        //alerted.OnExit += (nextState) => { };

        #endregion
        #region Pursue

        pursue.OnEnter += (previousState) =>
        {
            //print("pursue");
            AP_Walking = true;
        };
        pursue.OnUpdate += () =>
        {
            //Correr como si no hubiera un mañana (?
            transform.forward = Vector3.Slerp(transform.forward, sight.dirToTarget, _rotationLerpSpeed);

            agent.Move(sight.dirToTarget * MovementSpeed * Time.deltaTime);

            if (sight.distanceToTarget <= AttackRange)
                sm.Feed(BasicEnemyStates.attack);
        };
        pursue.OnExit += (nextState) => AP_Walking = false;

        #endregion
        #region Attack

        attack.OnEnter += (previousState) =>
        {
            //SetAttackTrigger();
            AP_SimpleAttack = true;
            _rotationLerpSpeed = AttackRotationLerpSpeed;
            LookTowardsPlayer = true;
            FRitmo.ShowVulnerability();
        };
        attack.OnUpdate += () =>
        {
            var toDamage = sight.target.GetComponent<IKilleable>();

            if (!toDamage.IsAlive)
                sm.Feed(BasicEnemyStates.idle);
        };
        attack.OnExit += (nextState) =>
        {
            _rotationLerpSpeed = NormalRotationLerpSeed;
            //Debug.LogWarning("Enemy End of Attack");
        };

        #endregion
        #region Blocking

        block.OnEnter += (previousState) =>
        {
            AP_Blocking = true;
            LookTowardsPlayer = true;

            blockTime = blockDuration;
        };
        block.OnUpdate += () =>
        {
            blockTime -= Time.deltaTime;

            if (blockTime <= 0)
            {
                if (!IsAlive)
                    sm.Feed(BasicEnemyStates.dead);

                var toDamage = sight.target.GetComponent<IKilleable>();

                if (sight.distanceToTarget < AttackRange)
                    sm.Feed(BasicEnemyStates.attack);
                else
                    sm.Feed(BasicEnemyStates.think);
            }
        };
        block.OnExit += (nextState) =>
        {
            if (nextState != BasicEnemyStates.blocking)
            {
                LookTowardsPlayer = true;
                AP_Blocking = false;
            }
        };

        #endregion
        #region Thinking

        think.OnEnter += (previousState) =>
        {
            //print("Thinking");
            StartCoroutine(thinkAndWatch());
        };

        #endregion

        dead.OnEnter += (previousState) =>
        {
            StopAllCoroutines();
            AP_Die = true;
            //HideVulnerability();
            Die();
        };

        #endregion

        sm = new GenericFSM<BasicEnemyStates>(idle); 

        #endregion
    }

    // Update is called once per frame
    void Update()
    {
        #region Editor Only
        #if UNITY_EDITOR
        CurrentState = sm.currentState;
        #endif 
        #endregion

        sight.Update();

        if (LookTowardsPlayer && _targetDetected)
            transform.forward = Vector3.Lerp(transform.forward, sight.dirToTarget, _rotationLerpSpeed * Time.deltaTime);

        if (_hp <= 0)
            sm.Feed(BasicEnemyStates.dead);

        sm.Update();
    }

    //=========================== CORRUTINES ==================================================

    IEnumerator thinkAndWatch()
    {
        var toDamage = sight.target.GetComponent<IKilleable>();
        float waitTime = Random.Range(0.5f, 1.5f);
        yield return new WaitForSeconds(waitTime);

        //print("Watchtime Started");
        float watchTime = Random.Range(2f, 4f);

        while (watchTime > 0)
        {
            watchTime -= Time.deltaTime;
            transform.forward = Vector3.Slerp(transform.forward, sight.dirToTarget, _rotationLerpSpeed);

            if (sight.distanceToTarget < AttackRange)
                break;

            yield return null;
        }

        if (toDamage.IsAlive)
        {
            if (sight.distanceToTarget < AttackRange)
                sm.Feed(BasicEnemyStates.attack);

            if (sight.distanceToTarget > AttackRange)
                sm.Feed(BasicEnemyStates.pursue);
        }
    }
}
