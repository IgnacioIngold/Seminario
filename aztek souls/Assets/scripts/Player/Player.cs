﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using Core;
using Core.Entities;

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
    public int Sangre;
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
public class Player : MonoBehaviour, IPlayerController, IDamageable<HitData, HitResult>, IKilleable
{
    #region Eventos.

    public event Action OnDie = delegate { };
    public event Action OnGetHit = delegate { };
    public event Action OnAttackLanded = delegate { };
    public event Action OnActionHasEnded = delegate { };
    public event Action OnStaminaIsEmpty = delegate { };
    public event Action OnFeastBlood = delegate { };
    public event Action OnConsumeBlood = delegate { };
    public event Action OnHealthHitMax = delegate { };
    //public event Action OnPositionIsUpdated = delegate { }; 

    #endregion

    #region Variables de Inspector.

    public StatusBars _myBars;                               // Display de la vida y la estamina del jugador.
    public LevelUpPanel levelUpPanel;
    public Transform AxisOrientation;                       // Transform que determina la orientación del jugador.
    public LayerMask floor;                                 // Máscara de collisiones para el piso.
    public GameObject OnHitParticle;                        // Particula a instanciar al recibir daño.
    public ParticleSystem RollParticle;                     // Partícula que emite cuando rollea.
    public ParticleSystem FeastBlood;                       // Partícula que emite cuando recibe sangre.
    public PlayableDirector StaminaEffect;                  // Efecto que se reproduce al reducirse la estamina por debajo de cierto punto.
    public PlayableDirector CameraShake;                    // Efecto "Sacudón" que se reproduce al recibir daño.
    public GameObject BloodConsume;                         // Efecto curacion.
    #endregion

