using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Core.Entities;
using UnityEngine.Playables;

public interface IPlayerController
{
    bool active { get; set; }
}

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
public class Player : MonoBehaviour, IPlayerController, IKilleable, IAttacker<object[]>
{
    #region Estado
    //Eventos
    public event Action OnDie = delegate { };
    public event Action OnGetHit = delegate { };
    public event Action OnAttackLanded = delegate { };
    public event Action OnActionHasEnded = delegate { };
    public event Action OnStaminaIsEmpty = delegate { };
    public event Action OnFeastBlood = delegate { };
    public event Action OnConsumeBlood = delegate { };
    public event Action OnHealthHitMax = delegate { };
    //public event Action OnPositionIsUpdated = delegate { };

    //Objetos que hay que setear.
    public HealthBar _myBars;                               // Display de la vida y la estamina del jugador.
    [SerializeField] Transform AxisOrientation;             // Transform que determina la orientación del jugador.
    public LayerMask floor;
    public GameObject marker;                               // Índicador de ventana de Input.
    public GameObject OnHitParticle;                        // Particula a instanciar al recibir daño.
    public ParticleSystem RollParticle;
    public ParticleSystem.EmissionModule rollparticleEmission;
    public ParticleSystem FeastBlood;
    public Collider HitCollider;
    Rigidbody _rb;                                          // Componente Rigidbody.
                                                            //CharacterController controller;
    Animator _anims;                                        // Componente Animator.
    //Orientación
    Vector3 _dir = Vector3.zero;                            // Dirección a la que el jugador debe mirar (Forward).
    Vector3 _rollDir = Vector3.zero;                        // Dirección a la que el jugador debe mirar al hacer un roll.

    public PlayableDirector StaminaEffect;
    public PlayableDirector CameraShake;

    [Header("Main Stats")] //Estados Principales.
    public Stats myStats;
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
    bool _canHeal = false;

    //Estamina.
    float _st = 100f;                                        // PRIVADO: valor actual de la estamina.
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
    //public float ExhaustTime = 2f;                           // Tiempo que dura el Estado de "Exhaust".
    [Range(2,10)]
    public float staminaRateDecrease = 5;                    // Reducción de regeneración de stamina al estar exhausto.
    public float rotationLerpSpeed = 0.1f;
    bool _recoverStamina = true;                             // Verdadero cuando se pierde estamina.
    public bool moving = false;                                    // PRIVADO: Si el jugador se está moviendo actualmente.
    public bool running = false;                                   // PRIVADO: si el jugador esta corriendo actualmente.
    public bool clamped = false;                                   // PRIVADO: si el jugador puede moverse.
    //bool _exhausted = false;                                 // Verdadero cuando mi estamina se reduce a 0.

    public float walkSpeed = 4f;                             // Velocidad de movimiento del jugador al caminar.

    public float runSpeed = 20f;                             // Velocidad de movimiento del jugador al correr.
    public float runCost = 20;                               // Costo por segundo de la acción correr.

    bool _invulnerable = false;                              // Si el jugador puede recibir daño.
  

    public bool isInStair;
    public Transform stairOrientation;
    public float rollSpeed = 30f;                            // Velocidad de desplazamiento mientras hago el roll.
    public float rollDuration = 0.8f;                        // Duración del Roll.
    public float rollCost = 20f;                             // Costo del roll por Acción.
    public float RollCoolDown = 0.1f;                        // Cooldown del roll despues de ser Ejecutado.
    public bool listenToInput = true;
    //bool _canRoll = true;                                  // Si puedo rollear.
    bool _rolling = false;                                   // Si estoy rolleando actualmente.
    bool _AttackStep = false;                                // si estoy dando el paso
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
    bool _attacking = false;                                 // Si estoy atacando actualmente.
    bool _shoked;
    bool breakDefence = false;
    Vector3 _AttackOrientation = Vector3.zero;

    #endregion

    //============================================= INTERFACES ================================================================

    public bool IsAlive => _hp > 0;
    public bool active { get => enabled; set => enabled = value; }
    public bool invulnerable => _invulnerable;

