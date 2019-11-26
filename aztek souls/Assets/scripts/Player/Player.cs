using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Core;
using Core.Entities;
using UnityEngine.Playables;

[Serializable]
public struct Stats
{
    /// <summary>
    /// Cada nivel garantiza un punto extra que puede gastarse en adquirir más puntos de las demás stats.
    /// </summary>
    public int Nivel;
    [Tooltip("Influencia el daño generado.")]
    /// <summary>
    /// Influencia el daño generado.
    /// </summary>
    public int Fuerza;
    [Tooltip("Sangre acumulada")]
    /// <summary>
    /// Sangre acumulada. Se puede intercambiar por puntos para subir de nivel o por puntos de vida.
    /// Si un enemigo elimina al jugador, este acumulará toda la sangre que se haya conseguido hasta ese punto.
    /// </summary>
    public float Sangre;
    [Tooltip("Influencia la cantidad total de vida.")]
    /// <summary>
    /// Influencia la cantidad total de vida.
    /// </summary>
    public int Vitalidad;
    [Tooltip("Influencia la velocidad del roll")]
    /// <summary>
    /// Influencia la velocidad del roll
    /// </summary>
    public int Agilidad;
    [Tooltip("Influencia la mitigación de daño recibido.")]
    /// <summary>
    /// Influencia la mitigación de daño recibido.
    /// </summary>
    public int Resistencia;

    /// <summary>
    /// Esto es temporal, y va a volar a futuro.
    /// </summary>
    public int bloodForLevelUp;
}

