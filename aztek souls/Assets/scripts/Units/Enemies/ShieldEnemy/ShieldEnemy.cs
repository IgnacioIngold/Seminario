using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IA.StateMachine.Generic;
using Core.Entities;
using System;
using Random = UnityEngine.Random;
using IA.RandomSelections;

public enum ShieldEnemyStates
{
    idle,
    alerted,
    block,
    vulnerable,
    parry,
    attack,
    pursue,
    think,
    dead
}

public class ShieldEnemy : BaseUnit
{
    public float AlertedTime = 2f;
    public float AlertRadius = 10f;

    GenericFSM<ShieldEnemyStates> _sm;
    private float _alertedTimeRemaining = 0f;

    private bool _attacking;

#if(UNITY_EDITOR)
    [SerializeField] ShieldEnemyStates current;
    private bool LookTowardsPlayer = true;
    private float ThinkTime;
    private float remainingThinkTime;
#endif


    //======================== OVERRIDES & INTERFACES =========================================

    public override void GetDamage(params object[] DamageStats)
    {
        anims.SetTrigger("getDamage");
        StopAllCoroutines();

        IAttacker<object[]> Aggresor = (IAttacker<object[]>)DamageStats[0];
        //Confirmar hit o no.

        //Si no estoy guardando.
        Health -= (float)DamageStats[1];

        base.GetDamage(DamageStats);

        //Aviso que estoy Muerto We.
        if (!IsAlive)
        {
            _sm.Feed(ShieldEnemyStates.dead);
            return;
        }

        if (!_targetDetected)
        {
            _targetDetected = true;
            _sm.Feed(ShieldEnemyStates.pursue);
        }
        else
            _sm.Feed(ShieldEnemyStates.idle);
    }

    public override object[] GetDamageStats()
    {
        return new object[2] { this, attackDamage };
    }

    //=========================================================================================

    protected override void Awake()
    {
        base.Awake();

        //State Machine.
        var idle = new State<ShieldEnemyStates>("Idle");
        var alerted = new State<ShieldEnemyStates>("Alerted");
        var pursue = new State<ShieldEnemyStates>("pursue");
        var blocking = new State<ShieldEnemyStates>("Bloquing");
        var vulnerable = new State<ShieldEnemyStates>("Vulnerable");
        var parry = new State<ShieldEnemyStates>("Parrying");
        var attack = new State<ShieldEnemyStates>("Attacking");
        var think = new State<ShieldEnemyStates>("Thinking");
        var dead = new State<ShieldEnemyStates>("Dead");

        /*
         * .OnEnter += (previousState) => { };
         * .OnUpdate += () => { };
         * .OnExit += (nextState) => { };
        */

        /*
            anims.SetBool("Dead", true);
            anims.SetTrigger("getDamage");
            anims.SetFloat("Moving", 1f);
            anims.SetInteger("Attack", 1);
            anims.SetBool("Blocking", true);
            anims.SetTrigger("BlockBreak");
            anims.SetTrigger("Parry");
        */
        #region Transitions

        idle.AddTransition(ShieldEnemyStates.alerted, alerted)
            .AddTransition(ShieldEnemyStates.dead, dead);

        alerted.AddTransition(ShieldEnemyStates.think, think)
               .AddTransition(ShieldEnemyStates.dead, dead);

        pursue.AddTransition(ShieldEnemyStates.pursue, pursue)
              .AddTransition(ShieldEnemyStates.think, think)
              .AddTransition(ShieldEnemyStates.dead, dead);

        blocking.AddTransition(ShieldEnemyStates.parry, parry)
                .AddTransition(ShieldEnemyStates.attack, attack)
                .AddTransition(ShieldEnemyStates.vulnerable, vulnerable)
                .AddTransition(ShieldEnemyStates.think, think)
                .AddTransition(ShieldEnemyStates.dead, dead);

        parry.AddTransition(ShieldEnemyStates.think, think)
             .AddTransition(ShieldEnemyStates.dead, dead);

        attack.AddTransition(ShieldEnemyStates.dead, dead)
              .AddTransition(ShieldEnemyStates.think, think);

        think.AddTransition(ShieldEnemyStates.dead, dead)
             .AddTransition(ShieldEnemyStates.block, blocking)
             .AddTransition(ShieldEnemyStates.parry, parry)
             .AddTransition(ShieldEnemyStates.vulnerable, vulnerable)
             .AddTransition(ShieldEnemyStates.attack, attack)
             .AddTransition(ShieldEnemyStates.pursue, pursue)
             .AddTransition(ShieldEnemyStates.idle, idle);

        #endregion

        #region Estados

        idle.OnEnter += (previousState) => 
        {
            anims.SetFloat("Moving", 0f);
        };
        idle.OnUpdate += () => 
        {
            var toDamage = sight.target.GetComponent<IKilleable>();
            if (!toDamage.IsAlive) return;

            if (sight.IsInSight() || sight.distanceToTarget < minDetectionRange)
                _targetDetected = true;

            if (_targetDetected)
                _sm.Feed(ShieldEnemyStates.alerted);
        };
        idle.OnExit += (nextState) => { };


        alerted.OnEnter += (previousState) => 
        {
            _alertedTimeRemaining = AlertedTime;
        };
        alerted.OnUpdate += () => 
        {
            _alertedTimeRemaining = AlertedTime;

            if (_alertedTimeRemaining > 0)
            {
                _alertedTimeRemaining -= Time.deltaTime;
                transform.forward = Vector3.Slerp(transform.forward, sight.dirToTarget, rotationLerpSpeed);
            }
            else
                _sm.Feed(ShieldEnemyStates.pursue);
        };
        alerted.OnExit += (nextState) => { };


        blocking.OnEnter += (previousState) => { };
        blocking.OnUpdate += () => { };
        blocking.OnExit += (nextState) => { };

        pursue.OnEnter += (previousState) => 
        {
            anims.SetFloat("Moving", 1f);
        };
        pursue.OnUpdate += () => 
        {
            //Correr como si no hubiera un mañana (?
            transform.forward = Vector3.Slerp(transform.forward, sight.dirToTarget, rotationLerpSpeed);

            agent.Move(sight.dirToTarget * MovementSpeed * Time.deltaTime);

            if (sight.distanceToTarget <= AttackRange)
                _sm.Feed(ShieldEnemyStates.attack);
        };
        //pursue.OnExit += (nextState) => 
        //{
        //    //Desactivo la animación. Esto queda así para que otra animación lo sobreescriba.
        //};

        attack.OnEnter += (previousState) => 
        {
            _attacking = true;
            agent.isStopped = true;
            rb.velocity = Vector3.zero;
            StartCoroutine(TriCombo());
        };
        //attack.OnUpdate += () => { };
        attack.OnExit += (nextState) => 
        {
            _attacking = false;
            agent.isStopped = false;
        };

        think.OnEnter += (previousState) => 
        {
            if (ThinkTime > 0) remainingThinkTime = ThinkTime;
        };
        think.OnUpdate += () => 
        {
            if (remainingThinkTime > 0)
                remainingThinkTime -= Time.deltaTime;
            else
            {
                remainingThinkTime = 0;
                IKilleable target = Target.GetComponent<IKilleable>();

                if (target.IsAlive)
                {
                    //Tomo una desición.
                    float blockImportance = (1 - (Health / MaxHP) * 10);
                    float pursueImportance = ((Health / MaxHP) * 10);
                    float AttackImportance = 5f;

                    //Desiciones posibles:
                    //Perseguir --> se realiza siempre que el enemigo esté más lejos de cierta distancia.
                    //Atacar --> Su importancia se mantiene.
                    //Bloquear --> Cuando la vida se va reduciendo su imporancia es mayor.

                    if (sight.distanceToTarget > AttackRange)
                    {
                        int decition = RoulleteSelection.Roll(new float[2] { pursueImportance, blockImportance });
                        print("Distance is bigger than the AttackRange.\nDecition was: " + decition);

                        if (decition == 0) _sm.Feed(ShieldEnemyStates.pursue);
                        if (decition == 1) _sm.Feed(ShieldEnemyStates.block);
                    }

                    if (sight.distanceToTarget < AttackRange)
                    {
                        int decition = RoulleteSelection.Roll(new float[2] { AttackImportance, blockImportance });
                        print("Distance is smaller than the AttackRange.\nDecition was: " + decition);

                        if (decition == 0) _sm.Feed(ShieldEnemyStates.pursue);
                        if (decition == 1) _sm.Feed(ShieldEnemyStates.block);
                    }
                }
            }
        };
        think.OnExit += (nextState) => 
        {
            print(string.Format("Exiting from Thinking, next State will be {0}", nextState.ToString()));
        };

        dead.OnEnter += (previousState) => 
        {
            StopAllCoroutines();
            anims.SetBool("Dead", true);
            Die();
        };

        #endregion

        _sm = new GenericFSM<ShieldEnemyStates>(idle);
    }