    public void GetDamage(params object[] DamageStats)
    {
        if (!_invulnerable && IsAlive)
        {
            IAttacker<object[]> Aggresor = (IAttacker<object[]>)DamageStats[0];
            float Damage = (float)DamageStats[1];
            print("Daño recibido es: " + (float)DamageStats[1]);

            //Cálculo el daño real.
            float resist = myStats.Resistencia * 0.5f;
            Damage -= resist;
            _shoked = false;
            _anims.SetBool("Disarmed", false);

            print(string.Format("El jugador recibió {0} puntos de daño, y mitigó {1} puntos de daño.\nDaño final es: {2}", (float)DamageStats[1], resist, Damage));

            Health -= Damage;

            if (IsAlive)
            {
                Aggresor.OnHitConfirmed(new object[1] { 0f });

                //Permito recuperar estamina.
                _rolling = false;
                _attacking = false;
                running = false;
                _recoverStamina = true;

                //FeedBack de Daño.
                _anims.SetTrigger("hurted");
                listenToInput = false;
                CurrentWeapon.InterruptAttack();
                _attacking = false;
                GetHit();
                _rb.velocity /= 3;

                //Particula de Daño.
                var particle = Instantiate(OnHitParticle, transform.position, Quaternion.identity);
                Destroy(particle, 3f);

                _myBars.UpdateHeathBar(_hp, MaxHealth);
                _myBars.UpdateStamina(Stamina, MaxStamina);

                //Entro al estado de recibir daño.
                if (!_invulnerable) StartCoroutine(HurtFreeze());
            }
            if (!IsAlive)
            {
                Aggresor.OnKillConfirmed(new object[] { myStats.Sangre });
                myStats.Sangre = 0;
                Die();
            }
        }
    }

    public object[] GetDamageStats()
    {
        // Retornar la info del sistema de Daño.
        if (CurrentWeapon != null && CurrentWeapon.CurrentAttack != null)
        {
            float DañoFinal = myStats.Fuerza + CurrentWeapon.CurrentAttack.Damage;
            object[] combatStats = new object[3] { this, DañoFinal, breakDefence };
            if (combatStats != null)
                return combatStats;
        }

