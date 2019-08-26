using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using IA.StateMachine.Generic;
using Core.Entities;

[RequireComponent(typeof(Collider)), RequireComponent(typeof(Rigidbody))]
public class Hero : MonoBehaviour, IKilleable,IAttacker<object[]>
{
    public InputKeyMap controls;
    public HealthBar _myBars;
    [SerializeField] string currentStateDisplay = "";
    public Transform AxisOrientation;
    public enum CharacterState
    {
        idle,
        walking,
        running,
        rolling,
        Attacking,
        Hurted,
        dead
    }
    GenericFSM<CharacterState> States;
    public event Action OnDie = delegate { };
    public event Action OnPositionIsUpdated = delegate { };
    public event Action OnActionHasEnded = delegate { };

    public bool IsAlive => _hp > 0;                           //Implementación de IKilleable.

    [Header("Main Stats")]
    [SerializeField] float _hp = 100f;
    public float maxHp = 100f;
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
    [SerializeField] float _st = 100f;
    public float Stamina
    {
        get { return _st; }
        set
        {
            if (value < 0)
            {
                StartCoroutine(exhausted());
                value = 0;
            }
            _st = value;

            //Display Value
            if (_myBars != null)
                _myBars.UpdateStamina(Stamina, MaxStamina);
        }
    }
    public float MaxStamina = 100f;
    public float StaminaRegeneration = 2f;
    public float StRecoverDelay = 0.8f;
    public float ExhaustTime = 2f;

    public float walkSpeed = 4f;
    public float runSpeed = 6f;
    public float runCost = 20f;

    public float rollCost = 10f;
    public float rollDuration = 1f;
    public float rollSpeed = 10f;

    [Header("Attack System")]
    public string AttackButton = "Fire1";
    [HideInInspector] public float currentDamage = 0;
    public float[] comboInputFrame = new float[3];
    public float[] animDurations = new float[3];
    public float[] AttackCosts = new float[3];
    public float[] AttackDamages = new float[3];

    [Header("Debug Elements")]
    public Collider AttackCollider;

    //----------------Private Members---------------

    Func<bool> _condition_idle,_condition_walk, _condition_attack, _condition_roll, _runCondition, _condition_rollingWhileRun = delegate { return false; };

    Rigidbody _rb;
    Animator _am;
    Collider _col;

    Vector3 _dir = Vector3.zero;
    Vector3 _rollDir = Vector3.zero;


    bool invulnerable = false;
    bool canMove = true;
    bool _running = false;

    bool Attacking;

    int ComboCounter;
    int[] executionStack = new int[3];

    //4 Fixes.
    bool _rolling = false;
    bool _recoverStamina = true;
    bool _exhausted = false;

    //========================================= Unity Functions =======================================================