    [Header("Main Stats")] //Estados Principales.
    #region Vida
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
                _myBars.m_UpdateHeathBar(_hp, MaxHealth);
        }
    }
    public float MaxHealth
    {
        get { return BaseHP + (myStats.Vitalidad * 5); }
    }
    float _hp = 100f;                                        // PRIVADO: valor actual de la vida.
    bool _canHeal = false;
    #endregion

    #region Estamina.

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
            if (_st > MaxStamina)
                _st = MaxStamina;

            //Display Value
            if (_myBars != null)
            {
                //if (!_myBars.staminaBarIsVisible)
                //    _myBars.Fade(HealthBar.InfoComponent.StaminaBar, HealthBar.FadeAction.FadeIn, 1f);

                _myBars.m_UpdateStamina(_st, MaxStamina);
            }
        }
    }
    public float MaxStamina = 100f;                          // Estamina máxima del jugador.
    public float StaminaRegeneration = 2f;                   // Regeneración por segundo de estamina.
    public float StRecoverDelay = 0.8f;                      // Delay de Regeneración de estamina luego de ejectuar una acción.
    //public float ExhaustTime = 2f;                           // Tiempo que dura el Estado de "Exhaust".
    [Range(2, 10)]
    public float staminaRateDecrease = 5;                    // Reducción de regeneración de stamina al estar exhausto.
    public float rotationLerpSpeed = 0.1f;
    bool _recoverStamina = true;                             // Verdadero cuando se pierde estamina.

    #endregion

    #region Walk y Run
    public float walkSpeed = 4f;                             // Velocidad de movimiento del jugador al caminar.

    public float runSpeed = 20f;                             // Velocidad de movimiento del jugador al correr.
    public float runCost = 20;                               // Costo por segundo de la acción correr.
    bool _running = false;                                   // PRIVADO: si el jugador esta corriendo actualmente. 
    #endregion

    #region Estados Alterados.

    bool _invulnerable = false;                              // Si el jugador puede recibir daño.
    bool _clamped = false;                                   // PRIVADO: si el jugador puede moverse.
    bool _moving = false;                                    // PRIVADO: Si el jugador se está moviendo actualmente. 

    #endregion

    #region Roll

    public bool isInStair;
    public Transform stairOrientation;
    public float rollSpeed = 30f;                            // Velocidad de desplazamiento mientras hago el roll.
    public float rollDuration = 0.8f;                        // Duración del Roll.
    public float rollCost = 20f;                             // Costo del roll por Acción.
    public float RollCoolDown = 0.1f;                        // Cooldown del roll despues de ser Ejecutado.
    //bool _canRoll = true;                                    // Si puedo rollear.
    bool _rolling = false;                                   // Si estoy rolleando actualmente.
    bool _listenToInput = true;

    #endregion

    #region Sistema de Sangre

    [Header("Blood System")]
    public float consumeBloodRate = 10f;
    public float HealthGainedBySeconds = 10f;
    public int Blood
    {
        get { return myStats.Sangre; }
        set
        {
            int val = value;
            if (val < 0)
                val = 0;

            myStats.Sangre = val;
            if (_myBars != null)
                _myBars.m_UpdateBloodAmmount((int)val);
        }
    } 

    #endregion

    #region Combate

    [Header("Combat")]
    public List<AnimationClip> AttackClips;
    public Weapon CurrentWeapon;
    public bool interruptAllowed = true;
    public float CombatRotationSpeed = 0.1f;
    public float ShockDuration = 2f;
    bool _attacking = false;                                 // Si estoy atacando actualmente.
    bool _shoked;
    bool breakDefence = false; 

    #endregion

    #region Orientación.

    Vector3 _dir = Vector3.zero;                            // Dirección a la que el jugador debe mirar (Forward).
    Vector3 _rollDir = Vector3.zero;                        // Dirección a la que el jugador debe mirar al hacer un roll. 

    #endregion

    Rigidbody _rb;                                          // Componente Rigidbody.
    Animator _anims;                                        // Componente Animator.
    Vector3 moveDiR;
    float speedR;

    //============================================= INTERFACES ================================================================

    public bool IsAlive => _hp > 0;
    public bool active { get => enabled; set => enabled = value; }
    public bool invulnerable => _invulnerable;

    /// <summary>
    /// Permite Aplicar daño a esta entidad.
    /// </summary>
    /// <param name="EntryData"> Estadísticas de Entrada.</param>
    /// <returns>Estadísticas de Salida. </returns>
    public HitResult Hit(HitData EntryData)
    {
        HitResult hitResult = HitResult.Empty();

        if (!_invulnerable && IsAlive)
        {
            print("Daño recibido es: " + EntryData.Damage);

            float RealDamage = EntryData.Damage - (myStats.Resistencia * 0.5f);

            _shoked = false;
            _anims.SetBool("Disarmed", false);
            print(string.Format("El jugador recibió {0} puntos de daño, y mitigó {1} puntos de daño.\nDaño final es: {2}", EntryData.Damage, (myStats.Resistencia * 0.5f), RealDamage));

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
                CurrentWeapon.InterruptAttack();
                _attacking = false;
                _rb.velocity /= 3;

                //Particula de Daño.
                var particle = Instantiate(OnHitParticle, transform.position, Quaternion.identity);
                Destroy(particle, 3f);

                _myBars.m_UpdateHeathBar(_hp, MaxHealth);
                _myBars.m_UpdateStamina(Stamina, MaxStamina);

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


    //public void GetDamage(params object[] DamageStats)
    //{
    //    if (!_invulnerable && IsAlive)
    //    {
    //        IAttacker<object[]> Aggresor = (IAttacker<object[]>)DamageStats[0];
    //        float Damage = (float)DamageStats[1];

    //        //Cálculo el daño real.
    //        float resist = myStats.Resistencia * 0.5f;
    //        Damage -= resist;
    //        _shoked = false;
    //        _anims.SetBool("Disarmed", false);

    //        print(string.Format("El jugador recibió {0} puntos de daño, y mitigó {1} puntos de daño.\nDaño final es: {2}", (float)DamageStats[1], resist, Damage));

    //        Health -= Damage;

    //        if (IsAlive)
    //        {
    //            Aggresor.OnHitConfirmed(new object[1] { 0f });

    //            //Permito recuperar estamina.
    //            _rolling = false;
    //            _attacking = false;
    //            _running = false;
    //            _recoverStamina = true;

    //            //FeedBack de Daño.
    //            _anims.SetTrigger("hurted");
    //            _listenToInput = false;
    //            CurrentWeapon.InterruptAttack();
    //            _attacking = false;
    //            GetHit();
    //            _rb.velocity /= 3;

    //            //Particula de Daño.
    //            var particle = Instantiate(OnHitParticle, transform.position, Quaternion.identity);
    //            Destroy(particle, 3f);

    //            _myBars.m_UpdateHeathBar(_hp, MaxHealth);
    //            _myBars.m_UpdateStamina(Stamina, MaxStamina);

    //            //Entro al estado de recibir daño.
    //            if (!_invulnerable) StartCoroutine(HurtFreeze());
    //        }
    //        if (!IsAlive)
    //        {
    //            Aggresor.OnKillConfirmed(new object[] { myStats.Sangre });
    //            myStats.Sangre = 0;
    //            Die();
    //        }
    //    }
    //}

    //public object[] GetDamageStats()
    //{
    //    // Retornar la info del sistema de Daño.
    //    if (CurrentWeapon != null && CurrentWeapon.CurrentAttack != null)
    //    {
    //        float DañoFinal = myStats.Fuerza + CurrentWeapon.CurrentAttack.Damage;
    //        object[] combatStats = new object[3] { this, DañoFinal, breakDefence };
    //        if (combatStats != null)
    //            return combatStats;
    //    }

    //    return new object[1] { 0f };
    //}

    //public void OnHit(object[] HitData)
    //{
    //    
    //}

    public void FeedHitResult(HitResult result)
    {
        if (result.HitBlocked)
        {
            print("Ataque Bloqueado.");
            CurrentWeapon.InterruptAttack();
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

    //public void OnHitBlocked(object[] data)
    //{

    //}
    //public void OnHitConfirmed(object[] data)
    //{


    //}
    ///// <summary>
    ///// Confirma al agresor, que su víctima ha muerto.
    ///// </summary>
    ///// <param name="data"></param>
    //public void OnKillConfirmed(object[] data)
    //{
    //    OnHitConfirmed(data);
    //}

    /// <summary>
    /// Retorna las estadísticas de combate de esta Entidad.
    /// </summary>
    /// <returns></returns>
    public HitData GetDamageStats()
    {
        //Crear una nueva instancia de HitData.
        HitData returnValue;
        if (CurrentWeapon != null && CurrentWeapon.CurrentAttack != null)
        {
            returnValue = new HitData()
            {
                Damage = (myStats.Fuerza + CurrentWeapon.CurrentAttack.Damage),
                BreakDefence = breakDefence
            };
        }
        else
            returnValue = HitData.Empty();

        return returnValue;
    }

    /// <summary>
    /// Como vampiro XD
    /// </summary>
    /// <param name="blood">Cantidad de sangre Obtenida.</param>
    public void FeedBlood(int blood)
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
        AxisOrientation = Camera.main.GetComponentInParent<MainCamBehaviour>().getPivotPosition();

        Health = BaseHP + (myStats.Vitalidad * 5);
        Stamina = MaxStamina;
        Blood = myStats.Sangre;

        _myBars.m_TurnOffAll();

        levelUpPanel.OnAccept += () =>
        {
            _myBars.m_UpdateHeathBar(_hp, MaxHealth);
            _myBars.m_UpdateStamina(_st, MaxStamina);
        };

        OnStaminaIsEmpty += StaminaEffecPlay;
        OnFeastBlood += FeastBloodEfect;

        #region Combate

        // El inicio del ataque tiene muchos settings, que en general se van a compartir con otras armas
        // Asi que seria buena idea encapsularlo en un Lambda y guardarlo para un uso compartido.
        CurrentWeapon = new Weapon(_anims);

        CurrentWeapon.canContinueAttack = () => { return Stamina > 0; };
        CurrentWeapon.DuringAttack += () =>
        {
            float AxisX = Input.GetAxis("Horizontal");
            float AxisY = Input.GetAxis("Vertical");

            Vector3 orientation;

            if (AxisX == 0 && AxisY == 0)
                orientation = AxisOrientation.forward;
            else
            {
                orientation = (AxisOrientation.forward * AxisY) + (AxisOrientation.right * AxisX);

                _anims.SetFloat("VelX", AxisY);
                _anims.SetFloat("VelY", 0);

                //Moverme ligeramente.
                Vector3 moveDir = orientation.normalized * (walkSpeed / 3);
                _rb.velocity = new Vector3(moveDir.x, _rb.velocity.y, moveDir.z);
            }

            transform.forward = Vector3.Slerp(transform.forward, orientation, CombatRotationSpeed);

            if (interruptAllowed && Stamina > rollCost && Input.GetButtonDown("Roll"))
            {
                _rollDir = (AxisOrientation.forward * AxisY + AxisOrientation.right * AxisX).normalized;
                CurrentWeapon.InterruptAttack();
                StartCoroutine(Roll());
            }
        };

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
        };

        #region Attacks

        #region Light

        Attack L1 = new Attack() { ID = 1, Name = "Light1", Cost = 15f, Damage = 20f, ChainIndex = 1, maxChainIndex = 3 };
        L1.OnStart += () =>
        {
            //Por aqui va la activación de la animación correspondiente a este ataque.
            _anims.SetInteger("combat", 1);
            Stamina -= L1.Cost;
            //print("Ejecutando Ataque:" + light1.IDName);
        };
        L1.AttackDuration = AttackClips[L1.ID - 1].length;
        L1.OnHit += () => 
        {
            //print("Light 1 conecto exitósamente");
        };

        Attack L2 = new Attack() { ID = 3, Name = "Light2", Cost = 15f, Damage = 20f, ChainIndex = 2, maxChainIndex = 3 };
        L2.OnStart += () =>
        {
            _anims.SetInteger("combat", 3);
            Stamina -= L2.Cost;
            //print("Ejecutando Ataque:" + light2.IDName);
        };
        L2.AttackDuration = AttackClips[L2.ID - 1].length;
        L2.OnHit += () => 
        {
            print("Light 2 conecto exitósamente");
        };

        Attack L3 = new Attack() { ID = 7, Name = "Light3",  Cost = 15f, Damage = 20f, ChainIndex = 3, maxChainIndex = 3 };
        L3.OnStart += () =>
        {
            _anims.SetInteger("combat", 7);
            Stamina -= L3.Cost;
            //print("Ejecutando Ataque:" + light3.IDName);
        };
        L3.AttackDuration = AttackClips[L3.ID - 1].length;
        L3.OnHit += () => 
        {
            print("Light 3 conecto exitósamente");
        };

        Attack L4 = new Attack() { ID = 5, Name = "Light4",  Cost = 10f, Damage = 15f, ChainIndex = 2, maxChainIndex = 3 };
        L4.OnStart += () =>
        {
            Stamina -= L4.Cost;
            _anims.SetInteger("combat", 5);
            //print("Ejecutando Ataque:" + quick1.IDName);
        };
        L4.AttackDuration = AttackClips[L4.ID - 1].length;

        Attack L5 = new Attack() { ID = 9, Name = "Light5",  Cost = 10f, Damage = 15f, ChainIndex = 3, maxChainIndex = 3 };
        L5.OnStart += () =>
        {
            Stamina -= L5.Cost;
            _anims.SetInteger("combat", 9);
            //print("Ejecutando Ataque:" + quick2.IDName);
        };
        L5.AttackDuration = AttackClips[L5.ID - 1].length;

        #endregion

        #region Strong

        Attack S1 = new Attack() { ID = 2, Name = "Strong1", Cost = 25f, Damage = 30f, ChainIndex = 1, maxChainIndex = 3 };
        S1.OnStart += () =>
        {
            _anims.SetInteger("combat", 2);
            Stamina -= S1.Cost;
            breakDefence = true;
            //print("Ejecutando Ataque:" + heavy1.IDName);
        };
        S1.OnEnd += () => { breakDefence = false; };
        S1.AttackDuration = AttackClips[S1.ID - 1].length;

        Attack S2 = new Attack() { ID = 4, Name = "Strong2", Cost = 25f, Damage = 30f, ChainIndex = 1, maxChainIndex = 3 };
        S2.OnStart += () =>
        {
            _anims.SetInteger("combat", 4);
            Stamina -= S2.Cost;
            breakDefence = true;
            //print("Ejecutando Ataque:" + heavy1.IDName);
        };
        S2.OnEnd += () => { breakDefence = false; };
        S2.AttackDuration = AttackClips[S2.ID - 1].length;

        Attack S3 = new Attack() { ID = 6, Name = "Strong3", Cost = 30f, Damage = 30f, ChainIndex = 1, maxChainIndex = 3 };
        S3.OnStart += () =>
        {
            _anims.SetInteger("combat", 6);
            Stamina -= S3.Cost;
            breakDefence = true;
            //print("Ejecutando Ataque:" + Airheavy.IDName);
        };
        S3.OnEnd += () => { breakDefence = false; };
        S3.AttackDuration = AttackClips[S3.ID - 1].length;

        Attack S4 = new Attack() { ID = 8, Name = "Strong4", Cost = 30f, Damage = 30f, ChainIndex = 1, maxChainIndex = 3 };
        S4.OnStart += () =>
        {
            Stamina -= S4.Cost;
            _anims.SetInteger("combat", 8);
            breakDefence = true;
            //print("Ejecutando Ataque:" + S4.IDName);
        };
        S4.OnEnd += () => { breakDefence = false; };
        S4.AttackDuration = AttackClips[S4.ID - 1].length;

        #endregion

        #endregion

        #region Conecciones

        //N1
        L1.AddConnectedAttack(Inputs.light, L2);
        L1.AddConnectedAttack(Inputs.strong, S2);

        S1.AddConnectedAttack(Inputs.light, L4);
        S1.AddConnectedAttack(Inputs.strong, S3);

        //N2
        L2.AddConnectedAttack(Inputs.light, L3);
        L2.AddConnectedAttack(Inputs.strong, S4);

        //---> S2 no tiene conecciones.

        L4.AddConnectedAttack(Inputs.light, L5);

        //---> S3 no tiene conecciones.

        //N3
        //---> L3 no tiene conecciones.
        //---> S4 no tiene conecciones.
        //---> L5 no tiene conecciones.

        #endregion

        CurrentWeapon.AddEntryPoint(Inputs.light, L1);
        CurrentWeapon.AddEntryPoint(Inputs.strong, S1);

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

        if (_st == MaxStamina && _myBars.staminaBarIsVisible)
            _myBars.m_DelayedFade(StatusBars.InfoComponent.StaminaBar, StatusBars.FadeType.FadeOut, 2f, 2f);

        if (_hp == MaxHealth && _myBars.healthBarIsVisible)
            _myBars.m_DelayedFade(StatusBars.InfoComponent.HealthBar, StatusBars.FadeType.FadeOut, 2f, 2f);

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
                        BloodConsume.SetActive(true);
                        Health += HealthGainedBySeconds * Time.deltaTime;
                        Blood -= (int)(consumeBloodRate * Time.deltaTime);
                    }

                    if (Blood <= 0 || !_canHeal)
                    {
                        print("NO puedes curarte más...");
                        _anims.SetBool("ConsummingBlood", false);
                        BloodConsume.SetActive(false);
                        _clamped = false;
                    }
                }

                if (Input.GetButtonUp("FeedBlood"))
                {
                    _anims.SetBool("ConsummingBlood", false);
                    BloodConsume.SetActive(false);
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

            if (_rolling) transform.forward = _rollDir;
            else if (!_rolling && Stamina > rollCost && _moving && Input.GetButtonDown("Roll"))
            {
                //Calculamos la dirección y el punto final.
                _rollDir = (AxisOrientation.forward * AxisY + AxisOrientation.right * AxisX).normalized;
                Vector3 FinalPos = transform.position + (_rollDir * rollSpeed); // Calculo la posición Final.

                //Arreglamos nuestra orientación para cuando termina el roll.
                _dir = (FinalPos - transform.position).normalized;
                StartCoroutine(Roll());
                return;
            }

            if (!_attacking )
            {
                if (Input.GetButtonDown("LighAttack"))
                    Attack(Inputs.light);
                else
                if (Input.GetButtonDown("StrongAttack"))
                    Attack(Inputs.strong);
            } 
        }

        if (_attacking)
        {
            _recoverStamina = false;
            CurrentWeapon.Update();

            if (Input.GetButtonDown("LighAttack"))
                CurrentWeapon.FeedInput(Inputs.light);
            else
               if (Input.GetButtonDown("StrongAttack"))
                CurrentWeapon.FeedInput(Inputs.strong);
        }

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
    }

    //=========================================================================================================================

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
        _clamped = true;
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

        _myBars.m_FadeAll( StatusBars.FadeType.FadeOut, 3f);
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

        _moving = false;
        _clamped = true;
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
        _clamped = true;
        _rolling = true;
        _recoverStamina = false;
        //_running = false;
        //_anims.SetBool("Running", false);
        _invulnerable = true;

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
        _clamped = true;
        _invulnerable = true;

        // Cooldown.
        yield return new WaitForSeconds(1f);

        _clamped = false;
        _invulnerable = false;
        _listenToInput = true;
    }

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
    public void StaminaEffecPlay()
    {
        StaminaEffect.Play();
    }
    public void FeastBloodEfect()
    {
        FeastBlood.Play();
    }
}
