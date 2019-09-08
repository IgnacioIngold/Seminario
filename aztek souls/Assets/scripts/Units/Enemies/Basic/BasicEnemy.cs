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
    think,
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

    private Animator _anims;
    private Transform _detectedTarget = null;


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

        if (_targetDetected)
            sm.Feed(BasicEnemyStates.pursue);
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
            {
                _targetDetected = true;
                _detectedTarget = sight.target;
            }

            if (_targetDetected)
                sm.Feed(BasicEnemyStates.alerted);
        };
        //idle.OnExit += (nextState) => { };

        alerted.OnEnter += (previousState) => 
        {
            print("Enemy has been Alerted");

            _viewDirection = (_detectedTarget.position - transform.position).normalized;

            _alertedTimeRemaining = AlertedTime;
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
            //Si entro en rango de ataque... pos lo ataco
            float targetDistance = Vector3.Distance(transform.position, _detectedTarget.position);
            print("Attack Range = " + AttackRange + " TargetDistance = " + targetDistance);

            if (targetDistance <= AttackRange)
            {
                print("TIME TO FUCKING KILL");
                sm.Feed(BasicEnemyStates.attack);
                return;
            }

            //Correr como si no hubiera un mañana (?
            _viewDirection = (_detectedTarget.position - transform.position).normalized;
            transform.forward = Vector3.Slerp(transform.forward, _viewDirection, rotationLerpSpeed);

            agent.Move(_viewDirection * MovementSpeed * Time.deltaTime);
        };
        pursue.OnExit += (nextState) => 
        { anims.SetBool("Walking", false); };

        attack.OnEnter += (previousState) => 
        {
            print("Attack");
            agent.isStopped = true;
            Attacking = true;
            StartCoroutine(SimpleAttack());
        };
        attack.OnUpdate += () => { };
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
            print("I´ll go to: " + nextState.ToString());
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
        sm.Update();
    }

    IEnumerator SimpleAttack()
    {
        var toDamage = sight.target.GetComponent<IKilleable>();
        print("Empezo la wea ctm");

        if (!toDamage.IsAlive)
            sm.Feed(BasicEnemyStates.idle);

        transform.forward = Vector3.Slerp(transform.forward, sight.dirToTarget, rotationLerpSpeed);

        while(Attacking)
        {
            anims.SetTrigger("SimpleAttack");
            yield return null;

            var info = anims.GetAnimatorTransitionInfo(0);
            float transitionTIme = info.duration;
            print("Transition = " + transitionTIme);
            yield return new WaitForSeconds(transitionTIme);

            bool knowHowMuchIsLeft = false;
            float remainingTime = 0;

            AnimatorClipInfo[] clipInfo = anims.GetCurrentAnimatorClipInfo(0);
            AnimationClip currentClip;
            if (clipInfo != null && clipInfo.Length > 0)
            {
                currentClip = clipInfo[0].clip;
                print("Current Clip = " + currentClip.name);

                if (!knowHowMuchIsLeft && currentClip.name == "Attack")
                {
                    print("currentClip is Correct!");
                    float length = currentClip.length;
                    //float normTime = anims.GetCurrentAnimatorStateInfo(0).normalizedTime;
                    float passed = length - (length * transitionTIme);
                    print("TimePassed is = " + passed);
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
        var toDamage = _detectedTarget.GetComponent<IKilleable>();
        float waitTime = Random.Range(2f, 4f);
        yield return new WaitForSeconds(waitTime);

        float watchTime = Random.Range(4f, 8f);
        while(watchTime > 0)
        {
            watchTime -= Time.deltaTime;
            transform.forward = Vector3.Slerp(transform.forward, sight.dirToTarget, rotationLerpSpeed);
        }

        if (toDamage.IsAlive && sight.distanceToTarget > AttackRange)
            sm.Feed(BasicEnemyStates.pursue);
    }

    private void OnGUI()
    {
        GUILayoutOption[] options = new GUILayoutOption[] { GUILayout.Width(110f) };
        GUI.color = Color.white;
        GUILayout.Label("Posición del enemigo",options);
    }

    private void OnDrawGizmos()
    {
        //Posición del enemigo.
    }
}
