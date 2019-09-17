using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IA.StateMachine.Generic;
using Core.Entities;
using System;
using Random = UnityEngine.Random;

public enum ShieldEnemyStates
{
    idle,
    alerted,
    block,
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
#endif


    //======================== OVERRIDES & INTERFACES =========================================

    public override void GetDamage(params object[] DamageStats)
    {
        //Seteo la animación.
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
        var blocking = new State<ShieldEnemyStates>("Bloquing");
        var pursue = new State<ShieldEnemyStates>("pursue");
        var attack = new State<ShieldEnemyStates>("Attacking");
        var think = new State<ShieldEnemyStates>("Thinking");
        var dead = new State<ShieldEnemyStates>("Dead");

        /*
         * .OnEnter += (previousState) => { };
         * .OnUpdate += () => { };
         * .OnExit += (nextState) => { };
        */

        #region Estados

        idle.OnEnter += (previousState) => 
        {
            //Activo la animación si es necesario.
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
            //Activo la animación.
        };
        pursue.OnUpdate += () => 
        {
            //Correr como si no hubiera un mañana (?
            transform.forward = Vector3.Slerp(transform.forward, sight.dirToTarget, rotationLerpSpeed);

            agent.Move(sight.dirToTarget * MovementSpeed * Time.deltaTime);

            if (sight.distanceToTarget <= AttackRange)
                _sm.Feed(ShieldEnemyStates.attack);
        };
        pursue.OnExit += (nextState) => 
        {
            //Desactivo la animación.
        };

        attack.OnEnter += (previousState) => 
        {
            _attacking = true;
            agent.isStopped = true;
            //Hago la corrutina.
        };
        attack.OnUpdate += () => { };
        attack.OnExit += (nextState) => 
        {
            _attacking = false;
            agent.isStopped = false;
        };

        think.OnEnter += (previousState) => { };
        think.OnUpdate += () => { };
        think.OnExit += (nextState) => 
        {
            print(string.Format("Exiting from Thinking, next State will be {0}", nextState.ToString()));
        };

        dead.OnEnter += (previousState) => 
        {
            StopAllCoroutines();
            //Seteo la animación de muerte.
            Die();
        };

        #endregion

        _sm = new GenericFSM<ShieldEnemyStates>(idle);
    }

    // Update is called once per frame
    void Update()
    {
        sight.Update();
        _sm.Update();
    }
}
