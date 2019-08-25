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
    Func<bool> _idle,_walk, _attack, _roll, _run = delegate { return false; };
    public event Action OnDie = delegate { };

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
            if (value < 0) value = 0;
            _st = value;

            //Display Value
            if (_myBars != null)
                _myBars.UpdateStamina(Stamina, MaxStamina);
        }
    }
    public float MaxStamina = 100f;
    public float StaminaRegeneration = 2f;

    public float walkSpeed = 4f;
    public float runSpeed = 6f;

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
    public Text HealthText;
    public Text StaminaText;

    HealthBar _myBars;

    //----------------Private Members---------------

    Rigidbody _rb;
    Animator _am;
    Collider _col;

    bool invulnerable = false;
    bool canMove = true;
    bool _running = false;
    Vector3 _dir = Vector3.zero;

    private bool Attacking;
    private int ComboCounter;

    public bool IsAlive => _hp > 0;
    int[] executionStack = new int[3];


    //========================================= Unity Functions =======================================================

    void Awake()
    {
        _am = GetComponentInChildren<Animator>();
        _col = GetComponent<Collider>();
        _rb = GetComponent<Rigidbody>();
        executionStack = new int[]{ 0,0,0};
        Health = maxHp;


        //Starting Display
        _myBars = GetComponentInChildren<HealthBar>();
        HealthText.text = "Health: " + _hp;
        StaminaText.text = "Stamina: " + _st;

        //Relleno las acciones que me permite determinar si ingresé el input correspondiente.
        _idle = () => { return Input.GetAxisRaw(controls.HorizontalAxis) == 0 && Input.GetAxisRaw(controls.VerticalAxis) == 0; };
        _walk = () => { return Input.GetButton(controls.HorizontalAxis) || Input.GetButton(controls.VerticalAxis); };
        _attack = () => { return Stamina > 0 && Input.GetButton(controls.AttackButton); };
        _roll = () => { return Stamina > 0 && Input.GetButtonDown(controls.RollButton); };

        _run = () => { return Stamina > 0f && Input.GetButton(controls.ToogleRun); };

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
            if (canMove && _walk())
            {
                States.Feed(CharacterState.walking);
                return;
            }

            //RunStart
            if (_run())
            {
                print("RUN START");
                States.Feed(CharacterState.running);
                return;
            }

            Vector3 newForward = Vector3.Slerp(transform.forward, AxisOrientation.forward, 0.1f);
            transform.forward = newForward;
            //Roll ---> Cuando estas en Idle no podes decir en que dirección hacer el roll so...
            //if (!rolling && _roll()) States.Feed(CharacterState.rolling);
        }; 
        #endregion

        #region Walk State
        Walking.OnUpdate += () =>
        {
            //Transiciones primero :D
            if (_idle())
            {
                print("From Walking to Idle");
                States.Feed(CharacterState.idle);
                return;
            }
            if (_run())
            {
                States.Feed(CharacterState.running);
                return;
            }
            if (_roll())
            {
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
            _running = true;
            _am.SetBool("Running", _running);
        };
        Running.OnUpdate += () =>
        {
            //Transiciones primero :D
            if (!_run())
            {
                //print("END OF RUN");
                States.Feed(CharacterState.idle);
                return;
            }

            if (_roll())
            {
                States.Feed(CharacterState.rolling);
                return;
            }

            _am.SetFloat("VelX", Input.GetAxisRaw("Vertical"));
            _am.SetFloat("VelY", Input.GetAxisRaw("Horizontal"));

            Move(Input.GetAxis(controls.HorizontalAxis),
                 Input.GetAxis(controls.VerticalAxis));
        };
        Running.OnExit += (nextState) =>
        {
            _running = false;
            _am.SetBool("Running", _running);
        };
        #endregion

        #region Roll State

        Rolling.OnEnter += (previousState) => 
        {
            _am.SetTrigger("RollAction");
            invulnerable = true;
            StartCoroutine(Roll(Input.GetAxis(controls.HorizontalAxis), Input.GetAxis(controls.VerticalAxis)));
        };
        Rolling.OnExit += (nextState) => 
        {
            invulnerable = false;
            StopCoroutine("Roll");
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
            _am.SetTrigger("died");
            OnDie();
        };


        States = new GenericFSM<CharacterState>(idle);
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsAlive) return;

        if (Stamina < MaxStamina)
            Stamina += StaminaRegeneration* Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (!IsAlive) return;

        States.Update();
        currentStateDisplay = States.current.StateName;
    }

    //#region Debug
    //private void OnDrawGizmosSelected()
    //{
    //    Gizmos.color = Color.red;
    //    Gizmos.matrix *= Matrix4x4.Scale(new Vector3(1, 0, 1));
    //    Gizmos.DrawWireSphere(transform.position, rollRange);
    //}
    //#endregion


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

        float movementSpeed = walkSpeed;

        //Correcting Forward.
        if (_running)
        {
            movementSpeed = runSpeed;
            Stamina -= 20f * Time.deltaTime;
            transform.forward = _dir;
        }
        else
        {
            Vector3 newForward = Vector3.Slerp(transform.forward, AxisOrientation.forward, 0.1f);
            transform.forward = newForward;
        }

        // Update Position
        _rb.MovePosition(transform.position + (_dir.normalized * movementSpeed * Time.deltaTime));
    }

    //---------------------------------------- CORRUTINAS -------------------------------------------------------------

    IEnumerator Roll(float AxisX, float AxisY)
    {
        Stamina -= rollCost;

        //Calculamos la dirección y el punto final.
        Vector3 rollDirection = AxisOrientation.forward * AxisY + AxisOrientation.right * AxisX;
        Vector3 FinalPos = Vector3.zero;           // Calculo la posición Final.
        //float realRange = rollRange;               // Necesito saber cual es el rango real de mi desplazamiento.

        FinalPos = transform.position + (rollDirection * rollSpeed);

        //Arreglamos nuestra orientación.
        _dir = (FinalPos - transform.position).normalized;

        //Calculamos la velocidad del desplazamiento:
        //float Velocity = rollVelocity * Time.deltaTime;

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

        canMove = true;                      // Avisamos que ya nos podemos mover.
        //_dir = WorldForward.forward;       // Calculamos nuestra orientación...
        //transform.forward = _dir;          // Seteamos la orientación como se debe.
        _col.enabled = true;                 // Reactivamos el collider.
        States.Feed(CharacterState.idle);

        // Adicional poner el roll en enfriamiento.
    }

    //Yo creo que esto podría tener un enfriamiento.
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
