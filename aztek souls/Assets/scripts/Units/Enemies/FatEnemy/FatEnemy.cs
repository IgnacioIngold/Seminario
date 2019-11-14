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
    hurted,
    dead
}

/*
 * En la sección de propiedades podemos encontrar uno x cada parámetro del animator.
*/

public class FatEnemy : BaseUnit
{
    public GenericFSM<FatEnemyStates> _sm;

    [Header("Estado de Alerta")]
    public float AlertTime;
    private float _alertedTimeRemainig;

    [Header("Rangos de Ataque")]
    public float rangeAttack_MaxRange;
    public float meleeAttack_MaxRange;

    [Header("Ataque Melee")]
    public float MeleeAttackDuration;
    public float MeleeAttackCooldown;
    private float _remainingMeleeAttackTime;

    [Header("Ataque de Rango")]
    public GameObject bullet;
    public Transform bulletParent;
    public float RangeAttackDuration;
    public float RangeAttackCooldown;
    private float _remaingRangeAttackTime;

    [Header("Explode")]
    public LayerMask DamageableMask;
    public GameObject explodeParticle;
    public GameObject explodeRangeParticle;
    public Transform explotionLocator;
    public SkinnedMeshRenderer rend;
    public float explotionDamage;
    public float ExplotionRange;
    public float explotionForce;
    public float TimeToExplote;
    public float minExplodeVel;
    public float maxExplodeVel;
    private float _remainingTimeToExplote;

    private float _thinkTime; //Tiempo de pensadar.

    int[] animationParams;

    //============================= DEBUGG GIZMOS ===============================================

#if UNITY_EDITOR
    [Header("Debugg FatEnemy")]
    public FatEnemyStates currentState;
    public bool Debug_AttackRanges; 
    
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        if (Debug_AttackRanges)
        {
            Gizmos.color = Color.blue;
            Gizmos.matrix *= Matrix4x4.Scale(new Vector3(1, 0, 1));
            Gizmos.DrawWireSphere(transform.position, meleeAttack_MaxRange);

            Gizmos.color = Color.red;
            Gizmos.matrix *= Matrix4x4.Scale(new Vector3(1, 0, 1));
            Gizmos.DrawWireSphere(transform.position, rangeAttack_MaxRange);

            Gizmos.color = Color.yellow;
            Gizmos.matrix *= Matrix4x4.Scale(new Vector3(1, 0, 1));
            Gizmos.DrawWireSphere(transform.position, ExplotionRange);
        }
    } 
