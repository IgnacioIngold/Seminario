using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using IA.StateMachine.Generic;
using Core.Entities;
using Random = UnityEngine.Random;
using Utility.Timers;
using Core;

public enum BasicEnemyStates
{
    idle,
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
    /// <summary>
    /// Evento que se llama cuando el Enemigo recibió un golpe directo.
    /// </summary>
    public event Action OnGetHit = delegate { };
    /// <summary>
    /// Evento que se llama cuando el enemigo recibió un golpe y lo bloqueó.
    /// </summary>
    public event Action OnBlockHit = delegate { };
    #endregion

#if UNITY_EDITOR
    public BasicEnemyStates CurrentState; 
#endif
    GenericFSM<BasicEnemyStates> sm;
    public void FeedFSM(BasicEnemyStates nextState)
    {
        sm.Feed(nextState);
    }

    public float AlertedTime = 2f;
    public float AlertRadius = 10f;
    public bool isAttacking;

    [Header("Blocking")]
    public bool canBlock = true;
    public bool isBlocking = false;
    public float blockDuration = 1.467f;
    public float blockExtraTimePerHit = 0.7f;
    float blockTime = 0f;

    private float _alertedTimeRemaining = 0f;

    //======================== OVERRIDES & INTERFACES =========================================

    public override HitResult Hit(HitData HitInfo)
    {
        HitResult result = HitResult.Default();
        float damage = HitInfo.Damage;

        if (IsAlive && damage > 0)
        {
            //print("Estoy Vulnerable?: " + isVulnerableToAttacks);

            if (!isVulnerableToAttacks && _targetDetected) //Bloqueo.
            {
                //print("BLOQUEO");

                sm.Feed(BasicEnemyStates.blocking);
                result.HitBlocked = true;

                float damageReduced = damage * incommingDamageReduction;
                Health -= damageReduced;

                OnBlockHit();
            }
            else //HitNormal.
            {
                anims.SetTrigger("GetHit");

                OnGetHit();

                bool coincided = false;
                bool completedCombo = false;
                //Contamos la cantidad de hits que obtenemos.
                //Aumentamos el "timer"
                comboVulnerabilityCountDown = ComboWindow; //Reasignamos el tiempo del combo.
                //Si el tipo del ataque coincide con el del combo al que es vulnerable
                if (vulnerabilityCombos[1][_attacksRecieved] == HitInfo.AttackType)
                {
                    _attacksRecieved++;
                    print("Recibido : " + _attacksRecieved);

                    if (_attacksRecieved == 3)
                    {
                        comboVulnerabilityCountDown = 0f;
                        print(string.Format("Reducido a 0 segundos la vulnerabilidad, tiempo de vulnerabilidad es {0}", comboVulnerabilityCountDown));
                    }
                    else if (_attacksRecieved == 2)
                    {
                        comboVulnerabilityCountDown += 4f;
                        print(string.Format("Añadido {0} segundos al combo, tiempo de vulnerabilidad es {1}", 4f, comboVulnerabilityCountDown));
                    }
                    else
                        comboVulnerabilityCountDown += 1f;
                    Display_CorrectButtonHitted();

                    coincided = true;

                    if (_attacksRecieved == 3)
                    {
                        completedCombo = true;
                        Health = 0;
                    }

                    //Muestro el siguiente ataque.
                    //ShowNextVulnerability(_attacksRecieved);
                }

                if (coincided)
                    Health -= damage;
                else
                    Health -= damage * incommingDamageReduction;


                if (!IsAlive)
                {
                    //Si el enemigo es el que mato al Player, entonces le añade el bono acumulado. TO DO.
                    result.TargetEliminated = true;
                    if (completedCombo)
                        result.bloodEarned = BloodForKill * 2;
                    else
                        result.bloodEarned = BloodForKill;
                    sm.Feed(BasicEnemyStates.dead);
                }
                else
                {
                    result.HitConnected = true;
                    result.bloodEarned = BloodPerHit;

                    if (!_targetDetected)
                    {
                        _targetDetected = true;
                        sm.Feed(BasicEnemyStates.pursue);
                    }
                    else
                        sm.Feed(BasicEnemyStates.idle);

                    LookTowardsPlayer = true;
                    sm.Feed(BasicEnemyStates.think);
                }
            }

            var particle = Instantiate(OnHitParticle, transform.position, Quaternion.identity);
            Destroy(particle, 3f);
            EnemyHealthBar.FadeIn();
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
        VulnerableMarker.gameObject.SetActive(false);
        ButtonHitConfirm.gameObject.SetActive(false);

        //Vulnerabilidad
        var MainVulnerability = new Inputs[] { Inputs.light, Inputs.light, Inputs.strong};
        vulnerabilityCombos = new Dictionary<int, Inputs[]>();
        vulnerabilityCombos.Add(1, MainVulnerability);

        #region State Machine

        var idle = new State<BasicEnemyStates>("Idle");
        var alerted = new State<BasicEnemyStates>("Alerted");
        var pursue = new State<BasicEnemyStates>("pursue");
        var attack = new State<BasicEnemyStates>("Attacking");
        var block = new State<BasicEnemyStates>("Blocking");
        var think = new State<BasicEnemyStates>("Thinking");
        var dead = new State<BasicEnemyStates>("Dead");

        #region Transiciones
        idle.AddTransition(BasicEnemyStates.dead, dead)
            .AddTransition(BasicEnemyStates.attack, attack)
            .AddTransition(BasicEnemyStates.alerted, alerted)
            .AddTransition(BasicEnemyStates.blocking, block);

        alerted.AddTransition(BasicEnemyStates.dead, dead)
               .AddTransition(BasicEnemyStates.attack, attack)
               .AddTransition(BasicEnemyStates.pursue, pursue);

        pursue.AddTransition(BasicEnemyStates.dead, dead)
              .AddTransition(BasicEnemyStates.attack, attack)
              .AddTransition(BasicEnemyStates.blocking, block);

        attack.AddTransition(BasicEnemyStates.dead, dead)
              .AddTransition(BasicEnemyStates.pursue, pursue)
              .AddTransition(BasicEnemyStates.idle, idle)
              .AddTransition(BasicEnemyStates.think, think)
              .AddTransition(BasicEnemyStates.blocking, block);

        block.AddTransition(BasicEnemyStates.dead, dead)
             .AddTransition(BasicEnemyStates.attack, attack)
             .AddTransition(BasicEnemyStates.think, think)
             .AddTransition(BasicEnemyStates.blocking, block);


        think.AddTransition(BasicEnemyStates.dead, dead)
             .AddTransition(BasicEnemyStates.pursue, pursue)
             .AddTransition(BasicEnemyStates.idle, idle)
             .AddTransition(BasicEnemyStates.attack, attack)
             .AddTransition(BasicEnemyStates.blocking, block);

        #endregion

        #region Estados

        #region Idle

        idle.OnEnter += (previousState) =>
        {
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

        #endregion

        #region Alerted

        alerted.OnEnter += (previousState) =>
        {
            //print("Enemy has been Alerted");
            _alertedTimeRemaining = AlertedTime;

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
            //Setear Animación.
            anims.SetBool("Walking", true);
        };
        pursue.OnUpdate += () =>
        {
            //Correr como si no hubiera un mañana (?
            transform.forward = Vector3.Slerp(transform.forward, sight.dirToTarget, _rotationLerpSpeed);

            agent.Move(sight.dirToTarget * MovementSpeed * Time.deltaTime);

            if (sight.distanceToTarget <= AttackRange)
                sm.Feed(BasicEnemyStates.attack);
        };
        pursue.OnExit += (nextState) => anims.SetBool("Walking", false);

        #endregion

        #region Attack

        attack.OnEnter += (previousState) =>
        {
            //Debug.LogWarning("Enemy Start of Attack");
            isAttacking = true;
            //GetComponent<NavMeshAgent>().enabled = false;
            _rotationLerpSpeed = AttackRotationLerpSpeed;
            LookTowardsPlayer = true;
            anims.SetTrigger("SimpleAttack");
            //SetVulnerabity(true);
        };
        attack.OnUpdate += () =>
        {
            var toDamage = sight.target.GetComponent<IKilleable>();

            if (!toDamage.IsAlive)
                sm.Feed(BasicEnemyStates.idle);
        };
        attack.OnExit += (nextState) =>
        {
            //GetComponent<NavMeshAgent>().enabled = true;
            _rotationLerpSpeed = NormalRotationLerpSeed;
            isAttacking = false;
            Debug.LogWarning("Enemy End of Attack");
        };

        #endregion

        #region Blocking

        block.OnEnter += (previousState) =>
        {
            anims.SetBool("Blocking", true);
            isBlocking = true;
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
                isBlocking = false;
            }
            anims.SetBool("Blocking", false);
        };

        #endregion

        #region Thinking

        think.OnEnter += (previousState) =>
        {
            //print("Thinking");
            StartCoroutine(thinkAndWatch());
        };
        //think.OnUpdate += () => { };
        //think.OnExit += (nextState) =>
        //{
        //    //print(string.Format("Exiting from Thinking, next State will be {0}", nextState.ToString()));
        //}; 

        #endregion

        dead.OnEnter += (previousState) =>
        {
            StopAllCoroutines();
            anims.SetTrigger("Die");
            Die();
        };

        #endregion

        sm = new GenericFSM<BasicEnemyStates>(idle); 

        #endregion
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        CurrentState = sm.currentState;
#endif

        //print("Sigo al objetivo: " + LookTowardsPlayer);

        #region CountDown de Vulnerabilidad.

        if (comboVulnerabilityCountDown > 0)
            comboVulnerabilityCountDown -= Time.deltaTime;
        else if (comboVulnerabilityCountDown <= 0)
        {
            //print("Se acabó el tiempo de vulnerabilidad");
            _attacksRecieved = 0;
            //SetVulnerabity(false);
            comboVulnerabilityCountDown = 0;
            ButtonHitConfirm.gameObject.SetActive(false);
        }

        #endregion

        sight.Update();

        if (LookTowardsPlayer && _targetDetected)
            transform.forward = Vector3.Lerp(transform.forward, sight.dirToTarget, _rotationLerpSpeed * Time.deltaTime);

        if (isVulnerableToAttacks)
            VulnerableMarker.gameObject.SetActive(true);
        else if (VulnerableMarker.gameObject.activeSelf)
            VulnerableMarker.gameObject.SetActive(false);

        if (_hp <= 0)
            sm.Feed(BasicEnemyStates.dead);

        sm.Update();
    }

    //public override void SetVulnerabity(bool vulnerable, int combo = 1)
    //{
    //    canBlock = !vulnerable;

    //    base.SetVulnerabity(vulnerable);
    //}

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
