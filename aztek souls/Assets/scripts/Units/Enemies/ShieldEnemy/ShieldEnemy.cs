using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IA.StateMachine.Generic;
using Core.Entities;
using Core;
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

public enum AttackStage
{
    StartUp,
    Active,
    Recovery
}

public class ShieldEnemy : BaseUnit
{
    public event Action onBlockedHit = delegate { };
    public event Action onGetHit = delegate { };

    [Header("Parry")]
    public ParticleSystem ShieldSparks;
    public float BlockRange = 3f;
    public float ParryCoolDown = 3f;
    private bool _canParry = true;

    public float AlertedTime = 2f;
    public float AlertRadius = 10f;

    [Header("Attack & Ritmo")]
    public ParticleSystem marker;
    public int CurrentAttackID = 0;
    public AttackStage CurrentStage;

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
    private bool _parrying;
#endif

    //======================== OVERRIDES & INTERFACES =========================================

    public override HitResult Hit(HitData HitInfo)
    {
        HitResult result = HitResult.Empty();

        if (HitInfo.Damage > 0 && IsAlive)
        {
            //Si el enemigo no me había detectado.
            if (!_targetDetected) _targetDetected = true;
            //TODO: chequear que el enemigo pase a Alerted despues de recibir el daño.

            if (_blocking) //Si estoy bloqueando...
            {
                if (sight.angleToTarget < 80 && !HitInfo.BreakDefence)
                {
                    result.HitBlocked = true;
                    onBlockedHit();

                    if (_canParry)
                        _sm.Feed(ShieldEnemyStates.parry);
                    else
                        _sm.Feed(ShieldEnemyStates.think);
                }
                else if(sight.angleToTarget > 80 || HitInfo.BreakDefence)
                    _sm.Feed(ShieldEnemyStates.vulnerable);
            }
            else //Si no estoy bloqueando...
            {
                anims.SetTrigger("getDamage");
                onGetHit();

                result.HitConnected = true;
                result.bloodEarned = BloodPerHit;

                //Si no estoy bloqueando.
                Health -= HitInfo.Damage;

                var particle = Instantiate(OnHitParticle, transform.position, Quaternion.identity);
                Destroy(particle, 3f);
                EnemyHealthBar.FadeIn();

                //Si mi vida es menor a 0...
                if (!IsAlive)
                {
                    result.TargetEliminated = true;    // Aviso que estoy muerto.
                    result.bloodEarned = BloodForKill; // Obtengo la recompenza.
                    _sm.Feed(ShieldEnemyStates.dead);  // Paso al estado de muerte.
                }
                else
                    _sm.Feed(ShieldEnemyStates.think); //Por default paso a think.
            }
        }
        return result;
    }

    public override void FeedHitResult(HitResult result)
    {
        print(string.Format("{0} ha conectado un ataque.", gameObject.name));
    }

    public override HitData GetDamageStats()
    {
        return new HitData() { Damage = attackDamage, BreakDefence = false };
    }

    //================================= Animation Events ======================================

    /// <summary>
    /// Permite obtener información del estado actual de la animación de combate.
    /// </summary>
    /// <param name="index"> Identificador de la animación de Ataque actual.</param>
    /// <param name="stage"> En que Stage de la animación de Combate está.</param>
    public void SetAttackState(int index, AttackStage stage)
    {
        CurrentAttackID = index;
        CurrentStage = stage;

        switch (stage)
        {
            case AttackStage.StartUp:
                LookTowardsPlayer = true;
                break;
            case AttackStage.Active:
                LookTowardsPlayer = false;
                break;
            case AttackStage.Recovery:
                LookTowardsPlayer = true;
                break;
            default:
                break;
        }
    }
    /// <summary>
    /// Permite avisar al animator cuando debe pasar al siguiente estado.
    /// </summary>
    /// <param name="nextAttack"></param>
    public void ChangeAttack(int nextAttack)
    {
        //Setea la siguiente animación de combate.
        anims.SetInteger("Attack", nextAttack);
    }
    /// <summary>
    /// Se llama cada vez que una animación de combate llega a su fín.
    /// </summary>
    /// <param name="index"></param>
    public void AttackEnded(int index)
    {
        //Si el ataque termino, y se trata del 3ero yendo para idle...
        if (index == 3)
            _sm.Feed(ShieldEnemyStates.think);
    }

    public void StartParryAttack()
    {
        anims.SetInteger("Attack", 2);
        anims.SetBool("Parrying", false); //Terminó la animación de parry, pero vamos al ataque.
    }
    /// <summary>
    /// Avisamos que estamos saliendo del estado de vulnerabilidad.
    /// </summary>
    public void EndVulnerableState()
    {
        _sm.Feed(ShieldEnemyStates.think);
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
            anims.SetBool("Parrying");
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

        vulnerable.OnEnter += (previousState) => 
        {
            _blocking = false;
            anims.SetTrigger("BlockBreak");
        };
        //vulnerable.OnUpdate += () => { };
        //vulnerable.OnExit += (nextState) => { };


        parry.OnEnter += (previousState) => 
        {
            _parrying = true;
            anims.SetTrigger("Parry");
        };
        //parry.OnUpdate += () => { };
        parry.OnExit += (nextState) => { _parrying = false; };

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
        //pursue.OnExit += (nextState) => { };

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
            _attacking = true;
            agent.isStopped = true;
            rb.velocity = Vector3.zero;
            anims.SetInteger("Attack", 1);
        };
        //attack.OnUpdate += () => { };
        attack.OnExit += (nextState) => 
        {
            _attacking = false;
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
#if UNITY_EDITOR
        current = _sm.currentState; 
#endif

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
