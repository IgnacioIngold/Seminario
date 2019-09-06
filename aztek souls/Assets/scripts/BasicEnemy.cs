using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IA.StateMachine.Generic;
using Core.Entities;

public enum BasicEnemyStates
{
    idle,
    alerted,
    pursue,
    attack,
    dead
}

public class BasicEnemy : BaseUnit
{
    public BasicEnemyStates MainState = BasicEnemyStates.idle;
    public Collider AttackCollider;

    public float AlertedTime = 2f;

    GenericFSM<BasicEnemyStates> sm;
    Vector3 _lastEnemyPositionKnow = Vector3.zero;
    bool Attacking = false;

    private float _alertedTimeRemaining = 0f;


    //======================== OVERRIDES & INTERFACES =========================================

    public override void GetDamage(params object[] DamageStats)
    {
        base.GetDamage(DamageStats);
    }

    public override object[] GetDamageStats()
    {
        return base.GetDamageStats();
    }

    //=========================================================================================

    protected override void Awake()
    {
        base.Awake();

        //State Machine
        var idle = new State<BasicEnemyStates>("Idle");
        var alerted = new State<BasicEnemyStates>("Alerted");
        var pursue = new State<BasicEnemyStates>("pursue");
        var attack = new State<BasicEnemyStates>("Attacking");
        var dead = new State<BasicEnemyStates>("Dead");

        #region Transiciones
        idle.AddTransition(BasicEnemyStates.dead, dead)
            .AddTransition(BasicEnemyStates.alerted, alerted);

        alerted.AddTransition(BasicEnemyStates.dead, dead)
               .AddTransition(BasicEnemyStates.pursue, pursue);

        pursue.AddTransition(BasicEnemyStates.dead, dead)
              .AddTransition(BasicEnemyStates.attack, attack);

        attack.AddTransition(BasicEnemyStates.dead, dead)
              .AddTransition(BasicEnemyStates.pursue, pursue)
              .AddTransition(BasicEnemyStates.idle, idle);
        #endregion

        /*
         * .OnEnter += (previousState) => { };
         * .OnUpdate += () => { };
         * .OnExit += (nextState) => { };
        */

        #region Estados

        idle.OnEnter += (previousState) => 
        {
            print("Idle");
            //Seteo la animación inicial.
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

        alerted.OnEnter += (previousState) => 
        {
            print("Enemy has been Alerted");

            _viewDirection = sight.dirToTarget;

            _alertedTimeRemaining = AlertedTime;

            sm.Feed(BasicEnemyStates.pursue);
        };
        alerted.OnUpdate += () => 
        {
            if (_alertedTimeRemaining > 0)
                _alertedTimeRemaining -= Time.deltaTime;
            else
                sm.Feed(BasicEnemyStates.pursue);
        };
        //alerted.OnExit += (nextState) => { };

        pursue.OnEnter += (previousState) => 
        {
            
        };
        pursue.OnUpdate += () => 
        {
            //Calcular la dirección al player
            //Correr como si no hubiera un mañana (?
            //Si entro en rango de ataque... pos lo ataco
        };
        pursue.OnExit += (nextState) => { };

        attack.OnEnter += (previousState) => { };
        attack.OnUpdate += () => { };
        attack.OnExit += (nextState) => { };

        dead.OnEnter += (previousState) => { }; 

        #endregion

        sm = new GenericFSM<BasicEnemyStates>(idle);
    }

    // Update is called once per frame
    void Update()
    {
        sm.Update();
    }
}
