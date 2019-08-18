using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Core.Entities;
using IA.StateMachine.Generic;
using IA.LineOfSight;

[RequireComponent(typeof(Animator)), RequireComponent(typeof(NavMeshAgent))]
public class Cursed : MonoBehaviour, IKilleable
{
    //Estados
    public enum enemyState
    {
        idle,
        pursue,
        attack,
        dead
    }
    public enemyState MainState;

    [Header("Estadísticas")]
    public float health;
    public float speed;
    public float attackRate;
    public float AttackRange;
    public float attackDamage;
    public float rotationLerpSpeed;
    public float minForwardAngle;

    bool _canAttack = true;
    Vector3 _lastEnemyPositionKnown = Vector3.zero;

    [Header("Line Of Sight")]
    [SerializeField] Transform target = null;
    [SerializeField] LineOfSight sight = null;

    //Componentes de Unity
    Animator anims;
    NavMeshAgent agent;

    //Componentes Propios
    GenericFSM<enemyState> sm;
    State<enemyState> idle;

    //Propiedades.
    /// <summary>
    /// Retorna verdadero si mis puntos de vida son mayores a 0
    /// </summary>
    public bool IsAlive => health > 0;


    private void Awake()
    {
        //Seteos iniciales.
        anims = GetComponentInChildren<Animator>();
        //transform.CreateSightEntity();
        agent = GetComponent<NavMeshAgent>();
        agent.speed = speed;

        //State Machine.
        idle = new State<enemyState>("Idle");
        var pursue = new State<enemyState>("Pursue");
        var attack = new State<enemyState>("Attack");
        var dead = new State<enemyState>("Dead");

        #region Estados

        #region Idle State
        idle.OnUpdate += () =>
        {
            print("Enemy is OnIdle");

            //transitions
            if (sight.IsInSight()) //Si esta dentro de mi visión --> pursue.
                sm.Feed(enemyState.pursue);
        }; 
        #endregion

        #region Pursue State
        pursue.OnEnter += (x) =>
        {
            print("Enemy has started Chasing After Player");
        };
        pursue.OnUpdate += () =>
        {
            if (!IsAlive) sm.Feed(enemyState.dead);

            sight.Update();
            //_lastEnemyPositionKnown = sight.target.position; // Recordamos la ultima posición en el que el player fue visto.
            if (sight.angleToTarget > minForwardAngle)
                transform.forward = Vector3.Slerp(transform.forward, sight.dirToTarget, rotationLerpSpeed);
            else
            {
                transform.forward = Vector3.Slerp(transform.forward, sight.dirToTarget, rotationLerpSpeed);
                agent.Move(sight.dirToTarget * speed * Time.deltaTime);
            }

            //transitions
            if (sight.distanceToTarget < AttackRange) //Si entra dentro del rango de ataque.
                sm.Feed(enemyState.attack);

            if (sight.distanceToTarget > sight.range) //Si el objetivo se va afuera del rango de visión.
                sm.Feed(enemyState.idle);
        }; 
        #endregion

        #region Attack State
        attack.OnEnter += (x) =>
        {
            agent.isStopped = true;
            print("Enemy started AttackMode");
        };
        attack.OnUpdate += () =>
        {
            if (_canAttack)
            {
                print("Enemy is Attacking");
                StartCoroutine(Attack());
            }

            //Transitions
            if (!sight.IsInSight())
                sm.Feed(enemyState.idle);

            if (sight.IsInSight() && sight.distanceToTarget > AttackRange)
                sm.Feed(enemyState.pursue);
        };
        #endregion

        #region Dead State
        dead.OnEnter += (x) =>
        {
            print("Enemy is dead");
            // Posible Spawneo de cosas.
        };  
        #endregion

        #endregion

        #region Transiciones
        //Transiciones posibles.
        idle.AddTransition(enemyState.pursue, pursue)
            .AddTransition(enemyState.dead, dead);

        pursue.AddTransition(enemyState.dead, dead)
            .AddTransition(enemyState.attack, attack)
            .AddTransition(enemyState.idle, idle);

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

    //Implementación de Ikilleable, permite recibir daño.
    public void GetDamage(params object[] DamageStats)
    {
        float Damage = (float)DamageStats[0];
        health -= Damage;

        //Activo Animación de "recibir Daño"


        if (health <= 0)
            sm.Feed(enemyState.dead);
    }


    IEnumerator Attack()
    {
        _canAttack = false;

        //Activa Animación.
        //Anims.SetBool("Attack", True);

        //Cálculo de daño
        //var toDamage = target.GetComponent<IKilleable>();
        //object[] damageStats = new object[] { attackDamage };
        //toDamage.GetDamage(damageStats);

        //Enfriamiento.
        yield return new WaitForSeconds(attackRate);
        _canAttack = true;
    }

    //Snippet for Debugg
#if (UNITY_EDITOR)
    void OnDrawGizmosSelected()
    {
        var currentPosition = transform.position;

        Gizmos.color = sight.IsInSight() ? Color.green : Color.red;       //Target In Sight es un bool en una clase Externa.
        float distanceToTarget = sight.positionDiference.magnitude;   //mySight es una instancia de la clase LineOfSight.
        if (distanceToTarget > sight.range) distanceToTarget = sight.range;
        sight.dirToTarget.Normalize();
        Gizmos.DrawLine(transform.position, transform.position + sight.dirToTarget * distanceToTarget);

        Gizmos.color = Color.white;
        Gizmos.matrix *= Matrix4x4.Scale(new Vector3(1, 0, 1));
        Gizmos.DrawWireSphere(transform.position, sight.range);

        Gizmos.color = Color.red;
        Gizmos.matrix *= Matrix4x4.Scale(new Vector3(1, 0, 1));
        Gizmos.DrawWireSphere(transform.position, AttackRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(currentPosition, currentPosition + Quaternion.Euler(0, sight.angle + 1, 0) * transform.forward * sight.range);
        Gizmos.DrawLine(currentPosition, currentPosition + Quaternion.Euler(0, -sight.angle - 1, 0) * transform.forward * sight.range);

        Gizmos.color = Color.gray;
        Gizmos.DrawLine(currentPosition, currentPosition + Quaternion.Euler(0, minForwardAngle + 1, 0) * transform.forward * sight.range);
        Gizmos.DrawLine(currentPosition, currentPosition + Quaternion.Euler(0, -minForwardAngle - 1, 0) * transform.forward * sight.range);
    }
#endif
}
