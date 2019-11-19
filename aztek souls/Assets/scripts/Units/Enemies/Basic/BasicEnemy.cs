using System.Collections;
using System.Linq;
using UnityEngine;
using IA.StateMachine.Generic;
using Core.Entities;
using System;
using Random = UnityEngine.Random;
using Utility.Timers;

public enum BasicEnemyStates
{
    idle,
    alerted,
    pursue,
    attack,
    think,
    dead
}

public class BasicEnemy : BaseUnit
{
    public event Action onGetHit = delegate { };

    public ParticleSystem VulnerableMark;

#if UNITY_EDITOR
    public BasicEnemyStates CurrentState; 
#endif
    GenericFSM<BasicEnemyStates> sm;

    public float AlertedTime = 2f;
    public float AlertRadius = 10f;

    [Header("BerserkTime")]
    public float vulnerableTime = 1.5f;
    public float incommingDamageReduction = 0.3f;
    public bool canBlock = true;
    Vector3 _lastEnemyPositionKnow = Vector3.zero;


    private bool isVulnerableToAttacks = false;
    private float _alertedTimeRemaining = 0f;
    private bool LookTowardsPlayer = true;
    //private bool BersekrMode;

    //======================== OVERRIDES & INTERFACES =========================================

    //int recieved = 0;
    public override void GetDamage(params object[] DamageStats)
    {
        float damage = (float)DamageStats[1];
        if (IsAlive && damage > 0)
        {
            IAttacker<object[]> Aggresor = (IAttacker<object[]>)DamageStats[0];
            bool breakStance = (bool)DamageStats[2];

            StopCoroutine(thinkAndWatch());
            StopCoroutine(SimpleAttack());

            print("I can block: " + canBlock);

            if (canBlock)
            {
                anims.SetTrigger("block");
                StartCoroutine(Block());
                Aggresor.OnHitBlocked(new object[] { 2 });

                float damageReduced = damage * incommingDamageReduction;
                object[] damageStat = new object[] {Aggresor, damageReduced, breakStance };
                Health -= damageReduced;
                base.GetDamage(damageStat);
            }
            else
            {
                anims.SetTrigger("GetHit");
                base.GetDamage(DamageStats);

                Health -= damage;
                onGetHit();

                //damage *= incommingDamageReduction;

                if (!IsAlive)
                {
                    //Si el enemigo es el que mato al Player, entonces le añade el bono acumulado. TO DO.
                    Aggresor.OnKillConfirmed(new object[] { BloodForKill });
                    sm.Feed(BasicEnemyStates.dead);
                    return;
                }
                else
                {
                    var hitConfirmData = new object[1] { 0f };
                    if (isVulnerableToAttacks) hitConfirmData = new object[] { BloodPerHit };
                    Aggresor.OnHitConfirmed(hitConfirmData);

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

            //StopAllCoroutines();
            ////if(!isVulnerableToAttacks) recieved++;
        }
    }

    public override object[] GetDamageStats()
    {
        return new object[3] { this, attackDamage, false };
    }

    IEnumerator vulnerableToAttacks()
    {
        canBlock = false;
        isVulnerableToAttacks = true;

        //BersekrMode = false;
        //StopCoroutine(Block());

        yield return new WaitForSeconds(vulnerableTime);
        canBlock = true;
        isVulnerableToAttacks = false;
    }

    //=========================================================================================

    protected override void Awake()
    {
        base.Awake();

        sight.target = FindObjectOfType<Player>().GetComponent<Transform>();

        //State Machine
        var idle = new State<BasicEnemyStates>("Idle");
        var alerted = new State<BasicEnemyStates>("Alerted");
        var pursue = new State<BasicEnemyStates>("pursue");
        var attack = new State<BasicEnemyStates>("Attacking");
        var think = new State<BasicEnemyStates>("Thinking");
        var dead = new State<BasicEnemyStates>("Dead");

        #region Transiciones
        idle.AddTransition(BasicEnemyStates.dead, dead)
            .AddTransition(BasicEnemyStates.attack, attack)
            .AddTransition(BasicEnemyStates.alerted, alerted);

        alerted.AddTransition(BasicEnemyStates.dead, dead)
               .AddTransition(BasicEnemyStates.attack, attack)
               .AddTransition(BasicEnemyStates.pursue, pursue);

        pursue.AddTransition(BasicEnemyStates.dead, dead)
              .AddTransition(BasicEnemyStates.attack, attack);

        attack.AddTransition(BasicEnemyStates.dead, dead)
              .AddTransition(BasicEnemyStates.pursue, pursue)
              .AddTransition(BasicEnemyStates.idle, idle)
              .AddTransition(BasicEnemyStates.think, think);


        think.AddTransition(BasicEnemyStates.dead, dead)
             .AddTransition(BasicEnemyStates.pursue, pursue)
             .AddTransition(BasicEnemyStates.idle, idle)
             .AddTransition(BasicEnemyStates.attack, attack);

        #endregion

        #region Estados

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
                transform.forward = Vector3.Slerp(transform.forward, sight.dirToTarget, rotationLerpSpeed);
            }
            else
                sm.Feed(BasicEnemyStates.pursue);
        };
        //alerted.OnExit += (nextState) => { };

        pursue.OnEnter += (previousState) => 
        {
            //print("pursue");
            //Setear Animación.
            anims.SetBool("Walking", true);
        };
        pursue.OnUpdate += () => 
        {
            //Correr como si no hubiera un mañana (?
            transform.forward = Vector3.Slerp(transform.forward, sight.dirToTarget, rotationLerpSpeed);

            agent.Move(sight.dirToTarget * MovementSpeed * Time.deltaTime);

            if (sight.distanceToTarget <= AttackRange)
                sm.Feed(BasicEnemyStates.attack);
        };
        pursue.OnExit += (nextState) => 
        { anims.SetBool("Walking", false); };

        attack.OnEnter += (previousState) => 
        {
            //print("Attack");
            agent.isStopped = true;
            StartCoroutine(SimpleAttack());
        };
        attack.OnUpdate += () => {};
        attack.OnExit += (nextState) => 
        {
            //print("Salió del ataque");
        };

        think.OnEnter += (previousState) => 
        {
            //print("Thinking");
            StartCoroutine(thinkAndWatch());
        };
        think.OnUpdate += () => { };
        think.OnExit += (nextState) => 
        {
            //print(string.Format("Exiting from Thinking, next State will be {0}", nextState.ToString()));
        };

        dead.OnEnter += (previousState) => 
        {
            StopAllCoroutines();
            anims.SetTrigger("Die");
            Die();
        }; 

        #endregion

        sm = new GenericFSM<BasicEnemyStates>(idle);
    }

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
        CurrentState = sm.currentState; 
#endif

