using System.Collections;
using System.Collections.Generic;
using Core;
using UnityEngine;
using IA.StateMachine.Generic;
using Core.Entities;

public enum FatEnemyStates
{
    idle,
    alert,
    rangeAttack,
    meleeAttack,
    pursue,
    think,
    dead
}

public class FatEnemy : BaseUnit
{
    public GameObject projectile;
    public GenericFSM<FatEnemyStates> _sm;
#if UNITY_EDITOR
    public FatEnemyStates currentState;
#endif

    [Header("Rangos de Ataque")]
    public float rangeAttack_MaxRange;
    public float meleeAttack_MaxRange;
    //Alerta
    public float AlertTime;
    private float _alertedTimeRemainig;
    //Ataque melee
    public float MeleeAttackTime;
    public float meleeAttackCooldownTime;
    private float _remainingMeleeAttackTime;
    //Ataque Rango.
    private float _remaingRangeAttackTime;


    //============================== INTERFACES ===============================================

    /// <summary>
    /// Retorna las estadísticas de combate de esta Unidad.
    /// </summary>
    /// <returns></returns>
    public override HitData GetDamageStats()
    {
        return HitData.Default();
    }
    /// <summary>
    /// Se llama cuando este enemigo recibe un Hit.
    /// </summary>
    /// <param name="EntryData"></param>
    /// <returns></returns>
    public override HitResult Hit(HitData EntryData)
    {
        HitResult result = HitResult.Default();

        //Completar

        return result;
    }
    /// <summary>
    /// Se llama cuando este Enemigo ejecuta un ataque.
    /// </summary>
    /// <param name="result"></param>
    public override void FeedHitResult(HitResult result)
    {
        //Completar.
    }

    //============================ UNITY FUNCTIONS ============================================

    protected override void Awake()
    {
        base.Awake();

        #region StateMachine

        #region Declaración de Estados

        var idle = new State<FatEnemyStates>("Idle");
        var alert = new State<FatEnemyStates>("Alerted");
        var rangeAttack = new State<FatEnemyStates>("RangeAttack");
        var meleeAttack = new State<FatEnemyStates>("MeleeAttack");
        var pursue = new State<FatEnemyStates>("Pursue");
        var think = new State<FatEnemyStates>("Thinking");
        var dead = new State<FatEnemyStates>("Dead");
        #endregion

        #region Transiciones.

        idle.AddTransition(FatEnemyStates.alert, alert)
            .AddTransition(FatEnemyStates.dead, dead);

        alert.AddTransition(FatEnemyStates.think, think)
             .AddTransition(FatEnemyStates.dead, dead);

        pursue.AddTransition(FatEnemyStates.think, think)
              .AddTransition(FatEnemyStates.dead, dead);

        think.AddTransition(FatEnemyStates.meleeAttack, meleeAttack)
             .AddTransition(FatEnemyStates.rangeAttack, rangeAttack)
             .AddTransition(FatEnemyStates.idle, idle)
             .AddTransition(FatEnemyStates.pursue, pursue)
             .AddTransition(FatEnemyStates.dead, dead);

        dead.AddTransition(FatEnemyStates.idle, idle); 
        #endregion

        //Estados
        /*
         * .OnEnter += (previousState) => { };
         * .OnUpdate += () => { };
         * .OnExit += (nextState) => { };
        */

        idle.OnEnter += (previousState) => 
        {
            //Animación.
        };
        idle.OnUpdate += () => 
        {
            var toDamage = sight.target.GetComponent<IKilleable>();
            if (!toDamage.IsAlive) return;

            if (sight.IsInSight() || sight.distanceToTarget < minDetectionRange)
                _targetDetected = true;

            if (_targetDetected)
                _sm.Feed(FatEnemyStates.alert);
        };
        //idle.OnExit += (nextState) => { };

        alert.OnEnter += (previousState) => 
        {
            //Tiempo de alerta
            if (_alertFriends)
                _alertedTimeRemainig = AlertTime;
        };
        alert.OnUpdate += () => 
        {
            if (_alertedTimeRemainig > 0)
            {
                _alertedTimeRemainig -= Time.deltaTime;
                LookTowardsPlayer = true;
            }
            else
                _sm.Feed(FatEnemyStates.think);
        };
        //alert.OnExit += (nextState) => { };

        pursue.OnEnter += (previousState) => 
        {
            //Animación
        };
        pursue.OnUpdate += () => 
        {
            transform.forward = Vector3.Slerp(transform.forward, sight.dirToTarget, _rotationLerpSpeed);

            if (sight.distanceToTarget > AttackRange) agent.Move(sight.dirToTarget * MovementSpeed * Time.deltaTime);

            if (sight.distanceToTarget <= AttackRange) _sm.Feed(FatEnemyStates.think);
        };
        //pursue.OnExit += (nextState) => { };

        think.OnEnter += (previousState) => { };
        think.OnUpdate += () => { };
        think.OnExit += (nextState) => { };

        rangeAttack.OnEnter += (previousState) => 
        {
            agent.isStopped = true;
            _remaingRangeAttackTime = 0;
        };
        rangeAttack.OnUpdate += () => 
        {
            //Atacamos varias veces segun el tiempo pase.
            _remaingRangeAttackTime -= Time.deltaTime;
            if (_remaingRangeAttackTime <= 0)
            {
                //Instanciamos un projectil.
                //Necesitamos un parent.
                //Necesitamos el prefab.

                //Reseteamos el tiempo para el siguiente ataque.
                _remaingRangeAttackTime = attackRate;
            }

            if (sight.distanceToTarget <= rangeAttack_MaxRange)
                _sm.Feed(FatEnemyStates.think);
        };
        rangeAttack.OnExit += (nextState) => 
        {
            agent.isStopped = false;
        };

        meleeAttack.OnEnter += (previousState) => 
        {
            rb.velocity = Vector3.zero;
            agent.isStopped = true;
            _remainingMeleeAttackTime = 0;  //Seteamos el valor original del ataque.
        };
        meleeAttack.OnUpdate += () => 
        {
            _remainingMeleeAttackTime -= Time.deltaTime;
            if (_remainingMeleeAttackTime <= 0)
            {
                //Activamos la animación.
                _remainingMeleeAttackTime = MeleeAttackTime + meleeAttackCooldownTime;
            }
        };
        meleeAttack.OnExit += (nextState) => 
        {
            agent.isStopped = false;
        };

        dead.OnEnter += (previousState) => 
        {
            //Animación
            Die();
        };

        _sm = new GenericFSM<FatEnemyStates>(idle);

        #endregion

    }
    void Update()
    {
        _sm.Update(); //Actualizamos la state Machine.

#if UNITY_EDITOR
        currentState = _sm.currentState; 
#endif
    }
}
