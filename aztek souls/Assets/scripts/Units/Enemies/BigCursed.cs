using System.Collections;
using UnityEngine;
using Core.Entities;
using IA.StateMachine.Generic;

public class BigCursed : BaseUnit
{
    //Estados
    public enum enemyState
    {
        idle,
        think,
        charge,
        pursue,
        attack,
        dead
    }
    public enemyState MainState;
    GenericFSM<enemyState> sm;
    State<enemyState> idle;
    public Collider AttackCollider;

    [Header("Estadísticas")]
    bool Attacking = false;
    Vector3 _lastEnemyPositionKnown = Vector3.zero;


    [Header("Charge")]
    public Collider ChargeCollider;
    public float chargeSpeed = 30f;
    public float maxChargeDistance = 100f;

    bool stopCharging = false;
    Vector3 _initialChargePosition;
    Vector3 _chargeDir = Vector3.zero;

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
            sm.Feed(enemyState.dead);

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

        //State Machine.
        idle = new State<enemyState>("Idle");
        var think = new State<enemyState>("Thinking");
        var pursue = new State<enemyState>("Pursue");
        var charge = new State<enemyState>("Charge");
        var JumpAttack = new State<enemyState>("AirAttack");
        var attack = new State<enemyState>("Attack");
        var dead = new State<enemyState>("Dead");

        /*
         * .OnEnter += (previousState) => { };
         * .OnUpdate += () => { };
         * .OnExit += (nextState) => { };
        */

        #region Estados

        #region Idle State
        idle.OnEnter += (x) => { anims.SetBool("isWalking", false); };
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
                    sm.Feed(enemyState.charge);

                sm.Feed(enemyState.pursue);
            }
        }; 
        #endregion

        #region Pursue State
        pursue.OnEnter += (x) =>
        {
            print("Enemy has started Chasing After Player");
            anims.SetBool("isWalking",true);
        };
        pursue.OnUpdate += () =>
        {
            //transitions
            if (!IsAlive) sm.Feed(enemyState.dead);

            sight.Update();
            if (sight.distanceToTarget < AttackRange) //Si entra dentro del rango de ataque.
                sm.Feed(enemyState.attack);

            if (sight.distanceToTarget > sight.range) //Si el objetivo se va afuera del rango de visión.
                sm.Feed(enemyState.idle);

            //Actions.
            //_lastEnemyPositionKnown = sight.target.position; // Recordamos la ultima posición en el que el player fue visto.
            if (sight.angleToTarget > _minForwardAngle)
                transform.forward = Vector3.Slerp(transform.forward, sight.dirToTarget, rotationLerpSpeed);
            else
            {
                transform.forward = Vector3.Slerp(transform.forward, sight.dirToTarget, rotationLerpSpeed);
                agent.Move(sight.dirToTarget * MovementSpeed * Time.deltaTime);
            }
        };

        #endregion

        #region Charge
        charge.OnEnter += (previousState) =>
        {
            //Activo la animación.

            print("CHAAAAAAAAAAAAAAARGEEEEE");

            //Activo la detección.
            ChargeCollider.enabled = true;

            //Guardo la posición inicial.
            _initialChargePosition = transform.position;

            //Calculo la dirección a la que me voy a mover.
            _chargeDir = (Target.position - transform.position).normalized;
        };
        charge.OnUpdate += () =>
        {
            //Me muevo primero
            //transform.forward = Vector3.Slerp(transform.forward, sight.dirToTarget, rotationLerpSpeed);
            agent.Move(_chargeDir * chargeSpeed * Time.deltaTime);

            //Si Collisione con algo, me detengo. (Paso a Idle)
            if (stopCharging)
                sm.Feed(enemyState.idle);

            //Sino...
            //Voy calculando la distancia en la que me estoy moviendo
            float distance = Vector3.Distance(transform.position, _initialChargePosition);
            //Si la distancia es mayor al máximo
            if (distance > maxChargeDistance)
            {
                //Me detengo.
                print("TE pasaste de verga :D");
                sm.Feed(enemyState.idle);
            }
        };
        charge.OnExit += (nextState) =>
        {
            // Reseteo el boleano de la colisión.
            stopCharging = false;

            //DesActivo la detección.
            ChargeCollider.enabled = false;
        }; 
        #endregion

        #region Attack State
        attack.OnEnter += (x) =>
        {
            agent.isStopped = true;
            anims.SetBool("isWalking", false);
            print("Enemy started AttackMode");
        };
        attack.OnUpdate += () =>
        {
            if (!Attacking)
            {
                print("Enemy is Attacking");
                StartCoroutine(Attack());
            }
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
        idle.AddTransition(enemyState.pursue, pursue)
            .AddTransition(enemyState.charge, charge)
            .AddTransition(enemyState.dead, dead);

        pursue.AddTransition(enemyState.dead, dead)
              .AddTransition(enemyState.attack, attack)
              .AddTransition(enemyState.idle, idle);

        charge.AddTransition(enemyState.dead, dead)
              .AddTransition(enemyState.idle, idle)
              .AddTransition(enemyState.attack, attack);

        attack.AddTransition(enemyState.dead, dead)
              .AddTransition(enemyState.pursue, pursue)
              .AddTransition(enemyState.idle, idle); 
        #endregion

        sm = new GenericFSM<enemyState>(idle);
    }
    // Update is called once per frame
    void Update()
    {
        sm.Update();
    }

    //=========================================================================================

    IEnumerator Attack()
    {
        Attacking = true;

        //Activa Animación.
        anims.SetTrigger("attack");

        //Enfriamiento.
        yield return new WaitForSeconds(attackRate);
        Attacking = false;

        //Chequeo si el enemigo esta vivo.
        var toDamage = sight.target.GetComponent<IKilleable>();
        if (!toDamage.IsAlive)
            sm.Feed(enemyState.idle);                        // Si el enemigo no esta Vivo, vuelvo a idle
        else                                                 // Si esta vivo...
        {
            if (!sight.IsInSight())                          // pero no esta en mi línea de visión...
                sm.Feed(enemyState.idle);                    // vuelvo a idle.
            else
            if (sight.distanceToTarget > AttackRange)        // si esta visible pero fuera del rango de ataque...
                sm.Feed(enemyState.pursue);                  // paso a pursue.
        }
    }
}