#endif
    //============================== PROPIEDADES ==============================================

    public float Locomotion
    {
        get => anims.GetFloat(animationParams[0]);
        set => anims.SetFloat(animationParams[0], value);
    }
    public bool IsDead
    {
        get => anims.GetBool(animationParams[1]);
        set => anims.SetBool(animationParams[1], value);
    }
    public int Combat
    {
        get => anims.GetInteger(animationParams[2]);
        set => anims.SetInteger(animationParams[2], value);
    }
    public bool IsAlerted
    {
        get => anims.GetBool(animationParams[3]);
        set => anims.SetBool(animationParams[3], value);
    }

    //Funciona como un Trigger.
    public void Alert()
    {
        StartCoroutine(SetAlertedTrigger());
    }

    //============================== INTERFACES ===============================================

    /// <summary>
    /// Retorna las estadísticas de combate de esta Unidad.
    /// </summary>
    /// <returns></returns>
    public override HitData DamageStats()
    {
        return new HitData() { Damage = attackDamage, AttackType = Inputs.light, BreakDefence = false };
    }
    /// <summary>
    /// Se llama cuando este enemigo recibe un Hit.
    /// </summary>
    /// <param name="EntryData"></param>
    /// <returns></returns>
    public override HitResult Hit(HitData EntryData)
    {
        HitResult result = HitResult.Default();
        EnemyHealthBar.FadeIn(); //Hacemos aparecer su barra de vida.

        //Particula de feedback del golpe
        var particle = Instantiate(OnHitParticle, transform.position, Quaternion.identity);
        Destroy(particle, 3f);

        var vulnerability = GetCurrentVulnerability();

        if (vulnerability == EntryData.AttackType)
        {
            _attacksRecieved++;
            VulnerableMarker.gameObject.SetActive(true);
            ButtonHitConfirm.Play();
        }

        //Completar
        Health -= EntryData.Damage;

        //Chequeamos el combow?

        if (!IsAlive)
        {
            result.TargetEliminated = true;
            //Si completo el combo...

            _sm.Feed(FatEnemyStates.dead);
        }

        return result;
    }
    /// <summary>
    /// Se llama cuando este Enemigo ejecuta un ataque.
    /// </summary>
    /// <param name="result"></param>
    public override void GetHitResult(HitResult result)
    {
        //Este enemigo en particular no hace mucho.
    }

    //============================ UNITY FUNCTIONS =============================================

    protected override void Awake()
    {
        base.Awake();

        //Combos a los que es vulnerable.
        vulnerabilityCombos.Add(0, new Inputs[3] { Inputs.light, Inputs.light, Inputs.strong });

        animationParams = new int[4];
        for (int i = 0; i < animationParams.Length; i++)
            animationParams[i] = anims.GetParameter(i).nameHash;

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

        meleeAttack.AddTransition(FatEnemyStates.dead, dead)
                   .AddTransition(FatEnemyStates.think, think);

        rangeAttack.AddTransition(FatEnemyStates.think, think)
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
            Locomotion = 0;
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
            Alert();
            _targetDetected = true;
            LookTowardsPlayer = true;
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

        pursue.OnEnter += (previousState) => { Locomotion = 1; };
        pursue.OnUpdate += () => 
        {
            LookTowardsPlayer = true;

            if (sight.distanceToTarget > AttackRange) agent.Move(sight.dirToTarget * MovementSpeed * Time.deltaTime);

            if (sight.distanceToTarget <= AttackRange) _sm.Feed(FatEnemyStates.think);
        };
        pursue.OnExit += (nextState) => 
        {
            if (nextState == FatEnemyStates.idle) Locomotion = 0;
        };

        think.OnEnter += (previousState) => { Locomotion = 0; };
        think.OnUpdate += () => 
        {
            if (_thinkTime > 0) _thinkTime -= Time.deltaTime;
            else
            {
                var target = Target.GetComponent<IKilleable>();
                float targetDistance = sight.distanceToTarget;

                if (!target.IsAlive) _sm.Feed(FatEnemyStates.idle);

                if (targetDistance <= meleeAttack_MaxRange)
                    _sm.Feed(FatEnemyStates.meleeAttack);
                else if (targetDistance <= rangeAttack_MaxRange && targetDistance > meleeAttack_MaxRange)
                    _sm.Feed(FatEnemyStates.rangeAttack);
                else if (targetDistance > rangeAttack_MaxRange)
                    _sm.Feed(FatEnemyStates.pursue);
            }
        };
        think.OnExit += (nextState) => { };

        rangeAttack.OnEnter += (previousState) => 
        {
            agent.isStopped = true;
            LookTowardsPlayer = true;
            _remaingRangeAttackTime = 0;
        };
        rangeAttack.OnUpdate += () => 
        {
            //Atacamos varias veces segun el tiempo pase.
            _remaingRangeAttackTime -= Time.deltaTime;
            if (_remaingRangeAttackTime <= 0)
            {
                Combat = 2;

                //Reseteamos el tiempo para el siguiente ataque.
                _remaingRangeAttackTime = RangeAttackDuration + RangeAttackCooldown;
            }

            if (sight.distanceToTarget <= meleeAttack_MaxRange)
                _sm.Feed(FatEnemyStates.think);
        };
        rangeAttack.OnExit += (nextState) => 
        {
            Combat = 0;
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
                Combat = 1;
                LookTowardsPlayer = true;
                _remainingMeleeAttackTime = MeleeAttackDuration + MeleeAttackCooldown;
            }
        };
        meleeAttack.OnExit += (nextState) => 
        {
            Combat = 0;
            agent.isStopped = false;
        };

        dead.OnEnter += (previousState) => 
        {
            IsDead = true;
            Die();
        };

        _sm = new GenericFSM<FatEnemyStates>(idle);

        #endregion

    }
    void Update()
    {
        //Condición de muerte, Update de Sight, FSM y Rotación.
        if (IsAlive)
        {
            sight.Update();

            if (LookTowardsPlayer && _targetDetected)
                transform.forward = Vector3.Lerp(transform.forward, sight.dirToTarget, _rotationLerpSpeed);

            _sm.Update();
        }
        else
            _sm.Feed(FatEnemyStates.dead);

#if UNITY_EDITOR
        currentState = _sm.currentState; 
#endif
    }

    //=============================== MEMBER FUNCS ============================================

    //public void SetHiteableRange(bool inRange)
    //{
    //    //Con esto le avisamos a la entidad que debe mostrar su vulnerabilidad.
    //    throw new System.NotImplementedException();
    //}

    //================================ OVERRIDES ==============================================

    protected override void Die()
    {
        EnemyHealthBar.FadeOut(3f);
        agent.enabled = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        rb.isKinematic = true;
        MainColl.enabled = false;

        OnDie();

        StartCoroutine(Explode());
    }

    //=============================== ANIMATION EVENTS ==========================================

    public void Shoot()
    {
        //Instanciamos un projectil.
        var projectil = Instantiate(bullet);
        projectil.transform.position = bulletParent.position;
        projectil.GetComponentInChildren<Bullet>().SetOwner(gameObject);

        Vector3 targetpos = Target.position + Vector3.up;
        Vector3 bulletDir = (targetpos - bulletParent.position).normalized;

        projectil.transform.forward = bulletDir;
    }

    //=============================== CORRUTINES ================================================

    IEnumerator SetAlertedTrigger()
    {
        anims.SetBool(animationParams[3], true);
        yield return new WaitForEndOfFrame();
        anims.SetBool(animationParams[3], false);
    }
    IEnumerator Explode()
    {
        //Obtengo el material.
        var mat = rend.materials[2];
        explodeRangeParticle.SetActive(true);

        //Seteo el parámetro para que aparezca como que va a explotar.
        mat.SetFloat("_life", 0);
        _remainingTimeToExplote = TimeToExplote;

        //Voy aumentando la velocidad del efecto gradualmente.
        while (_remainingTimeToExplote > 0)
        {
            _remainingTimeToExplote -= Time.deltaTime;

            float transcurred = (TimeToExplote - _remainingTimeToExplote) / TimeToExplote;
            float Speed = Mathf.Lerp(minExplodeVel, maxExplodeVel, transcurred);
            mat.SetFloat("_TickSpeed", Speed);

            yield return null;
        }


        //Aplico daño a todos los entes que esten dentro de un rango.
        Collider[] result = Physics.OverlapSphere(transform.position, ExplotionRange, DamageableMask);
        foreach (var target in result)
        {
            var damageable = target.GetComponentInParent<IDamageable<HitData, HitResult>>();

            if (damageable != null)
            {
                //Saco la dirección al objetivo.
                Vector3 dir = (transform.position - target.transform.position).normalized;
                dir.y = 0;  //Elimino el eje y.

                float DistanceToTarget = Vector3.Distance(transform.position, target.transform.position);
                float maximDistance = (ExplotionRange - DistanceToTarget) / DistanceToTarget;

                //Aplico una fuerza.
                target.GetComponentInParent<Rigidbody>().AddForce((dir * explotionForce) * maximDistance, ForceMode.Impulse);

                //Aplico un Daño.
                damageable.Hit(new HitData() { Damage = (explotionDamage * maximDistance), BreakDefence = true, AttackType = Inputs.strong });
            }
        }

        //Instancio la particula.
        var explotionParticle = Instantiate(explodeParticle);
        explotionParticle.transform.position = explotionLocator.position;
        Destroy(explotionParticle, 4f);

        //Destruyo Este GameObject
        Destroy(gameObject);
    }
}