    // Update is called once per frame
    void Update()
    {
        print("Current State is: " + _sm.current.StateName);

        sight.Update();

        if (LookTowardsPlayer)
            transform.forward = Vector3.Lerp(transform.forward, sight.dirToTarget, rotationLerpSpeed * Time.deltaTime);

        _sm.Update();
    }

    IEnumerator biCombo()
    {
        anims.SetFloat("Moving", 0f);
        LookTowardsPlayer = false;

        //Inicio el primer ataque.
        anims.SetInteger("Attack", 2);
        yield return null;

        float currentTransitionTime = getCurrentTransitionScaledTime();
        yield return new WaitForSeconds(currentTransitionTime);

        float remainingTime = getRemainingAnimTime("attack2", currentTransitionTime);
        yield return new WaitForSeconds(remainingTime);

        //Segundo ataque.
        anims.SetInteger("Attack", 3);
        yield return null;
        currentTransitionTime = getCurrentTransitionScaledTime();
        yield return new WaitForSeconds(currentTransitionTime);

        remainingTime = getRemainingAnimTime("attack3", currentTransitionTime);
        yield return new WaitForSeconds(remainingTime);

        _sm.Feed(ShieldEnemyStates.think);
    }

    IEnumerator TriCombo()
    {
        anims.SetFloat("Moving", 0f);
        LookTowardsPlayer = false;

        //Inicio el primer ataque.
        anims.SetInteger("Attack", 1);
        yield return null;

        float currentTransitionTime = getCurrentTransitionScaledTime();
        yield return new WaitForSeconds(currentTransitionTime);

        float remainingTime = getRemainingAnimTime("attack1", currentTransitionTime);
        yield return new WaitForSeconds(remainingTime);

        //Segundo ataque.
        anims.SetInteger("Attack", 2);
        yield return null;
        currentTransitionTime = getCurrentTransitionScaledTime();
        yield return new WaitForSeconds(currentTransitionTime);

        remainingTime = getRemainingAnimTime("attack2", currentTransitionTime);
        yield return new WaitForSeconds(remainingTime);

        //Tercer ataque.
        anims.SetInteger("Attack", 3);
        yield return null;

        currentTransitionTime = getCurrentTransitionScaledTime();
        yield return new WaitForSeconds(currentTransitionTime);

        remainingTime = getRemainingAnimTime("attack3", currentTransitionTime);
        yield return new WaitForSeconds(remainingTime);

        _sm.Feed(ShieldEnemyStates.think);
    }
}
