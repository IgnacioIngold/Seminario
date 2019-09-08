using System.Collections;
using System.Linq;
using UnityEngine;
using IA.StateMachine.Generic;
using Core.Entities;
using System;
using Random = UnityEngine.Random;

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
    public BasicEnemyStates MainState = BasicEnemyStates.idle;
    public Collider AttackCollider;

    public float AlertedTime = 2f;
    public float AlertRadius = 10f;

    GenericFSM<BasicEnemyStates> sm;
    Vector3 _lastEnemyPositionKnow = Vector3.zero;
    bool Attacking = false;

    private float _alertedTimeRemaining = 0f;

    private Animator _anims;

    //======================== OVERRIDES & INTERFACES =========================================

    public override void GetDamage(params object[] DamageStats)
    {
        anims.SetTrigger("GetHit");
        StopAllCoroutines();

        Health -= (float)DamageStats[0];

        base.GetDamage(DamageStats);

        if (!IsAlive)
        {
            sm.Feed(BasicEnemyStates.dead);
            return;
        }

        if (!_targetDetected)
        {
            _targetDetected = true;
            sm.Feed(BasicEnemyStates.pursue);
        }
        else
            sm.Feed(BasicEnemyStates.idle);
    }

    public override object[] GetDamageStats()
    {
        return base.GetDamageStats();
    }

    //=========================================================================================

    protected override void Awake()
    {
        base.Awake();
        _anims = GetComponentInChildren<Animator>();

        //State Machine
        var idle = new State<BasicEnemyStates>("Idle");
        var alerted = new State<BasicEnemyStates>("Alerted");
        var pursue = new State<BasicEnemyStates>("pursue");
        var attack = new State<BasicEnemyStates>("Attacking");
        var think = new State<BasicEnemyStates>("Thinking");
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
              .AddTransition(BasicEnemyStates.idle, idle)
              .AddTransition(BasicEnemyStates.think, think);


        think.AddTransition(BasicEnemyStates.dead, dead)
             .AddTransition(BasicEnemyStates.pursue, pursue)
             .AddTransition(BasicEnemyStates.idle, idle)
             .AddTransition(BasicEnemyStates.attack, attack);

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
            _alertedTimeRemaining = AlertedTime;

            EnemyHealthBar.FadeIn();

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
            print("pursue");
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
            print("Attack");
            Attacking = true;
            agent.isStopped = true;
            StartCoroutine(SimpleAttack());
        };
        attack.OnUpdate += () => {};
        attack.OnExit += (nextState) => 
        {
            Attacking = false;
            print("Salió del ataque");
        };

        think.OnEnter += (previousState) => 
        {
            print("Thinking");
            StartCoroutine(thinkAndWatch());
        };
        think.OnUpdate += () => { };
        think.OnExit += (nextState) => 
        {
            print(string.Format("Exiting from Thinking, next State will be {0}", nextState.ToString()));
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
        sight.Update();
        sm.Update();
    }

    IEnumerator SimpleAttack()
    {
        var toDamage = sight.target.GetComponent<IKilleable>();

        if (!toDamage.IsAlive)
            sm.Feed(BasicEnemyStates.idle);

        while(facingTowardsPlayer() <= 0.9f)
        {
            transform.forward = Vector3.Slerp(transform.forward, sight.dirToTarget, rotationLerpSpeed);
            yield return null;
        }

        while (Attacking)
        {
            anims.SetTrigger("SimpleAttack");
            yield return null;

            var info = anims.GetAnimatorTransitionInfo(0);
            float transitionTIme = info.duration;
            //print("Transition = " + transitionTIme);
            yield return new WaitForSeconds(transitionTIme);

            bool knowHowMuchIsLeft = false;
            float remainingTime = 0;

            AnimatorClipInfo[] clipInfo = anims.GetCurrentAnimatorClipInfo(0);
            AnimationClip currentClip;
            if (clipInfo != null && clipInfo.Length > 0)
            {
                currentClip = clipInfo[0].clip;
                //print("Current Clip = " + currentClip.name);

                if (!knowHowMuchIsLeft && currentClip.name == "Attack")
                {
                    //print("currentClip is Correct!");
                    float length = currentClip.length;
                    //float normTime = anims.GetCurrentAnimatorStateInfo(0).normalizedTime;
                    float passed = length - (length * transitionTIme);
                    //print("TimePassed is = " + passed);
                    remainingTime = passed;
                    knowHowMuchIsLeft = true;
                }
                else
                    yield return null;

                if (knowHowMuchIsLeft)
                {
                    yield return new WaitForSeconds(remainingTime);
                    break;
                }
            }
        }

        sm.Feed(BasicEnemyStates.think);
    }

    IEnumerator thinkAndWatch()
    {
        var toDamage = sight.target.GetComponent<IKilleable>();
        float waitTime = Random.Range(0.5f, 1.5f);
        yield return new WaitForSeconds(waitTime);


        print("Watchtime Started");
        float watchTime = Random.Range(2f, 4f);
        while(watchTime > 0)
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

    float facingTowardsPlayer()
    {
        return Vector3.Dot(sight.dirToTarget, transform.forward);
    }
}
