﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Entities;

[RequireComponent(typeof(Rigidbody))]
public class LAWEA : MonoBehaviour, IKilleable, IAttacker<object[]>, CamTarget
{
    //Eventos
    public event Action OnDie = delegate { };
    public event Action OnActionHasEnded = delegate { };
    public event Action OnPositionIsUpdated = delegate { };

    //Objetos que hay que setear.
    public HealthBar _myBars;                               // Display de la vida y la estamina del jugador.
    public Transform AxisOrientation;                       // Transform que determina la orientación del jugador.
    Rigidbody _rb;                                          // Componente Rigidbody.
    Animator _anims;                                        // Componente Animator.

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
    bool _recoverStamina = true;                             // Verdadero cuando se pierde estamina.
    bool _exhausted = false;                                 // Verdadero cuando mi estamina se reduce a 0.

    public float walkSpeed = 4f;                             // Velocidad de movimiento del jugador al caminar.

    public float runSpeed = 20f;                             // Velocidad de movimiento del jugador al correr.
    public float runCost = 20;                               // Costo por segundo de la acción correr.
    bool _running = false;                                   // PRIVADO: si el jugador esta corriendo actualmente.

    bool _invulnerable = false;                              // Si el jugador puede recibir daño.
    bool _canMove = true;                                    // PRIVADO: si el jugador puede moverse.
    bool _moving = false;                                    // PRIVADO: Si el jugador se está moviendo actualmente.



    public float rollSpeed = 30f;                            // Velocidad de desplazamiento mientras hago el roll.
    public float rollDuration = 0.8f;                        // Duración del Roll.
    public float rollCost = 20f;                             // Costo del roll por Acción.
    public float RollCoolDown = 0.1f;                        // Cooldown del roll despues de ser Ejecutado.
    bool _canRoll = true;                                    // Si puedo rollear.
    bool _rolling = false;                                   // Si estoy rolleando actualmente.

    public bool IsAlive => _hp > 0;                          //Implementación de IKilleable.

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _anims = GetComponentInChildren<Animator>();

        Health = maxHp;
        Stamina = MaxStamina;

        OnActionHasEnded += () =>
        {
            StopCoroutine("StaminaRecoverDelay");
            StartCoroutine(StaminaRecoverDelay(StRecoverDelay));
        };
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsAlive) return;

        //Inputs, asi es más responsive.

        if (Stamina > 0 && !_exhausted && Input.GetButtonDown("Run")) _running = true;
        if (_running && Input.GetButtonUp("Run") || _exhausted ) _running = false;
        _anims.SetBool("Running", _running);

        if (!_rolling)
        {
            float AxisY = Input.GetAxis("Vertical");
            float AxisX = Input.GetAxis("Horizontal");

            _anims.SetFloat("VelY", AxisX);
            _anims.SetFloat("VelX", AxisY);

            if (Input.GetButtonDown("Roll"))
            {
                _anims.SetTrigger("RollAction");
                _rollDir = AxisOrientation.forward * AxisY + AxisOrientation.right * AxisX;

                StartCoroutine(Roll());
            }

            if (_canMove)
            {
                if (Input.GetButton("Horizontal") || Input.GetButton("Vertical"))
                {
                    _moving = true;
                    _dir = AxisOrientation.forward * AxisY + AxisOrientation.right * AxisX;
                }
                else _moving = false;
            }
        }
        else transform.forward = _rollDir;

        if (_recoverStamina && Stamina < MaxStamina)
        {
            float rate = (_exhausted ? StaminaRegeneration / 10 : StaminaRegeneration) * Time.deltaTime;
            Stamina += rate;
        }
    }

    private void FixedUpdate()
    {
        if (_moving) Move();
    }

    public void Move()
    {
        float movementSpeed = walkSpeed;

        //Correcting Forward.
        if (_running)
        {
            _recoverStamina = false;
            movementSpeed = runSpeed;
            Stamina -= runCost * Time.deltaTime;
            if (_dir != Vector3.zero)
                transform.forward = _dir;
        }
        else
        {
            _recoverStamina = true;
            Vector3 newForward = Vector3.Slerp(transform.forward, AxisOrientation.forward, 0.1f);
            transform.forward = newForward;
        }

        // Update Position
        _rb.MovePosition(transform.position + (_dir.normalized * movementSpeed * Time.deltaTime));
        OnPositionIsUpdated();
    }

    public void Die()
    {

    }

    IEnumerator Roll()
    {
        _rolling = true;
        _recoverStamina = false;

        Stamina -= rollCost;

        //Calculamos la dirección y el punto final.
        Vector3 FinalPos = transform.position + (_rollDir * rollSpeed); // Calculo la posición Final.

        //Arreglamos nuestra orientación.
        _dir = (FinalPos - transform.position).normalized;

        //Primero que nada avisamos que no podemos hacer otras acciones.
        _canMove = false;
        //transform.forward = _rollDir;

        // Hacemos el Roll.
        _rb.velocity = (_dir * rollSpeed);
        yield return new WaitForSeconds(rollDuration);
        _rb.velocity = Vector3.zero;

        // Pequeño Delay para cuando el roll Termina.
        yield return new WaitForSeconds(0.1f);

        //End of Roll.
        _rolling = false;
        _recoverStamina = true;
        _canMove = true;                      // Avisamos que ya nos podemos mover.
        //_dir = WorldForward.forward;       // Calculamos nuestra orientación...
        //transform.forward = _dir;          // Seteamos la orientación como se debe.
        //_col.enabled = true;                 // Reactivamos el collider.
        //States.Feed(CharacterState.idle);

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
        _canMove = false;
        
        // Cooldown.
        yield return new WaitForSeconds(1f);

        //Muerte del Jugador
        if (!IsAlive) Die();

        _canMove = true;
    }

    public void GetDamage(params object[] DamageStats)
    {
        if (!_invulnerable)
        {
            //FeedBack de Daño.
            float Damage = (float)DamageStats[0];
            Health -= Damage;
            _myBars.UpdateHeathBar(_hp, maxHp);
            _myBars.UpdateStamina(Stamina, MaxStamina);

            //Entro al estado de recibir daño.
            StartCoroutine(HurtFreeze());
        }
    }

    public object[] GetDamageStats()
    {
        // Retornar la info del sistema de Daño.
        throw new NotImplementedException();
    }
}