        return new object[1] { 0f };
    }
    public void OnHitBlocked(object[] data)
    {
        int blockTipe = (int)data[0];
        print("Ataque Bloqueado.");
        CurrentWeapon.InterruptAttack();

        if (blockTipe == 1)
        {
            StartCoroutine(Shock());
        }
    }
    public void OnHitConfirmed(object[] data)
    {
        if (data.Count() < 1) return;

        if (CurrentWeapon != null && CurrentWeapon.CurrentAttack != null)
        {
            FeedBlood((float)data[0]);
            CurrentWeapon.ConfirmHit();
            OnAttackLanded();
        }
    }
    /// <summary>
    /// Confirma al agresor, que su víctima ha muerto.
    /// </summary>
    /// <param name="data"></param>
    public void OnKillConfirmed(object[] data)
    {
        OnHitConfirmed(data);
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
        //Time.timeScale = 0.08f;

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
        CurrentWeapon = new Weapon(_anims);

        CurrentWeapon.canContinueAttack = () => { return Stamina > 0; };
        CurrentWeapon.OnBegginChain += () => 
        {
            _rolling = false;
            moving = false;
            running = false;

            listenToInput = false;
            _attacking = true;
            clamped = true;
        };
        CurrentWeapon.OnEndChain += () => 
        {
            //On Exit Combat
            _anims.SetInteger("combat", 0);
            listenToInput = true;
            _attacking = false;
            clamped = false;

            _AttackOrientation = Vector3.zero;
        };

        #region Attacks

        #region Light

        Attack L1 = new Attack() { ID = 1, Name = "Light1", Cost = 15f, Damage = 20f, AttackDuration = 1.25f, ChainIndex = 1, maxChainIndex = 3 };
        L1.OnStart += () =>
        {
            //Por aqui va la activación de la animación correspondiente a este ataque.
            _anims.SetInteger("combat", 1);
            Stamina -= L1.Cost;
            print("Ejecutando Ataque:" + L1.Name);
        };
        L1.OnEnd += () => { print(string.Format("Ataque {0} ha llegado a su fin", L1.Name)); };
        L1.OnEnableInput += () => 
        {
            var lightAttack = L1.getConnectedAttack(Inputs.light);
            var strongAttack = L1.getConnectedAttack(Inputs.strong);
            print(string.Format("Se ha habilitado el input. Posibles siguiente ataques: Light[{0}], Heavy[{1}]", lightAttack != null ? lightAttack.Name : "No tiene conección", strongAttack != null ? strongAttack.Name : "No tiene conección"));
            marker.SetActive(true);
        };

        Attack L2 = new Attack() { ID = 3, Name = "Light2", Cost = 15f, Damage = 20f, AttackDuration = 1.25f, ChainIndex = 2, maxChainIndex = 3 };
        L2.OnStart += () =>
        {
            _anims.SetInteger("combat", 3);
            Stamina -= L2.Cost;
            print("Ejecutando Ataque:" + L2.Name);
        };
        L2.OnEnd += () => { print(string.Format("Ataque {0} ha llegado a su fin", L2.Name)); };
        L2.OnEnableInput += () => {
            var lightAttack = L2.getConnectedAttack(Inputs.light);
            var strongAttack = L2.getConnectedAttack(Inputs.strong);
            print(string.Format("Se ha habilitado el input. Posibles siguiente ataques: Light[{0}], Heavy[{1}]", lightAttack != null ? lightAttack.Name : "No tiene conección", strongAttack != null ? strongAttack.Name : "No tiene conección"));
            marker.SetActive(true);
        };

        Attack L3 = new Attack() { ID = 7, Name = "Light3",  Cost = 15f, Damage = 20f, AttackDuration = 1.25f, ChainIndex = 3, maxChainIndex = 3 };
        L3.OnStart += () =>
        {
            _anims.SetInteger("combat", 7);
            Stamina -= L3.Cost;
            print("Ejecutando Ataque:" + L3.Name);
        };
        L3.OnEnd += () => { print(string.Format("Ataque {0} ha llegado a su fin", L3.Name)); };

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

        if (listenToInput)
        {
            if (_canHeal)
            {
                if (Input.GetButton("FeedBlood"))
                {
                    clamped = true;
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
                        clamped = false;
                    }
                }

                if (Input.GetButtonUp("FeedBlood"))
                {
                    _anims.SetBool("ConsummingBlood", false);
                    clamped = false;
                }
            }

            if (!clamped)
            {
                if (Input.GetButton("Horizontal") || Input.GetButton("Vertical"))
                {
                    moving = true;
                    _dir = AxisOrientation.forward * AxisY + AxisOrientation.right * AxisX;

                    if (!running && Stamina > 0 && Input.GetButtonDown("Run"))
                    {
                        running = true;
                        _anims.SetBool("Running", true);
                    }

                    if (running)
                    {
                        if (Input.GetButtonUp("Run") || Stamina <= 0)
                        {
                            running = false;
                            _anims.SetBool("Running", false);
                        }
                    }
                }
                else
                    moving = false;
            }

            if (_rolling)
            {
                rollparticleEmission.enabled = true;
                transform.forward = _rollDir;
            }
            else if (!_rolling && Stamina > 0 && moving && Input.GetButtonDown("Roll") && !clamped && !_attacking)
            {
                //Calculamos la dirección y el punto final.
                _rollDir = (AxisOrientation.forward * AxisY + AxisOrientation.right * AxisX).normalized;
                Vector3 FinalPos = transform.position + (_rollDir * rollSpeed); // Calculo la posición Final.

                //Arreglamos nuestra orientación para cuando termina el roll.
                _dir = (FinalPos - transform.position).normalized;
                StartCoroutine(Roll());
                return;
            }

            if (!_attacking && !_rolling && Stamina>0)
            {
                _AttackOrientation = (AxisOrientation.forward * AxisY) + (AxisOrientation.right * AxisX);

                if (Input.GetButtonDown("LighAttack"))
                    Attack(Inputs.light);
                //else
                //if (Input.GetButtonDown("StrongAttack"))
                //    Attack(Inputs.strong);
            } 
        }

