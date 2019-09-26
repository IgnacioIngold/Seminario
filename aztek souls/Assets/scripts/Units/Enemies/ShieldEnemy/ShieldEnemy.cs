using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IA.StateMachine.Generic;
using Core.Entities;
using System;
using Random = UnityEngine.Random;
using IA.RandomSelections;

public enum ShieldEnemyStates
{
    idle,
    alerted,
    block,
    vulnerable,
    reposition,
    parry,
    attack,
    pursue,
    think,
    dead
}

public class ShieldEnemy : BaseUnit
{
    public event Action onBlockedHit = delegate { };

    [Header("Parry")]
    public float BlockRange = 3f;
    public float ParryCoolDown = 3f;
    private bool _canParry = true;

    public float AlertedTime = 2f;
    public float AlertRadius = 10f;

    GenericFSM<ShieldEnemyStates> _sm;
    private float _alertedTimeRemaining = 0f;
    private float _originalRotLerpSpeed = 0f;


    private bool _attacking;
    private bool LookTowardsPlayer = true;
    private float ThinkTime = 0;
    private float remainingThinkTime = 0;
    private bool _blocking = false;

#if(UNITY_EDITOR)
    [SerializeField] ShieldEnemyStates current;
#endif


    //======================== OVERRIDES & INTERFACES =========================================

    public override void GetDamage(params object[] DamageStats)
    {
        IAttacker<object[]> Aggresor = (IAttacker<object[]>)DamageStats[0];
        float damage = (float)DamageStats[1];

        if (damage > 0)
        {
            StopCoroutine(TriCombo());
            bool breakDefenceAttack = (bool)DamageStats[2];

            if (_blocking && breakDefenceAttack)
            {
                _sm.Feed(ShieldEnemyStates.vulnerable);
                return;
            }

            //Confirmar hit o no.
            if (_blocking && sight.angleToTarget < 80)
            {
                Aggresor.OnHitBlocked(null);
                onBlockedHit();

                if (_canParry)
                    _sm.Feed(ShieldEnemyStates.parry);
                else
                    _sm.Feed(ShieldEnemyStates.think);
            }
            else
            {
                anims.SetTrigger("getDamage");

                Aggresor.OnHitConfirmed(new object[] { BloodPerHit });
                //Si no estoy guardando.
                Health -= (float)DamageStats[1];

                base.GetDamage(DamageStats);

                //Aviso que estoy Muerto We.
                if (!IsAlive)
                {
                    Aggresor.OnKillConfirmed(new object[] { BloodForKill });
                    _sm.Feed(ShieldEnemyStates.dead);
                    return;
                }

                if (!_targetDetected)
                {
                    _targetDetected = true;
                    _sm.Feed(ShieldEnemyStates.alerted);
                }
            }
        }
    }

    public override object[] GetDamageStats()
    {
        return new object[3] { this, attackDamage, false };
    }

    //=========================================================================================