        sight.Update();

        if (LookTowardsPlayer && _targetDetected)
            transform.forward = Vector3.Lerp(transform.forward, sight.dirToTarget, rotationLerpSpeed * Time.deltaTime);

        sm.Update();
    }

    public void SetVulnerabity(bool vulnerable)
    {
        isVulnerableToAttacks = vulnerable;
        StartCoroutine(vulnerableToAttacks());
        VulnerableMark.Play();
    }

    IEnumerator SimpleAttack()
    {
        var toDamage = sight.target.GetComponent<IKilleable>();

        if (!toDamage.IsAlive)
            sm.Feed(BasicEnemyStates.idle);

        //Inicio el primer ataque.
        LookTowardsPlayer = false;
        anims.SetTrigger("SimpleAttack");
        yield return null;

        float currentTransitionTime = getCurrentTransitionDuration();
        yield return new WaitForSeconds(currentTransitionTime);

        float remainingTime = getRemainingAnimTime();
        yield return new WaitForSeconds(remainingTime);

        LookTowardsPlayer = true;
        sm.Feed(BasicEnemyStates.think);
    }

    IEnumerator Block()
    {
        LookTowardsPlayer = true;
        yield return null;

        float currentTransitionTime = getCurrentTransitionDuration();
        yield return new WaitForSeconds(currentTransitionTime);

        float remainingTime = getRemainingAnimTime();
        yield return new WaitForSeconds(remainingTime);
        LookTowardsPlayer = true;

        sm.Feed(BasicEnemyStates.attack);
    }

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
            transform.forward = Vector3.Slerp(transform.forward, sight.dirToTarget, rotationLerpSpeed);

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
