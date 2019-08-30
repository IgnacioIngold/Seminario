using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using Core.Entities;
using IA.StateMachine.Generic;
using IA.LineOfSight;

[RequireComponent(typeof(Animator)), RequireComponent(typeof(NavMeshAgent))]
public class Cursed : MonoBehaviour, IKilleable, IAttacker<object[]>
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

    [Header("Estadísticas")]
    [SerializeField] float _hp;
    public float speed;
    public float attackRate;
    public float AttackRange;
    public float attackDamage;
    public float rotationLerpSpeed;
    public float minForwardAngle;

    public float Health
    {
        get { return _hp; }
        set
        {
            _hp = value;
            if (_hp < 0) _hp = 0;

            EnemyHP.text = "Enemy Health: " + _hp;
        }
    }
    bool Attacking = false;
    Vector3 _lastEnemyPositionKnown = Vector3.zero;

    [Header("Attacks")]
    public Collider DamageCollider;

    [Header("Line Of Sight")]
    [SerializeField] LineOfSight sight = null;


    //----------------Componentes de Unity
    Animator anims;
    NavMeshAgent agent;

    //------------------Componentes Propios
    GenericFSM<enemyState> sm;
    State<enemyState> idle;
    public float minDetectionRange;
    public float MediumRange = 40f;
    public float HighRange = 60f;


    //----------------------Propiedades.
    /// <summary>
    /// Retorna verdadero si mis puntos de vida son mayores a 0
    /// </summary>
    public bool IsAlive => _hp > 0;
    public bool invulnerable => false;

    bool targetDetected = false;

#if UNITY_EDITOR
    [Header("Debug")]
    public Text EnemyHP;
    public bool Debug_Gizmos          = false;
    public bool Debug_LineOFSight     = false;
    public bool Debug_Attacks         = false;
    public bool Debug_DetectionRanges = false; 
#endif

    private void Awake()
    {
        //Seteos iniciales.
        anims = GetComponentInChildren<Animator>();
        //transform.CreateSightEntity();
        agent = GetComponent<NavMeshAgent>();
        agent.speed = speed;

        EnemyHP.text = "Enemy Health: " + _hp;

        //Collider
        DamageCollider.enabled = false;

        //State Machine.
        idle = new State<enemyState>("Idle");
        var pursue = new State<enemyState>("Pursue");
        var charge = new State<enemyState>("Charge");
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

            //transitions
            if (targetDetected) sm.Feed(enemyState.pursue);

            if (sight.IsInSight() || sight.distanceToTarget < minDetectionRange)
            {
                targetDetected = true;
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
            if (sight.angleToTarget > minForwardAngle)
                transform.forward = Vector3.Slerp(transform.forward, sight.dirToTarget, rotationLerpSpeed);
            else
            {
                transform.forward = Vector3.Slerp(transform.forward, sight.dirToTarget, rotationLerpSpeed);
                agent.Move(sight.dirToTarget * speed * Time.deltaTime);
            }
        };

        #endregion

        charge.OnEnter += (previousState) => 
        {
            //Activo la animación.

            //Activo la detección.

            //Calculo la dirección a la que me voy a mover.
        };
        charge.OnUpdate += () => 
        {
            //Me muevo primero

            //Si Collisione con algo, me detengo.

            //Sino...
                //Voy calculando la distancia en la que me estoy moviendo
                //Si la distancia es mayor al máximo
                    //Me detengo.
        };
        charge.OnExit += (nextState) => 
        {
            // Reseteo el boleano de la colisión.
        };

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

    //Implementación de Ikilleable, permite recibir daño.
    public void GetDamage(params object[] DamageStats)
    {
        float Damage = (float)DamageStats[0];
        print("Enemigo ha recibido daño: " + Damage);
        Health -= Damage;

        //Activo Animación de "recibir Daño"

        if (!IsAlive)
            sm.Feed(enemyState.dead);
    }


    IEnumerator Attack()
    {
        Attacking = true;
        DamageCollider.enabled = true;

        //Activa Animación.
        anims.SetTrigger("attack");

        //Enfriamiento.
        yield return new WaitForSeconds(attackRate);
        DamageCollider.enabled = false;
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
    /// <summary>
    /// Retorna las estadísticas de combate de esta Unidad.
    /// </summary>
    public object[] GetDamageStats()
    {
        return new object[1] { attackDamage };
    }

    //Snippet for Debugg
#if (UNITY_EDITOR)
    void OnDrawGizmosSelected()
    {
        if (Debug_Gizmos)
        {
            var currentPosition = transform.position;

            if (Debug_LineOFSight)
            {
                Gizmos.color = sight.IsInSight() ? Color.green : Color.red;       //Target In Sight es un bool en una clase Externa.
                float distanceToTarget = sight.positionDiference.magnitude;   //mySight es una instancia de la clase LineOfSight.
                if (distanceToTarget > sight.range) distanceToTarget = sight.range;
                sight.dirToTarget.Normalize();
                Gizmos.DrawLine(currentPosition, currentPosition + sight.dirToTarget * distanceToTarget);

                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(currentPosition, currentPosition + Quaternion.Euler(0, sight.angle + 1, 0) * transform.forward * sight.range);
                Gizmos.DrawLine(currentPosition, currentPosition + Quaternion.Euler(0, -sight.angle - 1, 0) * transform.forward * sight.range);

                Gizmos.color = Color.gray;
                Gizmos.DrawLine(currentPosition, currentPosition + Quaternion.Euler(0, minForwardAngle + 1, 0) * transform.forward * sight.range);
                Gizmos.DrawLine(currentPosition, currentPosition + Quaternion.Euler(0, -minForwardAngle - 1, 0) * transform.forward * sight.range);

                Gizmos.color = Color.white;
                Gizmos.matrix *= Matrix4x4.Scale(new Vector3(1, 0, 1));
                Gizmos.DrawWireSphere(currentPosition, sight.range);
            }

            if (Debug_Attacks)
            {
                Gizmos.color = Color.red;
                Gizmos.matrix *= Matrix4x4.Scale(new Vector3(1, 0, 1));
                Gizmos.DrawWireSphere(currentPosition, AttackRange);
            }

            if (Debug_DetectionRanges)
            {
                Gizmos.color = Color.yellow;
                Gizmos.matrix *= Matrix4x4.Scale(new Vector3(1, 0, 1));
                Gizmos.DrawWireSphere(currentPosition, minDetectionRange);

                Gizmos.color = Color.magenta;
                Gizmos.matrix *= Matrix4x4.Scale(new Vector3(1, 0, 1));
                Gizmos.DrawWireSphere(currentPosition, MediumRange);

                Gizmos.color = Color.magenta;
                Gizmos.matrix *= Matrix4x4.Scale(new Vector3(1, 0, 1));
                Gizmos.DrawWireSphere(currentPosition, HighRange);
            }
        }
    }
#endif
}