    protected override void Awake()
    {
        base.Awake();

        //State Machine.
        var idle = new State<ShieldEnemyStates>("Idle");
        var alerted = new State<ShieldEnemyStates>("Alerted");
        var pursue = new State<ShieldEnemyStates>("pursue");
        var blocking = new State<ShieldEnemyStates>("Bloquing");
        var vulnerable = new State<ShieldEnemyStates>("Vulnerable");
        var reposition = new State<ShieldEnemyStates>("Repositioning");
        var parry = new State<ShieldEnemyStates>("Parrying");
        var attack = new State<ShieldEnemyStates>("Attacking");
        var think = new State<ShieldEnemyStates>("Thinking");
        var dead = new State<ShieldEnemyStates>("Dead");

        /*
         * .OnEnter += (previousState) => { };
         * .OnUpdate += () => { };
         * .OnExit += (nextState) => { };
        */

        /*
            anims.SetBool("Dead", true);
            anims.SetTrigger("getDamage");
            anims.SetFloat("Moving", 1f);
            anims.SetInteger("Attack", 1);
            anims.SetBool("Blocking", true);
            anims.SetTrigger("BlockBreak");
            anims.SetTrigger("Parry");
        */
        #region Transitions

        idle.AddTransition(ShieldEnemyStates.alerted, alerted)
            .AddTransition(ShieldEnemyStates.dead, dead);

        alerted.AddTransition(ShieldEnemyStates.think, think)
               .AddTransition(ShieldEnemyStates.dead, dead);

        pursue.AddTransition(ShieldEnemyStates.pursue, pursue)
              .AddTransition(ShieldEnemyStates.think, think)
              .AddTransition(ShieldEnemyStates.dead, dead);

        reposition.AddTransition(ShieldEnemyStates.dead, dead)
                  .AddTransition(ShieldEnemyStates.think, think);

        blocking.AddTransition(ShieldEnemyStates.parry, parry)
                .AddTransition(ShieldEnemyStates.attack, attack)
                .AddTransition(ShieldEnemyStates.vulnerable, vulnerable)
                .AddTransition(ShieldEnemyStates.think, think)
                .AddTransition(ShieldEnemyStates.dead, dead);

        vulnerable.AddTransition(ShieldEnemyStates.think, think)
                  .AddTransition(ShieldEnemyStates.dead, dead);

        parry.AddTransition(ShieldEnemyStates.think, think)
             .AddTransition(ShieldEnemyStates.dead, dead);

        attack.AddTransition(ShieldEnemyStates.dead, dead)
              .AddTransition(ShieldEnemyStates.think, think);

        think.AddTransition(ShieldEnemyStates.dead, dead)
             .AddTransition(ShieldEnemyStates.block, blocking)
             .AddTransition(ShieldEnemyStates.parry, parry)
             .AddTransition(ShieldEnemyStates.vulnerable, vulnerable)
             .AddTransition(ShieldEnemyStates.reposition, reposition)
             .AddTransition(ShieldEnemyStates.attack, attack)
             .AddTransition(ShieldEnemyStates.pursue, pursue)
             .AddTransition(ShieldEnemyStates.idle, idle);

        #endregion

        #region Estados

        idle.OnEnter += (previousState) => 
        {
            anims.SetFloat("Moving", 0f);
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
        //idle.OnExit += (nextState) => { };


        alerted.OnEnter += (previousState) => 
        {
            _alertedTimeRemaining = AlertedTime;
        };
        alerted.OnUpdate += () => 
        {
            if (_alertedTimeRemaining >= 0)
            {
                _alertedTimeRemaining -= Time.deltaTime;
                transform.forward = Vector3.Slerp(transform.forward, sight.dirToTarget, rotationLerpSpeed);
            }
            else
                _sm.Feed(ShieldEnemyStates.think);
        };
        alerted.OnExit += (nextState) => { };


        blocking.OnEnter += (previousState) => 
        {
            anims.SetBool("Blocking", true);
            LookTowardsPlayer = true;
            rotationLerpSpeed *= 3;
            _blocking = true;
        };
        blocking.OnUpdate += () => 
        {
            if (sight.distanceToTarget > BlockRange) _sm.Feed(ShieldEnemyStates.think);
        };
        blocking.OnExit += (nextState) => 
        {
            anims.SetBool("Blocking", false);
            rotationLerpSpeed /= 3;
            _blocking = false;
        };

        vulnerable.OnEnter += (previousState) => { StartCoroutine(defenceDestroyed()); };
        //vulnerable.OnUpdate += () => { };
        //vulnerable.OnExit += (nextState) => { };


        parry.OnEnter += (previousState) => 
        {
            StopAllCoroutines();
            StartCoroutine(parryBicombo());
        };
        //parry.OnUpdate += () => { };
        parry.OnExit += (nextState) => { };

        pursue.OnEnter += (previousState) => 
        {
            anims.SetFloat("Moving", 1f);
        };
        pursue.OnUpdate += () => 
        {
            //Correr como si no hubiera un mañana (?
            transform.forward = Vector3.Slerp(transform.forward, sight.dirToTarget, rotationLerpSpeed);

            if (sight.distanceToTarget > AttackRange) agent.Move(sight.dirToTarget * MovementSpeed * Time.deltaTime);

            if (sight.distanceToTarget <= AttackRange) _sm.Feed(ShieldEnemyStates.think);
        };
        //pursue.OnExit += (nextState) => 
        //{
        //    //Desactivo la animación. Esto queda así para que otra animación lo sobreescriba.
        //};

        reposition.OnEnter += (previousState) => 
        {
            LookTowardsPlayer = true;
            _originalRotLerpSpeed = rotationLerpSpeed;
            rotationLerpSpeed *= ((sight.angleToTarget / 360) * 20);
        };
        reposition.OnUpdate += () =>
        {
            if (sight.angleToTarget < 45) _sm.Feed(ShieldEnemyStates.think);
        };
        reposition.OnExit += (nextState) => 
        {
            LookTowardsPlayer = false;
            rotationLerpSpeed = _originalRotLerpSpeed;
        };

        attack.OnEnter += (previousState) => 
        {
            //_attacking = true;
            agent.isStopped = true;
            rb.velocity = Vector3.zero;
            StartCoroutine(TriCombo());
        };
        //attack.OnUpdate += () => { };
        attack.OnExit += (nextState) => 
        {
            //_attacking = false;
            agent.isStopped = false;
        };

        think.OnEnter += (previousState) => 
        {
            if (ThinkTime > 0) remainingThinkTime = ThinkTime;
        };
        think.OnUpdate += () => 
        {
            if (remainingThinkTime > 0)
                remainingThinkTime -= Time.deltaTime;
            else
            {
                remainingThinkTime = 0;
                IKilleable target = Target.GetComponent<IKilleable>();

                if (sight.angleToTarget > 45f)
                {
                    _sm.Feed(ShieldEnemyStates.reposition);
                    return;
                }

                if (target.IsAlive)
                {
                    //Tomo una desición.
                    float blockImportance = (1 - (Health / MaxHP)) * 10;
                    float pursueImportance = ((Health / MaxHP) * 10);
                    float AttackImportance = 10f - (blockImportance * 0.8f);

                    print(string.Format("BlockImp: {0} / PursueImp: {1} / AttackImportance: {2}",
                        blockImportance, pursueImportance, AttackImportance));

                    //Desiciones posibles:
                    //Perseguir --> se realiza siempre que el enemigo esté más lejos de cierta distancia.
                    //Atacar --> Su importancia se mantiene.
                    //Bloquear --> Cuando la vida se va reduciendo su imporancia es mayor.

                    if (sight.distanceToTarget > AttackRange)
                    {
                        int decition = RoulleteSelection.Roll(new float[2] { pursueImportance, blockImportance });
                        print("Distance is bigger than the AttackRange.\nDecition was: " + (decition == 0 ? "pursue" : "block"));

                        if (decition == 0) _sm.Feed(ShieldEnemyStates.pursue);
                        if (decition == 1) _sm.Feed(ShieldEnemyStates.block);
                    }

                    if (sight.distanceToTarget < AttackRange)
                    {
                        int decition = RoulleteSelection.Roll(new float[2] { AttackImportance, blockImportance });
                        print("Distance is smaller than the AttackRange.\nDecition was: " + (decition == 0 ? "Attack" : "block"));

                        if (decition == 0) _sm.Feed(ShieldEnemyStates.attack);
                        if (decition == 1) _sm.Feed(ShieldEnemyStates.block);
                    }
                }
            }
        };
        think.OnExit += (nextState) => 
        {
            print(string.Format("Exiting from Thinking, next State will be {0}", nextState.ToString()));
        };

        dead.OnEnter += (previousState) => 
        {
            anims.SetBool("Dead", true);
            Die();
        };

        #endregion

        _sm = new GenericFSM<ShieldEnemyStates>(idle);
    }

    // Update is called once per frame
    void Update()
    {
        print("Current State is: " + _sm.current.StateName);

        if (IsAlive)
        {
            sight.Update();

            if (LookTowardsPlayer && _targetDetected)
                transform.forward = Vector3.Lerp(transform.forward, sight.dirToTarget, rotationLerpSpeed * Time.deltaTime);

            _sm.Update();
        }
    }

    IEnumerator StartParryCoolDown()
    {
        _canParry = false;
        yield return new WaitForSeconds(ParryCoolDown);
        _canParry = true;
    }

    IEnumerator defenceDestroyed()
    {
        _blocking = false;
        anims.SetTrigger("BlockBreak");
        yield return null;
        float currentTransitionTime = getCurrentTransitionScaledTime();
        yield return new WaitForSeconds(currentTransitionTime);
        float remainingTime = getRemainingAnimTime("disamed", currentTransitionTime);
        yield return new WaitForSeconds(remainingTime);

        _sm.Feed(ShieldEnemyStates.think);
    }

    IEnumerator parryBicombo()
    {
        anims.SetTrigger("Parry");
        LookTowardsPlayer = false;
        float currentTransitionTime = getCurrentTransitionScaledTime();
        print("Transicion es: " + currentTransitionTime);
        yield return new WaitForSeconds(currentTransitionTime);
        float remainingTime = getRemainingAnimTime(" parry", currentTransitionTime);

        anims.SetInteger("Attack", 2);                   //  <---- Primer Ataque
        yield return new WaitForSeconds(remainingTime);
        currentTransitionTime = getCurrentTransitionScaledTime();
        yield return new WaitForSeconds(currentTransitionTime);

        remainingTime = getRemainingAnimTime("attack2", currentTransitionTime);
        anims.SetInteger("Attack", 3);
        yield return new WaitForSeconds(remainingTime);

        //Segundo ataque.
        currentTransitionTime = getCurrentTransitionScaledTime();
        yield return new WaitForSeconds(currentTransitionTime);

        remainingTime = getRemainingAnimTime("attack3", currentTransitionTime);
        yield return new WaitForSeconds(remainingTime);

        anims.SetBool("Parrying", false);
        _sm.Feed(ShieldEnemyStates.think);
    }

    IEnumerator TriCombo()
    {
        anims.SetFloat("Moving", 0f);
        LookTowardsPlayer = false;

        //Inicio el primer ataque.
        anims.SetInteger("Attack", 1);
        yield return null;

        float currentTransitionTime = getCurrentTransitionScaledTime();
        yield return new WaitForSeconds(currentTransitionTime);

        float remainingTime = getRemainingAnimTime("attack1", currentTransitionTime);
        anims.SetInteger("Attack", 2);
        yield return new WaitForSeconds(remainingTime);

        //Segundo ataque.
        currentTransitionTime = getCurrentTransitionScaledTime();
        yield return new WaitForSeconds(currentTransitionTime);

        remainingTime = getRemainingAnimTime("attack2", currentTransitionTime);
        anims.SetInteger("Attack", 3);
        yield return new WaitForSeconds(remainingTime);

        //Tercer ataque.
        currentTransitionTime = getCurrentTransitionScaledTime();
        yield return new WaitForSeconds(currentTransitionTime);

        remainingTime = getRemainingAnimTime("attack3", currentTransitionTime);
        yield return new WaitForSeconds(remainingTime);

        _sm.Feed(ShieldEnemyStates.think);
    }

#if(UNITY_EDITOR)
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Gizmos.color = Color.cyan;
        Gizmos.matrix *= Matrix4x4.Scale(new Vector3(1, 0, 1));
        Gizmos.DrawWireSphere(transform.position, BlockRange);
    }
#endif
}