//[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour, IDamageable<HitData, HitResult>, IKilleable
{
    #region Eventos

    public event Action OnDie = delegate { };
    public event Action OnGetHit = delegate { };
    public event Action OnAttackLanded = delegate { };
    public event Action OnActionHasEnded = delegate { };
    public event Action OnStaminaIsEmpty = delegate { };
    public event Action OnFeastBlood = delegate { };
    public event Action OnConsumeBlood = delegate { };
    public event Action OnHealthHitMax = delegate { };

    #endregion

    #region Variables de Inspector
    [SerializeField] HealthBar _myBars = null;                                    // Display de la vida y la estamina del jugador.
    [SerializeField] Transform AxisOrientation;                            // Transform que determina la orientación del jugador.
    [SerializeField] LayerMask floor;                                      // Máscara de colisión para el piso.
    [SerializeField] GameObject marker = null;                                    // Índicador de ventana de Input.
    [SerializeField] GameObject OnHitParticle;                             // Particula a instanciar al recibir daño.
    [SerializeField] ParticleSystem RollParticle = null;                          // Partícula de Roll.
    [SerializeField] ParticleSystem.EmissionModule rollparticleEmission;   // Módulo de Emisión de la particula de roll.
    [SerializeField] ParticleSystem FeastBlood = null;                            // Particula que se reproduce al cargar vida.
    [SerializeField] Collider HitCollider;                                 // Collider de daño.
    [SerializeField] PlayableDirector StaminaEffect = null;                       // Reproduce el Efecto/Aviso de falta de Stamina.
    public PlayableDirector CameraShake;                         // Reproduce un Shake de la cámara.

    Rigidbody _rb;                                          // Componente Rigidbody.
    Animator _anims;                                        // Componente Animator. 
    #endregion

    #region Orientación
    public Transform stairOrientation;

    Vector3 _dir = Vector3.zero;                            // Dirección a la que el jugador debe mirar (Forward).
    Vector3 _rollDir = Vector3.zero;                        // Dirección a la que el jugador debe mirar al hacer un roll.
    Vector3 moveDiR = Vector3.zero;                         // Dirección real utilizada para hacer el movimiento. 
    #endregion


    [Header("Main Stats")] //Estados Principales.
    public Stats myStats;
    #region Health.
    public float BaseHP = 100f;                               // Máxima cantidad de vida posible del jugador.
    /// <summary>
    /// Controla el Display de la vida.
    /// </summary>
    public float Health
    {
        get
        {
            return _hp;
        }
        set
        {
            float val = value;
            if (val < 0) val = 0;
            else
            if (val > MaxHealth)
            {
                OnHealthHitMax();
                _canHeal = false;
                val = MaxHealth;
            }
            else
                _canHeal = true;

            _hp = val;

            if (_myBars != null)
                _myBars.UpdateHeathBar(_hp, MaxHealth);
        }
    }
    public float MaxHealth
    {
        get { return BaseHP + (myStats.Vitalidad * 5); }
    }
    float _hp = 100f;                                        // PRIVADO: valor actual de la vida. 
    #endregion
    #region Stamina.
    /// <summary>
    /// Controla el Display de la Estamina.
    /// </summary>
    public float Stamina
    {
        get { return _st; }
        set
        {
            _st = value;
            if (_st < 0)
            {
                _st = 0;
                //StartCoroutine(exhausted());
                OnStaminaIsEmpty();
            }
            _st = value;

            //Display Value
            if (_myBars != null)
                _myBars.UpdateStamina(_st, MaxStamina);
        }
    }
    public float MaxStamina = 100f;                          // Estamina máxima del jugador.
    public float StaminaRegeneration = 2f;                   // Regeneración por segundo de estamina.
    public float StRecoverDelay = 0.8f;                      // Delay de Regeneración de estamina luego de ejectuar una acción.

    float _st = 100f;                                        // PRIVADO: valor actual de la estamina.
    bool _recoverStamina = true;                             // Verdadero cuando se pierde estamina. 
    #endregion

    #region Walking
    [Header("Walking")]
    public float walkSpeed = 4f;                             // Velocidad de movimiento del jugador al caminar.
    public float rotationLerpSpeed = 0.1f; 
    #endregion
    #region Run
    [Header("Running")]
    public float runSpeed = 20f;                             // Velocidad de movimiento del jugador al correr.
    public float runCost = 20;                               // Costo por segundo de la acción correr.
    bool _running = false;                                   // PRIVADO: si el jugador esta corriendo actualmente. 
    #endregion
    #region Roll
    public float rollSpeed = 30f;                            // Velocidad de desplazamiento mientras hago el roll.
    public float rollDuration = 0.8f;                        // Duración del Roll.
    public float rollCost = 20f;                             // Costo del roll por Acción.
    public float RollCoolDown = 0.1f;                        // Cooldown del roll despues de ser Ejecutado. 
    #endregion

    float _forceStep;                                         //fuerza y direccion del movimiento
    float _timeStep;

    [Header("Blood System")]
    public float consumeBloodRate = 10f;
    public float HealthGainedBySeconds = 10f;
    public float Blood
    {
        get { return myStats.Sangre; }
        set
        {
            float val = value;
            if (val < 0)
                val = 0;

            myStats.Sangre = val;
            if (_myBars != null)
                _myBars.UpdateBloodAmmount((int)val);
        }
    }

    [Header("Combat")]
    public List<AnimationClip> AttackClips;
    public Weapon CurrentWeapon;
    public bool interruptAllowed = false;
    public float CombatRotationSpeed = 0f;
    public float ShockDuration = 2f;
    bool breakDefence = false;
    Vector3 _AttackOrientation = Vector3.zero;

    [Header("Estado Actual")]
    public bool _listenToInput = true;
    public bool _isInStair = false;

    [SerializeField] bool _invulnerable = false;                              // Si el jugador puede recibir daño.
    [SerializeField] bool _clamped = false;                                   // PRIVADO: si el jugador puede moverse.
    [SerializeField] bool _moving = false;                                    // PRIVADO: Si el jugador se está moviendo actualmente.
    [SerializeField] bool _canHeal = false;
    [SerializeField] bool _rolling = false;                                   // Si estoy rolleando actualmente.
    [SerializeField] bool _AttackStep = false;                                // si estoy dando el paso
    [SerializeField] bool _attacking = false;                                 // Si estoy atacando actualmente.
    [SerializeField] bool _shoked;
    [SerializeField] float speedR;

    //============================================= INTERFACES ================================================================

    public bool IsAlive => _hp > 0;
    public bool Active { get => enabled; set => enabled = value; }
    public bool invulnerable => _invulnerable;

    /// <summary>
    /// Permite Aplicar daño a esta entidad.
    /// </summary>
    /// <param name="EntryData"> Estadísticas de Entrada.</param>
    /// <returns>Estadísticas de Salida. </returns>
    public HitResult Hit(HitData EntryData)
    {
        HitResult hitResult = HitResult.Default();

        if (!_invulnerable && IsAlive)
        {
            //print("Daño recibido es: " + EntryData.Damage);

            float RealDamage = Mathf.Clamp((EntryData.Damage - (myStats.Resistencia * 0.5f)), 0, float.MaxValue);

            _shoked = false;
            _anims.SetBool("Disarmed", false);
            //print(string.Format("El jugador recibió {0} puntos de daño, y mitigó {1} puntos de daño.\nDaño final es: {2}", EntryData.Damage, (myStats.Resistencia * 0.5f), RealDamage));

            Health -= RealDamage;
            hitResult.HitConnected = true;

            if (IsAlive)
            {
                GetHit();

                //Permito recuperar estamina.
                _rolling = false;
                _attacking = false;
                _running = false;
                _recoverStamina = true;

                //FeedBack de Daño.
                _anims.SetTrigger("hurted");
                _listenToInput = false;
                //CurrentWeapon.InterruptAttack();
                _attacking = false;
                _rb.velocity /= 3;

                //Particula de Daño.
                var particle = Instantiate(OnHitParticle, transform.position, Quaternion.identity);
                Destroy(particle, 3f);

                _myBars.UpdateHeathBar(_hp, MaxHealth);
                _myBars.UpdateStamina(Stamina, MaxStamina);

                //Entro al estado de recibir daño.
                if (!_invulnerable) StartCoroutine(HurtFreeze());
            }
            else
            {
                hitResult.HitConnected = true;
                hitResult.TargetEliminated = true;
                myStats.Sangre = 0;
                Die();
            }
        }

        return hitResult;
    }

    public HitData DamageStats()
    {
        //Crear una nueva instancia de HitData.
        HitData returnValue;
        if (CurrentWeapon != null && CurrentWeapon.CurrentAttack != null)
        {
            returnValue = new HitData()
            {
                Damage = (myStats.Fuerza + CurrentWeapon.CurrentAttack.Damage),
                BreakDefence = breakDefence,
                AttackType = CurrentWeapon.CurrentAttack.attackType
            };
        }
        else
            returnValue = HitData.Default();

        return returnValue;
    }

    public void GetHitResult(HitResult result)
    {
        if (result.HitBlocked)
        {
            //print("Ataque Bloqueado.");
            //CurrentWeapon.InterruptAttack();
            StartCoroutine(Shock());
        }
        else if (result.HitConnected && CurrentWeapon != null && CurrentWeapon.CurrentAttack != null)
        {
            //Esto se llama cuando un Hit Conecta.

            if (result.TargetEliminated)
                FeedBlood(result.bloodEarned);
            CurrentWeapon.ConfirmHit();
            OnAttackLanded();
        }
    }

    /// <summary>
    /// Como vampiro XD
    /// </summary>
    /// <param name="blood">Cantidad de sangre Obtenida.</param>
    public void FeedBlood(float blood)
    {
        if (blood > 0)
        {
            Blood += blood;
            OnFeastBlood();
        }
    }

    //=========================================================================================================================

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _anims = GetComponentInChildren<Animator>();
        rollparticleEmission = RollParticle.emission;
        AxisOrientation = Camera.main.GetComponentInParent<MainCamBehaviour>().getPivotPosition();

        Health = BaseHP + (myStats.Vitalidad * 5);
        Stamina = MaxStamina;
        Blood = myStats.Sangre;

        OnStaminaIsEmpty += StaminaEffecPlay;
        OnFeastBlood += FeastBloodEfect;

        #region Combate

        // El inicio del ataque tiene muchos settings, que en general se van a compartir con otras armas
        // Asi que seria buena idea encapsularlo en un Lambda y guardarlo para un uso compartido.
        CurrentWeapon = new Weapon(_anims) { canContinueAttack = () => { return Stamina > 0; } };

        CurrentWeapon.OnBegginChain += () => 
        {
            _rolling = false;
            _moving = false;
            _running = false;

            _listenToInput = false;
            _attacking = true;
            _clamped = true;
        };
        CurrentWeapon.OnEndChain += () => 
        {
            //On Exit Combat
            _anims.SetInteger("combat", 0);
            _listenToInput = true;
            _attacking = false;
            _clamped = false;

            _AttackOrientation = Vector3.zero;
        };

        #region Attacks

        #region Light

        Attack L1 = new Attack() { ID = 1, Name = "Light1", Cost = 15f, Damage = 20f};
        L1.OnStart += () =>
        {
            //Por aqui va la activación de la animación correspondiente a este ataque.
            _anims.SetInteger("combat", 1);
            Stamina -= L1.Cost;
        };
        L1.OnEnableInput += () => 
        {
            print("Input está habilitado");
            marker.SetActive(true);
        };

        Attack L2 = new Attack() { ID = 3, Name = "Light2", Cost = 15f, Damage = 20f};
        L2.OnStart += () =>
        {
            _anims.SetInteger("combat", 3);
            Stamina -= L2.Cost;
        };
        L2.OnEnableInput += () => { marker.SetActive(true); };

        Attack L3 = new Attack() { ID = 7, Name = "Light3", Cost = 15f, Damage = 20f, isChainFinale = true };
        L3.OnStart += () =>
        {
            _anims.SetInteger("combat", 7);
            Stamina -= L3.Cost;
        };

        #endregion

        #endregion

        #region Conecciones

        L1.AddConnectedAttack(Inputs.light, L2);
        L2.AddConnectedAttack(Inputs.light, L3);

        #endregion

        CurrentWeapon.AddEntryPoint(Inputs.light, L1);

        #endregion

        //Permite tener un Delay
        OnActionHasEnded += () =>
        {
            StopCoroutine("StaminaRecoverDelay");
            StartCoroutine(StaminaRecoverDelay(StRecoverDelay));
        };
    }

    private void Start()
    {
        //Esto es para Updatear la cámara apenas comienza el juego.
        //OnPositionIsUpdated();
    }
    void Update()
    {
        if (!IsAlive || _shoked) return;

        //Inputs, asi es más responsive.
        float AxisY = Input.GetAxis("Vertical");
        float AxisX = Input.GetAxis("Horizontal");
        _anims.SetFloat("VelY", AxisX);
        _anims.SetFloat("VelX", AxisY);

        if (_listenToInput)
        {
            if (_canHeal)
            {
                if (Input.GetButton("FeedBlood"))
                {
                    _clamped = true;
                    if (Blood > 0)
                    {
                        _anims.SetBool("ConsummingBlood", true);
                        Health += HealthGainedBySeconds * Time.deltaTime;
                        Blood -= consumeBloodRate * Time.deltaTime;
                    }

                    if (Blood <= 0 || !_canHeal)
                    {
                        print("NO puedes curarte más...");
                        _anims.SetBool("ConsummingBlood", false);
                        _clamped = false;
                    }
                }

                if (Input.GetButtonUp("FeedBlood"))
                {
                    _anims.SetBool("ConsummingBlood", false);
                    _clamped = false;
                }
            }

            if (!_clamped)
            {
                if (Input.GetButton("Horizontal") || Input.GetButton("Vertical"))
                {
                    _moving = true;
                    _dir = AxisOrientation.forward * AxisY + AxisOrientation.right * AxisX;

                    if (!_running && Stamina > 0 && Input.GetButtonDown("Run"))
                    {
                        _running = true;
                        _anims.SetBool("Running", true);
                    }

                    if (_running)
                    {
                        if (Input.GetButtonUp("Run") || Stamina <= 0)
                        {
                            _running = false;
                            _anims.SetBool("Running", false);
                        }
                    }
                }
                else
                    _moving = false;
            }

            if (_rolling)
            {
                rollparticleEmission.enabled = true;
                transform.forward = _rollDir;
            }
            else if (!_rolling && Stamina > 0 && _moving && Input.GetButtonDown("Roll") && !_clamped && !_attacking)
            {
                //Calculamos la dirección y el punto final.
                _rollDir = (AxisOrientation.forward * AxisY + AxisOrientation.right * AxisX).normalized;
                Vector3 FinalPos = transform.position + (_rollDir * rollSpeed); // Calculo la posición Final.

                //Arreglamos nuestra orientación para cuando termina el roll.
                _dir = (FinalPos - transform.position).normalized;
                StartCoroutine(Roll());
                return;
            }

            if (!_attacking && !_rolling && Stamina > 0)
            {
                _AttackOrientation = (AxisOrientation.forward * AxisY) + (AxisOrientation.right * AxisX);

                if (Input.GetButtonDown("LighAttack"))
                    Attack(Inputs.light);
            }
        }

        if (_attacking)
        {
            _recoverStamina = false;
            _running = false;
            _moving = false;

            if (Input.GetButtonDown("LighAttack"))
                CurrentWeapon.FeedInput(Inputs.light);
            //else
            //   if (Input.GetButtonDown("StrongAttack"))
            //    CurrentWeapon.FeedInput(Inputs.strong);

            if (_AttackOrientation != Vector3.zero)
            {
                transform.forward = Vector3.Slerp(transform.forward, _AttackOrientation, CombatRotationSpeed);
            }
        }

        if (!_rolling)
            rollparticleEmission.enabled = false;

        if (!_rolling && !_moving && !_attacking)
        {
            _running = false;
            _anims.SetBool("Running", false);

            Vector3 originalVelocity = _rb.velocity;
            _rb.velocity =  new Vector3(originalVelocity.x * AxisX, _rb.velocity.y, originalVelocity.z * AxisY);
        }

        if (_running || _rolling || _attacking)
            _recoverStamina = false;
        else
            _recoverStamina = true;

        if (_recoverStamina && Stamina < MaxStamina)
        {
            float rate = StaminaRegeneration * Time.deltaTime;
            Stamina += rate;
        }
    }
    private void FixedUpdate()
    {
        if (!IsAlive) return;
        if (!_clamped && _moving) Move();
        if(_AttackStep)
        {
             _rb.AddForce(transform.forward * _forceStep,ForceMode.Impulse);
            _timeStep -= Time.deltaTime;
            if (_timeStep <= 0)
            {
                _AttackStep = false;
                _rb.velocity = Vector3.zero;

            }
        }
    }

    //======================================== Funciones Miembro ==============================================================

    public void EndAttackAnimation()
    {
        print("PLayer: LA tuya CON VINAGRE LCDTM");
        CurrentWeapon.EndCurrentAttack();
    }

    public void Move()
    {
        float movementSpeed = walkSpeed;

        //Correcting Forward.
        if (_running)
        {
            movementSpeed = runSpeed;
            Stamina -= runCost * Time.deltaTime;
            if (_dir != Vector3.zero)
                transform.forward =  _dir;
        }
        else
        {
            Vector3 newForward = Vector3.Slerp(transform.forward, AxisOrientation.forward, rotationLerpSpeed);
            transform.forward = newForward;
        }

        // Update Position
        Vector3 moveDir = _dir.normalized * movementSpeed;

        moveDiR = moveDir;
        speedR = movementSpeed;

        //Hago un sphereCast basado en el movimiento.
        Ray ray = new Ray(transform.position + ((moveDiR * 0.1f) + (Vector3.up * 4)), Vector3.down);
        Physics.Raycast(ray, out RaycastHit info, 100f, floor);

        Vector3 realPosToGo = ((info.point - transform.position).normalized) * speedR;

        _rb.velocity = realPosToGo;
    }
    public void Attack(Inputs input)
    {
        if (!_attacking)
        {
            //On Begin Combat
            _attacking = true;

            //Bloqueo las animaciones anteriores.
            StopAllCoroutines();
            _anims.SetBool("Running", false);
            _anims.SetFloat("VelY", 0);
            _anims.SetFloat("VelX", 0);

            _moving = false;
            _clamped = true;

            CurrentWeapon.BegginCombo(input);
        }
    }
    public void GetHit()
    {
        OnGetHit();
    }
    public void Die()
    {
        _anims.SetTrigger("died");
        _clamped = true;
        _rb.isKinematic = true;

        StartCoroutine(ReduxStaminaTo0(3f));

        //Termina el juego...
        //Tengo que decirle a algún Mánager quién es el enemigo que me mató, y luego marcarlo como mi killer.
        //Al volver a matarlo, nos devuelve la sangre que perdimos.
    }
    /// <summary>
    /// Al morir el jugador, la barra de estamina se reduce gradualmente a 0.
    /// </summary>
    /// <param name="duration">El tiempo en segundos que va a durar el Fade Out</param>
    /// <returns></returns>
    public IEnumerator ReduxStaminaTo0(float duration)
    {
        float remaining = duration;
        _recoverStamina = false;
        float originalStamina = Stamina;

        while (remaining > 0)
        {
            remaining -= Time.deltaTime;

            Stamina = (remaining / duration) * originalStamina;
            yield return null;
        }

        Stamina = 0;

        yield return new WaitForSeconds(2f);

        _myBars.FadeOut(3f);
    }

    //===================================== CORRUTINAS ========================================================================

    IEnumerator Roll()
    {
        //Primero que nada avisamos que no podemos hacer otras acciones.
        _clamped = true;
        _rolling = true;
        _recoverStamina = false;
        //_running = false;
        //_anims.SetBool("Running", false);
        _invulnerable = true;

        //FeedBack
        _anims.SetTrigger("RollAction");

        Stamina -= rollCost;

        // Hacemos el Roll.
        float left = rollDuration;

        do
        {
            left -= Time.deltaTime;
            Vector3 normalMove = new Vector3(_rollDir.x, 0, _rollDir.z) * rollSpeed;

            if (stairOrientation != null && _isInStair)
            {
                float stairf = Vector3.Dot(transform.forward, stairOrientation.forward);
                Vector3 stairDir = stairOrientation.forward * stairf;
                Vector3 stairMove = new Vector3(transform.forward.x, stairDir.y, stairDir.z).normalized;
                _rb.velocity = stairMove * rollSpeed;
            }
            else
                _rb.velocity = normalMove;

            yield return null;
        } while (left > 0);

        //Detengo el roll una vez que termine el roll.
        _rb.velocity = Vector3.zero;

        // Pequeño Delay para cuando el roll Termina.
        yield return new WaitForSeconds(0.1f);

        //End of Roll.
        _rolling = false;
        _recoverStamina = true;
        _clamped = false;                      // Avisamos que ya nos podemos mover.
        _invulnerable = false;

        // Adicional poner el roll en enfriamiento.
    }

    IEnumerator StaminaRecoverDelay(float Delay)
    {
        _recoverStamina = false;
        yield return new WaitForSeconds(Delay);
        _recoverStamina = true;
    }

    IEnumerator Shock()
    {
        _shoked = true;
        _anims.SetBool("Disarmed", true);
        yield return new WaitForSeconds(ShockDuration);
        _anims.SetBool("Disarmed", false);
        _shoked = false;
    }

    IEnumerator HurtFreeze()
    {
        _clamped = true;
        _invulnerable = true;

        // Cooldown.
        yield return new WaitForSeconds(1f);

        _clamped = false;
        _invulnerable = false;
        _listenToInput = true;
    }

    //======================================== COLLISIONES ====================================================================

    private void OnCollisionStay(Collision collision)
    {
        if (_rolling && collision.collider.tag == "DestructibleObject")
        {
            IDamageable<HitData, HitResult> Damageable = collision.gameObject.GetComponent<IDamageable<HitData, HitResult>>();
            if (_rolling && Damageable != null)
            {
                //Creo una nueva instancia de HitData.
                HitData data = new HitData() { Damage = 0f, BreakDefence = false };

                //Realizo el ataque.
                Damageable.Hit(data);
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        IDamageable<HitData, HitResult> Damageable = collision.gameObject.GetComponent<IDamageable<HitData, HitResult>>();
        if (_rolling && Damageable != null)
        {
            //Creo una nueva instancia de HitData.
            HitData data = new HitData() { Damage = 0f, BreakDefence = false };

            //Realizo el ataque.
            Damageable.Hit(data);
        }
    }

    //========================================== FEEDBACKS ====================================================================

    public void StaminaEffecPlay()
    {
        StaminaEffect.Play();
    }
    public void FeastBloodEfect()
    {
        FeastBlood.Play();
    }
    public void Step(float StepForce, float Steptime)
    {
        _forceStep = StepForce;
        _timeStep = Steptime;
        _AttackStep = true;
    }

    
}