    void Awake()
    {
        _am = GetComponentInChildren<Animator>();
        _col = GetComponent<Collider>();
        _rb = GetComponent<Rigidbody>();
        executionStack = new int[]{ 0,0,0};
        Health = maxHp;
        OnActionHasEnded += () => { StartCoroutine(StaminaRecoverDelay()); };


        //Starting Display
        //_myBars = GetComponentInChildren<HealthBar>();

        //Relleno las acciones que me permite determinar si ingresé el input correspondiente.
        _condition_idle = () => { return Input.GetAxisRaw(controls.HorizontalAxis) == 0 && Input.GetAxisRaw(controls.VerticalAxis) == 0; };
        _condition_walk = () => { return Input.GetButton(controls.HorizontalAxis) || Input.GetButton(controls.VerticalAxis); };
        _condition_attack = () => { return Input.GetButton(controls.AttackButton) && !_exhausted; };
        _condition_roll = () => { return Stamina > 0 && !_exhausted && Input.GetButtonDown(controls.RollButton); };
        _condition_rollingWhileRun = () => { return _condition_roll() && (Input.GetButton(controls.HorizontalAxis) || Input.GetButton(controls.VerticalAxis)); };

        _runCondition = () => { return Stamina > 0f && !_exhausted && Input.GetButton(controls.ToogleRun); };

        //State Machine.
        #region Declaración de Estados
        var idle = new State<CharacterState>("Idle");
        var Walking = new State<CharacterState>("Walking");
        var Running = new State<CharacterState>("Running");
        var Rolling = new State<CharacterState>("Rolling");
        var Attacking = new State<CharacterState>("Attacking");
        var Hitted = new State<CharacterState>("Hurted");
        var Dead = new State<CharacterState>("Dead");
        #endregion

        #region Transiciones
        idle.AddTransition(CharacterState.dead, Dead)
            .AddTransition(CharacterState.Hurted, Hitted)
            .AddTransition(CharacterState.walking, Walking)
            .AddTransition(CharacterState.running, Running)
            //.AddTransition(CharacterState.rolling, Rolling)
            .AddTransition(CharacterState.Attacking, Attacking);

        Walking.AddTransition(CharacterState.dead, Dead)
               .AddTransition(CharacterState.Hurted, Hitted)
               .AddTransition(CharacterState.idle, idle)
               .AddTransition(CharacterState.running, Running)
               .AddTransition(CharacterState.rolling, Rolling)
               .AddTransition(CharacterState.Attacking, Attacking);

        Running.AddTransition(CharacterState.dead, Dead)
               .AddTransition(CharacterState.Hurted, Hitted)
               .AddTransition(CharacterState.idle, idle)
               .AddTransition(CharacterState.walking, Walking)
               .AddTransition(CharacterState.rolling, Rolling)
               .AddTransition(CharacterState.Attacking, Attacking);

        Rolling.AddTransition(CharacterState.dead, Dead)
               .AddTransition(CharacterState.idle, idle)
               .AddTransition(CharacterState.walking, Walking)
               .AddTransition(CharacterState.running, Running)
               .AddTransition(CharacterState.Attacking, Attacking);


        Attacking.AddTransition(CharacterState.dead, Dead)
                 .AddTransition(CharacterState.Hurted, Hitted)
                 .AddTransition(CharacterState.idle, idle)
                 .AddTransition(CharacterState.walking, Walking)
                 .AddTransition(CharacterState.running, Running)
                 .AddTransition(CharacterState.rolling, Rolling);

        Hitted.AddTransition(CharacterState.dead, Dead)
              .AddTransition(CharacterState.idle, idle)
              .AddTransition(CharacterState.walking, Walking)
              .AddTransition(CharacterState.running, Running)
              .AddTransition(CharacterState.rolling, Rolling);

        #endregion

        #region Idle State
        idle.OnEnter += (previousState) =>
        {
            //StopAllCoroutines();
            _am.SetFloat("VelX", Input.GetAxisRaw("Vertical"));
            _am.SetFloat("VelY", Input.GetAxisRaw("Horizontal"));
        };
        idle.OnUpdate += () =>
        {
            //Transiciones chequearemos el input.

            //Walk
            if (canMove && _condition_walk())
            {
                States.Feed(CharacterState.walking);
                return;
            }

            //RunStart
            if (_runCondition())
            {
                //print("RUN START");
                States.Feed(CharacterState.running);
                return;
            }

            Vector3 newForward = Vector3.Slerp(transform.forward, AxisOrientation.forward, 0.1f);
            transform.forward = newForward;
            //Roll ---> Cuando estas en Idle no podes decir en que dirección hacer el roll so...
        }; 
        #endregion

        #region Walk State
        Walking.OnUpdate += () =>
        {
            //Transiciones primero :D
            if (_condition_idle())
            {
                //print("From Walking to Idle");
                States.Feed(CharacterState.idle);
                return;
            }
            if (_runCondition())
            {
                States.Feed(CharacterState.running);
                return;
            }
            if (_condition_roll())
            {
                //Calculo la dirección de roll.
                Vector3 axis = new Vector3(Input.GetAxis(controls.HorizontalAxis), 0, Input.GetAxis(controls.VerticalAxis));
                //_dir = AxisOrientation.forward * AxisY + AxisOrientation.right * AxisX;
                _rollDir = AxisOrientation.forward * axis.z + AxisOrientation.right * axis.x;

                States.Feed(CharacterState.rolling);
                return;
            }

            _am.SetFloat("VelX", Input.GetAxis("Vertical"));
            _am.SetFloat("VelY", Input.GetAxis("Horizontal"));

            Move(Input.GetAxis(controls.HorizontalAxis),
                 Input.GetAxis(controls.VerticalAxis));
        }; 
        #endregion

        #region Run State
        Running.OnEnter += (previousState) =>
        {
            Debug.LogWarning("RUN START");
            _running = true;
            _recoverStamina = false;
            _am.SetBool("Running", _running);
            _am.SetFloat("VelX", Input.GetAxisRaw("Vertical"));
            _am.SetFloat("VelY", Input.GetAxisRaw("Horizontal"));


            Move(Input.GetAxis(controls.HorizontalAxis),
                 Input.GetAxis(controls.VerticalAxis));
        };
        Running.OnUpdate += () =>
        {
            //Transiciones primero :D

            if (Input.GetKeyUp(KeyCode.Space))
            {
                Debug.Assert(true, "ROLL NOW!");
            }

            if (_condition_roll())
            {
                Vector3 axis = new Vector3(Input.GetAxis(controls.HorizontalAxis), 0, Input.GetAxis(controls.VerticalAxis));
                //_dir = AxisOrientation.forward * AxisY + AxisOrientation.right * AxisX;
                _rollDir = AxisOrientation.forward * axis.z + AxisOrientation.right * axis.x;

                States.Feed(CharacterState.rolling);
            }

            _am.SetFloat("VelX", Input.GetAxisRaw("Vertical"));
            _am.SetFloat("VelY", Input.GetAxisRaw("Horizontal"));

            Move(Input.GetAxis(controls.HorizontalAxis),
                 Input.GetAxis(controls.VerticalAxis));

            if (!_runCondition())
            {
                //print("END OF RUN");
                States.Feed(CharacterState.idle);
            }
        };
        Running.OnExit += (nextState) =>
        {
            Debug.LogWarning("RUN END");
            _running = false;
            _recoverStamina = true;
            _am.SetBool("Running", _running);
            OnActionHasEnded();
        };
        #endregion

        #region Roll State

        Rolling.OnEnter += (previousState) => 
        {
            _am.SetTrigger("RollAction");
            invulnerable = true;
            _recoverStamina = false;
            StartCoroutine(Roll());
        };
        Rolling.OnExit += (nextState) => 
        {
            invulnerable = false;
            _recoverStamina = true;

            OnActionHasEnded();
            //StopCoroutine(Roll());
        }; 

        #endregion

        Attacking.OnEnter += (previousState) => { };
        Attacking.OnUpdate += () => { };
        Attacking.OnExit += (nextState) => { };

        #region OnHit State

        Hitted.OnEnter += (previousState) =>
        {
            canMove = false;
            _am.SetTrigger("hurted");
            StartCoroutine(HurtFreeze());
        };
        Hitted.OnExit += (x) =>
        {
            canMove = true;
        };

        #endregion

        Dead.OnEnter += (previousState) => 
        {
            //print("Estas Muerto Wey");

            canMove = false;
            _recoverStamina = false;
            _am.SetTrigger("died");
            OnDie();
        };


        States = new GenericFSM<CharacterState>(idle);
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsAlive) return;

