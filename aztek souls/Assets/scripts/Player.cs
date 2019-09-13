using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Entities;

public interface IPlayerController
{
    bool active { get; set; }
}

//[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour, IPlayerController, IKilleable, IAttacker<object[]>
{
    #region Estado
    //Eventos
    public event Action OnDie = delegate { };
    public event Action OnGetHit = delegate { };
    public event Action OnActionHasEnded = delegate { };
    //public event Action OnPositionIsUpdated = delegate { };

    //Objetos que hay que setear.
    public HealthBar _myBars;                               // Display de la vida y la estamina del jugador.
    [SerializeField] Transform AxisOrientation;             // Transform que determina la orientación del jugador.
    public LayerMask floor;
    public GameObject OnHitParticle;                        // Particula a instanciar al recibir daño.
    public ParticleSystem RollParticle;
    public Collider HitCollider;
    Rigidbody _rb;                                          // Componente Rigidbody.
    //CharacterController controller;
    Animator _anims;                                        // Componente Animator.
    [SerializeField]
    Weapon CurrentWeapon;

    //Orientación
    Vector3 _dir = Vector3.zero;                            // Dirección a la que el jugador debe mirar (Forward).
    Vector3 _rollDir = Vector3.zero;                        // Dirección a la que el jugador debe mirar al hacer un roll.


    [Header("Main Stats")] //Estados Principales.
    public float maxHp = 100f;                               // Máxima cantidad de vida posible del jugador.
    /// <summary>
    /// Controla el Display de la vida.
    /// </summary>
    public float Health
    {
        get { return _hp; }
        set
        {
            if (value < 0) value = 0;
            _hp = value;

            if (_myBars != null)
                _myBars.UpdateHeathBar(_hp, maxHp);
        }
    }
    float _hp = 100f;                                        // PRIVADO: valor actual de la vida.

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
                StartCoroutine(exhausted());
                _st = 0;
            }
            _st = value;

            //Display Value
            if (_myBars != null)
                _myBars.UpdateStamina(Stamina, MaxStamina);
        }
    }
    public float MaxStamina = 100f;                          // Estamina máxima del jugador.
    public float StaminaRegeneration = 2f;                   // Regeneración por segundo de estamina.
    public float StRecoverDelay = 0.8f;                      // Delay de Regeneración de estamina luego de ejectuar una acción.
    public float ExhaustTime = 2f;                           // Tiempo que dura el Estado de "Exhaust".
    [Range(2,10)]
    public float staminaRateDecrease = 5;                    // Reducción de regeneración de stamina al estar exhausto.
    bool _recoverStamina = true;                             // Verdadero cuando se pierde estamina.
    bool _exhausted = false;                                 // Verdadero cuando mi estamina se reduce a 0.

    public float walkSpeed = 4f;                             // Velocidad de movimiento del jugador al caminar.

    public float runSpeed = 20f;                             // Velocidad de movimiento del jugador al correr.
    public float runCost = 20;                               // Costo por segundo de la acción correr.
    bool _running = false;                                   // PRIVADO: si el jugador esta corriendo actualmente.

    bool _invulnerable = false;                              // Si el jugador puede recibir daño.
    bool _clamped = false;                                    // PRIVADO: si el jugador puede moverse.
    bool _moving = false;                                    // PRIVADO: Si el jugador se está moviendo actualmente.

    public bool isInStair;
    public Transform stairOrientation;
    public float rollSpeed = 30f;                            // Velocidad de desplazamiento mientras hago el roll.
    public float rollDuration = 0.8f;                        // Duración del Roll.
    public float rollCost = 20f;                             // Costo del roll por Acción.
    public float RollCoolDown = 0.1f;                        // Cooldown del roll despues de ser Ejecutado.
    bool _canRoll = true;                                    // Si puedo rollear.
    bool _rolling = false;                                   // Si estoy rolleando actualmente.

    bool _attacking = false;                                 // Si estoy atacando actualmente.

    public bool IsAlive => _hp > 0;                          //Implementación de IKilleable.

    public bool active { get => enabled; set => enabled = value; }
    public bool invulnerable => _invulnerable;

    #endregion

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        //controller = GetComponent<CharacterController>();
        _anims = GetComponentInChildren<Animator>();
        AxisOrientation = Camera.main.GetComponentInParent<MainCamBehaviour>().getPivotPosition();

        //INICIO DEL COMBATE.
        // El inicio del ataque tiene muchos settings, que en general se van a compartir con otras armas
        // Asi que seria buena idea encapsularlo en un Lambda y guardarlo para un uso compartido.
        CurrentWeapon = new Weapon(
                        () => {
                            //On Begin Attack
                            _attacking = true;

                            //Bloqueo las animaciones anteriores.
                            StopAllCoroutines();
                            _anims.SetBool("Running", false);
                            _anims.SetFloat("VelY", 0);
                            _anims.SetFloat("VelX", 0);

                            _moving = false;

                            _clamped = true;

                            //Debug.LogWarning("INICIO COMBATE");
                        }, 
                        () =>
                        {
                            //On Exit Attack
                            _attacking = false;

                            HitCollider.enabled = false;
                            _clamped = false;
                            CurrentWeapon.CurrentAttack = null;
                            //Debug.LogWarning("FIN COMBATE");
                        }
                        );
        CurrentWeapon.canContinueAttack = () => { return Stamina > 0; };
        CurrentWeapon.DuringAttack += () => 
        {
            float AxisX = Input.GetAxis("Horizontal");
            float AxisY = Input.GetAxis("Vertical");

            //Corregir el forward lentamente.
            _dir = AxisOrientation.forward * AxisY + AxisOrientation.right * AxisX;
            Vector3 newForward = Vector3.Slerp(transform.forward, _dir, 1f);
            transform.forward = newForward;

            if (Input.GetButton("Vertical"))
            {
                _anims.SetFloat("VelX", AxisY);
                _anims.SetFloat("VelY", 0);

                //Moverme ligeramente.
                Vector3 moveDir = (AxisOrientation.forward * AxisY).normalized * (walkSpeed / 8);
                _rb.velocity = new Vector3(moveDir.x, _rb.velocity.y, moveDir.z);
            }

            if (Stamina > rollCost && Input.GetButtonDown("Roll"))
            {
                _rollDir = (AxisOrientation.forward * AxisY + AxisOrientation.right * AxisX).normalized;
                CurrentWeapon.InterruptAttack();
                StartCoroutine(Roll());
            }
        };

        //Combo 1
        Attack light1 = new Attack() { IDName = "A", AttackDuration = 0.7f, Cost = 15f, Damage = 20f };
        Attack light2 = new Attack() { IDName = "B", AttackDuration = 0.5f, Cost = 15f, Damage = 20f };
        Attack light3 = new Attack() { IDName = "C", AttackDuration = 1f, Cost = 15f, Damage = 20f };
        Attack quick1 = new Attack() { IDName = "C", AttackDuration = 1f, Cost = 10f, Damage = 15f };
        Attack quick2 = new Attack() { IDName = "C", AttackDuration = 1f, Cost = 10f, Damage = 15f };
        Attack heavy1 = new Attack() { IDName = "D", AttackDuration = 1f, Cost = 25f, Damage = 30f };
        Attack Airheavy = new Attack() { IDName = "D", AttackDuration = 2.2f, Cost = 30f, Damage = 30f };

        light1.AddConnectedAttack(Inputs.light, light2);
        light1.OnExecute += () => 
        {
            //Por aqui va la activación de la animación correspondiente a este ataque.
            _anims.SetTrigger("atk1");
            _anims.SetInteger("combat", 0);
            Stamina -= light1.Cost;
            //print("Ejecutando Ataque:" + light1.IDName);
        };

        light2.AddConnectedAttack(Inputs.light, light3);
        light2.AddConnectedAttack(Inputs.strong, Airheavy);
        light2.OnExecute += () => {
            _anims.SetInteger("combat", 1);
            Stamina -= light2.Cost;
            //print("Ejecutando Ataque:" + light2.IDName);
        };


        light3.OnExecute += () => {
            Stamina -= light3.Cost;
            _anims.SetInteger("combat", 2);
            //print("Ejecutando Ataque:" + light3.IDName);
        };

        Airheavy.OnExecute += () => {
            Stamina -= Airheavy.Cost;
            _anims.SetInteger("combat", 3);
            //print("Ejecutando Ataque:" + Airheavy.IDName);
        };

        heavy1.AddConnectedAttack(Inputs.light, quick1);
        heavy1.OnExecute += () => {
            Stamina -= Airheavy.Cost;
            _anims.SetTrigger("atk2");
            //print("Ejecutando Ataque:" + heavy1.IDName);
        };

        quick1.AddConnectedAttack(Inputs.light, quick2);
        quick1.OnExecute += () =>
        {
            Stamina -= Airheavy.Cost;
            _anims.SetInteger("combat", 4);
            //print("Ejecutando Ataque:" + quick1.IDName);

        };

        quick2.AddConnectedAttack(Inputs.light, quick1);
        quick2.OnExecute += () =>
        {
            Stamina -= Airheavy.Cost;
            _anims.SetInteger("combat", 5);
            //print("Ejecutando Ataque:" + quick2.IDName);

        };

        CurrentWeapon.AddEntryPoint(Inputs.light, light1);
        //Acá hace falta un entryPoint Para el primer ataque Pesados
        CurrentWeapon.AddEntryPoint(Inputs.strong, heavy1);


        //FIN DEL COMBATE.

        Health = maxHp;
        Stamina = MaxStamina;

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

    // Update is called once per frame
    void Update()
    {
        if (!IsAlive) return;

        //Inputs, asi es más responsive.

        float AxisY = Input.GetAxis("Vertical");
        float AxisX = Input.GetAxis("Horizontal");
        _anims.SetFloat("VelY", AxisX);
        _anims.SetFloat("VelX", AxisY);

        if (!_clamped)
        {
            if (Input.GetButton("Horizontal") || Input.GetButton("Vertical"))
            {
                _moving = true;
                _dir = AxisOrientation.forward * AxisY + AxisOrientation.right * AxisX;

                if (!_running && Stamina > 0 && !_exhausted && Input.GetButtonDown("Run"))
                {
                    _running = true;
                    _anims.SetBool("Running", true);
                }

                if (_running && Input.GetButtonUp("Run") || _exhausted)
                {
                    _running = false;
                    _anims.SetBool("Running", false);
                }
            }
            else
                _moving = false;
        }

        if (_rolling) transform.forward = _rollDir;
        else if(!_rolling && Stamina > rollCost && _moving && Input.GetButtonDown("Roll"))
        {
            //Calculamos la dirección y el punto final.
            _rollDir = (AxisOrientation.forward * AxisY + AxisOrientation.right * AxisX).normalized;
            Vector3 FinalPos = transform.position + (_rollDir * rollSpeed); // Calculo la posición Final.

            //Arreglamos nuestra orientación para cuando termina el roll.
            _dir = (FinalPos - transform.position).normalized;
            StartCoroutine(Roll());
            return;
        }

        if (_attacking)
        {
            CurrentWeapon.Update();
            return;
        }
        else if (!_attacking && Input.GetButtonDown("LighAttack") || Input.GetButtonDown("StrongAttack"))
        {
            CurrentWeapon.StartAttack();
            return;
        }


        if (!_rolling && !_moving)
        {
            _running = false;
            _anims.SetBool("Running", false);

            var vel = _rb.velocity;
            _rb.velocity =  new Vector3(vel.x * AxisX, _rb.velocity.y, vel.z * AxisY);
        }

        if (_running || _rolling || _attacking)
            _recoverStamina = false;
        else
            _recoverStamina = true;

        if (_recoverStamina && Stamina < MaxStamina)
        {
            float rate = (_exhausted ? StaminaRegeneration / staminaRateDecrease : StaminaRegeneration) * Time.deltaTime;
            Stamina += rate;
        }
    }

    private void FixedUpdate()
    {
        if (!IsAlive) return;
        if (!_clamped && _moving) Move();
    }

    Vector3 moveDiR;
    float speedR;

    public void Move()
    {
        float movementSpeed = walkSpeed;

        //Correcting Forward.
        if (_running)
        {
            movementSpeed = runSpeed;
            Stamina -= runCost * Time.deltaTime;
            if (_dir != Vector3.zero)
                transform.forward = _dir;
        }
        else
        {
            Vector3 newForward = Vector3.Slerp(transform.forward, AxisOrientation.forward, 0.1f);
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

    public void Die()
    {
        _anims.SetTrigger("died");
        _clamped = true;
        _rb.isKinematic = true;

        //Termina el juego...
    }

    IEnumerator DamageEffect()
    {
        //Start -->
        //Intensity = 0.65f;
        //Color = rgb(255,0,0); FF0000

        yield return null;

        //Finish -->
        //Intensity = 0.28f;
        //Color = rgb(48,48,48); 303030

    }

    IEnumerator Roll()
    {
        //Primero que nada avisamos que no podemos hacer otras acciones.
        _clamped = true;
        _rolling = true;
        _recoverStamina = false;
        _running = false;
        _anims.SetBool("Running", false);

        //FeedBack
        _anims.SetTrigger("RollAction");
        var emission = RollParticle.emission;
        emission.enabled = true;

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

        //yield return new WaitForSeconds(rollDuration);

        //Detengo el roll una vez que termine el roll.
        _rb.velocity = Vector3.zero;

        //Deshabilitamos la emission de la particula de roll.
        emission.enabled = false;

        // Pequeño Delay para cuando el roll Termina.
        yield return new WaitForSeconds(0.1f);

        //End of Roll.
        _rolling = false;
        _recoverStamina = true;
        _clamped = false;                      // Avisamos que ya nos podemos mover.

        // Adicional poner el roll en enfriamiento.
    }

    IEnumerator RollCooldown()
    {
        _canRoll = false;
        yield return new WaitForSeconds(RollCoolDown);
        _canRoll = true;
    }

    IEnumerator StaminaRecoverDelay(float Delay)
    {
        _recoverStamina = false;
        yield return new WaitForSeconds(Delay);
        _recoverStamina = true;
    }

    IEnumerator exhausted()
    {
        _exhausted = true;
        //print("Exhausted");
        yield return new WaitForSeconds(ExhaustTime);
        //print("Recovered");
        _exhausted = false;
    }

    IEnumerator HurtFreeze()
    {
        _anims.SetTrigger("hurted");
        _clamped = true;
        _invulnerable = true;
        
        // Cooldown.
        yield return new WaitForSeconds(1f);

        //Muerte del Jugador
        if (!IsAlive) Die();

        _clamped = false;
        _invulnerable = false;
    }

    public void GetDamage(params object[] DamageStats)
    {
        if (!_invulnerable)
        {
            //FeedBack de Daño.
            float Damage = (float)DamageStats[0];
            Health -= Damage;
            CurrentWeapon.InterruptAttack();
            _attacking = false;
            OnGetHit();
            _rb.velocity /= 3;

            //Particula de Daño.
            var particle = Instantiate(OnHitParticle, transform.position, Quaternion.identity);
            Destroy(particle, 3f);

            _myBars.UpdateHeathBar(_hp, maxHp);
            _myBars.UpdateStamina(Stamina, MaxStamina);

            //Entro al estado de recibir daño.
            StartCoroutine(HurtFreeze());
        }
    }

    public object[] GetDamageStats()
    {
        // Retornar la info del sistema de Daño.
        if (CurrentWeapon != null && CurrentWeapon.CurrentAttack != null)
        {
            var stats = CurrentWeapon.CurrentAttack.GetDamageStats();
            if (stats != null)
                return stats;
        }

        return new object[1] { 0f };
    }

    private void OnCollisionEnter(Collision collision)
    {
        IDamageable Damageable = collision.gameObject.GetComponent<IDamageable>();
        if (_rolling && Damageable != null)
        {
            Damageable.GetDamage(2f);
        }
    }
}