        if (_attacking)
        {
            _recoverStamina = false;
            running = false;
            moving = false;
            CurrentWeapon.Update();

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

        if (!_rolling && !moving && !_attacking)
        {
            running = false;
            _anims.SetBool("Running", false);

            Vector3 originalVelocity = _rb.velocity;
            _rb.velocity =  new Vector3(originalVelocity.x * AxisX, _rb.velocity.y, originalVelocity.z * AxisY);
        }

        if (running || _rolling || _attacking)
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
        if (!clamped && moving) Move();
    }

    //=========================================================================================================================

    Vector3 moveDiR;
    float speedR;

    public void Move()
    {
        float movementSpeed = walkSpeed;

        //Correcting Forward.
        if (running)
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
        RaycastHit info;
        Physics.Raycast(ray, out info, 100f, floor);

        Vector3 realPosToGo = ((info.point - transform.position).normalized) * speedR;

        _rb.velocity = realPosToGo;
    }

    public void GetHit()
    {
        OnGetHit();
    }

    public void Die()
    {
        _anims.SetTrigger("died");
        clamped = true;
        _rb.isKinematic = true;

        StartCoroutine(reduxStaminaTo0(3f));

        //Termina el juego...
        //Tengo que decirle a algún Mánager quién es el enemigo que me mató, y luego marcarlo como mi killer.
        //Al volver a matarlo, nos devuelve la sangre que perdimos.
    }

    /// <summary>
    /// Al morir el jugador, la barra de estamina se reduce gradualmente a 0.
    /// </summary>
    /// <param name="duration">El tiempo en segundos que va a durar el Fade Out</param>
    /// <returns></returns>
    public IEnumerator reduxStaminaTo0(float duration)
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

    public void Attack(Inputs input)
    {
        //On Begin Combat
        _attacking = true;

        //Bloqueo las animaciones anteriores.
        StopAllCoroutines();
        _anims.SetBool("Running", false);
        _anims.SetFloat("VelY", 0);
        _anims.SetFloat("VelX", 0);

        moving = false;
        clamped = true;
        //Debug.LogWarning("INICIO COMBATE");

        CurrentWeapon.BegginCombo(input);
    }

    //A futuro we.
    //public void LevelUp()
    //{
    //    Health = BaseHP + myStats.Vitalidad * 5;
    //}

    IEnumerator Roll()
    {
        //Primero que nada avisamos que no podemos hacer otras acciones.
        clamped = true;
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

            if (stairOrientation != null && isInStair)
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
        clamped = false;                      // Avisamos que ya nos podemos mover.
        _invulnerable = false;

        // Adicional poner el roll en enfriamiento.
    }

    //IEnumerator RollCooldown()
    //{
    //    _canRoll = false;
    //    yield return new WaitForSeconds(RollCoolDown);
    //    _canRoll = true;
    //}

    IEnumerator StaminaRecoverDelay(float Delay)
    {
        _recoverStamina = false;
        yield return new WaitForSeconds(Delay);
        _recoverStamina = true;
    }

    //IEnumerator exhausted()
    //{
    //    _exhausted = true;
    //    //print("Exhausted");
    //    yield return new WaitForSeconds(ExhaustTime);
    //    //print("Recovered");
    //    _exhausted = false;
    //}

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
        clamped = true;
        _invulnerable = true;

        // Cooldown.
        yield return new WaitForSeconds(1f);

        clamped = false;
        _invulnerable = false;
        listenToInput = true;
    }

    private void OnCollisionStay(Collision collision)
    {
        if (_rolling && collision.collider.tag == "DestructibleObject")
        {
            IDamageable Damageable = collision.gameObject.GetComponent<IDamageable>();
            if (_rolling && Damageable != null)
            {
                Damageable.GetDamage(new object[3] { this, 0f, false });
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        IDamageable Damageable = collision.gameObject.GetComponent<IDamageable>();
        if (_rolling && Damageable != null)
        {
            Damageable.GetDamage(new object[3] { this, 0f, false });
        }
    }
    public void StaminaEffecPlay()
    {
        StaminaEffect.Play();
    }
    public void FeastBloodEfect()
    {
        FeastBlood.Play();
    }
    public void Step(float StepForce)
    {
        _forceStep = StepForce;
        _rb.AddForce(transform.forward * _forceStep, ForceMode.Impulse);
    }
}