        if (_recoverStamina && Stamina < MaxStamina)
        {
            float rate = (_exhausted ? StaminaRegeneration / 10 : StaminaRegeneration) * Time.deltaTime;
            Stamina += rate;
        }

        if (_rolling)
            OnPositionIsUpdated();

    }

    private void FixedUpdate()
    {
        if (!IsAlive) return;

        States.Update();
        currentStateDisplay = States.current.StateName;
    }

    //=================================================================================================================
    //-----------------------------------------------------------------------------------------------------------------

    #region Attack System
    private void Attack()
    {
        //Rellenar el Stack.
        executionStack = new int[] { 1, 0, 0 };
        canMove = false;
        StartCoroutine(Combo());
    }

    IEnumerator Combo()
    {
        print("Inicio del combo");
        Attacking = true;
        int index = 0;
        ComboCounter = 1;
        currentDamage = AttackDamages[0];
        float nextComboFrame;

        while (true)
        {
            if (index >= executionStack.Length || executionStack[index] == 0)
                break;

            //Ejecutamos el ataque correspondiente.
            ExecuteAnimation(executionStack[index]);
            Stamina -= AttackCosts[index];
            currentDamage = AttackDamages[index];

            AttackCollider.enabled = true;

            //Calculamos cuanto tiempo tiene que pasar para recibir input.
            nextComboFrame = animDurations[index] - comboInputFrame[index];
            yield return new WaitForSeconds(nextComboFrame);

            float inputFrame = comboInputFrame[index];//Cuánto tiempo espero para recibir input.
            bool gettedInput = false;
            while (inputFrame > 0)
            {
                //Chequear el nuevo input
                if (!gettedInput == Input.GetButtonDown(AttackButton))
                {
                    gettedInput = true;
                    ComboCounter++;
                    index++;

                    if (index < executionStack.Length)
                    {
                        executionStack[index] = ComboCounter;
                        print("recibido siguiente ataque: " + index + " ID de animación:" + executionStack[index]);
                    }
                    else
                        print("Recibido input, pero el combo llego a su fin.");
                    break;
                }
                yield return new WaitForSeconds(0.01f);
                inputFrame -= 0.01f;
            }

#if UNITY_EDITOR
            canMove = true;
            print("Fin ciclo de input");
            if (!gettedInput)
            {
                if (index < executionStack.Length)
                    print("Fin del Stack de Ejecución.");
                else
                    print("No se recibió input a tiempo.");
                index++;
            }
#endif
        }

        Attacking = false;
        executionStack = new int[] { 0, 0, 0 };
        AttackCollider.enabled = false;
        print("Fin del Combo");
    }

    public void ExecuteAnimation(int animationID)
    {
        switch (animationID)
        {
            case 1:
                //Attaque básico.
                _am.SetTrigger("atk1");
                break;
            case 2:
                //Combo 1
                _am.SetTrigger("atk2");
                break;
            case 3:
                //Combo 2
                _am.SetTrigger("atk3");
                //Añado el bono.
                break;

            default:
                break;
        }
    }
    #endregion

    //---------------------------------- MÉTODOS PÚBLICOS -------------------------------------------------------------
    //---------------------------------- MÉTODS PRIVADOS --------------------------------------------------------------

    void Move(float AxisX, float AxisY)
    {
        //WorldForward es un objeto que usamos para determinar nuestra orientacón en el mundo.
        //En este caso se trata del objeto Pivot de la cámara.

        _dir = AxisOrientation.forward * AxisY + AxisOrientation.right * AxisX;
        //print("La dirección es: " + _dir);
        //print("El jugador esta apretando, el axis? H: " + Input.GetAxis("Horizontal") + " V: " + Input.GetAxis("Vertical"));

        if (Input.GetButtonDown(controls.RollButton))
        {
            print("ROLL");
        }

        float movementSpeed = walkSpeed;

        //Correcting Forward.
        if (_running)
        {
            movementSpeed = runSpeed;
            Stamina -= runCost * Time.deltaTime;
            transform.forward = _dir;
        }
        else
        {
            Vector3 newForward = Vector3.Slerp(transform.forward, AxisOrientation.forward, 0.1f);
            transform.forward = newForward;
        }

        // Update Position
        _rb.MovePosition(transform.position + (_dir.normalized * movementSpeed * Time.deltaTime));
        OnPositionIsUpdated();
    }

    //---------------------------------------- CORRUTINAS -------------------------------------------------------------

    IEnumerator Roll()
    {
        _rolling = true;
        Stamina -= rollCost;

        //Calculamos la dirección y el punto final.
        Vector3 FinalPos = transform.position + (_rollDir * rollSpeed); // Calculo la posición Final.

        //Arreglamos nuestra orientación.
        _dir = (FinalPos - transform.position).normalized;

        //Primero que nada avisamos que no podemos hacer otras acciones.
        canMove = false;
        transform.forward = _dir;

        // Hacemos el Roll.
        _rb.velocity = (_dir * rollSpeed);
        yield return new WaitForSeconds(rollDuration);
        _rb.velocity = Vector3.zero;

        // Pequeño Delay para cuando el roll Termina.
        yield return new WaitForSeconds(0.2f);

        //End of Roll.
        _rolling = false;
        canMove = true;                      // Avisamos que ya nos podemos mover.
        //_dir = WorldForward.forward;       // Calculamos nuestra orientación...
        //transform.forward = _dir;          // Seteamos la orientación como se debe.
        _col.enabled = true;                 // Reactivamos el collider.
        States.Feed(CharacterState.idle);

        // Adicional poner el roll en enfriamiento.
    }

    IEnumerator StaminaRecoverDelay()
    {
        _recoverStamina = false;
        yield return new WaitForSeconds(StRecoverDelay);
        _recoverStamina = true;
    }

    IEnumerator exhausted()
    {
        _exhausted = true;
        print("Exhausted");
        yield return new WaitForSeconds(ExhaustTime);
        print("Recovered");
        _exhausted = false;
    }

    IEnumerator HurtFreeze()
    {
        // Cooldown.
        yield return new WaitForSeconds(1f);

        //Muerte del Jugador
        if (!IsAlive) States.Feed(CharacterState.dead);

        States.Feed(CharacterState.idle);
    }

    //--------------------------------------- INTERFACES --------------------------------------------------------------

    /// <summary>
    /// Permite que esta unidad Recíba Daño.
    /// </summary>
    /// <param name="DamageStats"></param>
    public void GetDamage(params object[] DamageStats)
    {
        if (!invulnerable)
        {
            //FeedBack de Daño.
            float Damage = (float)DamageStats[0];
            Health -= Damage;
            _myBars.UpdateHeathBar(_hp, maxHp);
            _myBars.UpdateStamina(Stamina, MaxStamina);

            //Entro al estado de recibir daño.
            States.Feed(CharacterState.Hurted);
        }
    }
    /// <summary>
    /// Retorna las estadísticas de daño de esta Unidad.
    /// </summary>
    public object[] GetDamageStats()
    {
        return new object[1]{ currentDamage };
    }
}
