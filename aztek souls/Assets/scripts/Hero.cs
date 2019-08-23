using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Core.Entities;


[RequireComponent(typeof(Collider)), RequireComponent(typeof(Rigidbody))]
public class Hero : MonoBehaviour, IKilleable,IAttacker<object[]>
{
    [Header("Main Stats")]
    [SerializeField] float _hp = 100f;
    public float Health
    {
        get { return _hp; }
        set
        {
            if (value < 0) value = 0;
            _hp = value;

            if (HealthText != null) HealthText.text = "Health: " + (int)_hp;
            else print("No asignaste el texto de la vida salamín");
        }
    }

    public float Speed = 4f;
    public float RunSpeed;
    public float rollRange = 5f;
    public float rollVelocity = 10f;
    public float rollCost = 10f;

    [SerializeField] float _st = 100f;
    public float Stamina
    {
        get { return _st; }
        set
        {
            if (value < 0) value = 0;
            _st = value;

            //Display Value
            if (StaminaText != null) StaminaText.text = "Stamina: " + (int)_st;
            else print("No asignaste el texto de la Stamina salamín");
        }
    }
    public float MaxStamina = 100f;
    public float StaminaRegeneration = 2f;

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
    public GameObject MousePosDebugObject;
    public GameObject RollPosDebugObject;
    public LayerMask RollObstacles;
    public Transform WorldForward;

    //----------------Private Members---------------

    Rigidbody _rb;
    Animator _am;
    Camera cam;
    Collider _col;

    bool invulnerable = false;
    bool canMove = true;
    bool _running = false;
    bool rolling = false;
    float _dirX;
    float _dirY;
    Vector3 _dir;

    private bool Attacking;
    private int ComboCounter;

    public bool IsAlive => _hp > 0;
    int[] executionStack = new int[3];

    //----------------------------------------- Unity Functions -------------------------------------------------------

    void Awake()
    {
        _am = GetComponentInChildren<Animator>();
        _col = GetComponent<Collider>();
        _rb = GetComponent<Rigidbody>();
        executionStack = new int[]{ 0,0,0};

        //Starting Display
        HealthText.text = "Health: " + _hp;
        StaminaText.text = "Stamina: " + _st;
    }

    void Start()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (Stamina<MaxStamina)
            Stamina += StaminaRegeneration* Time.deltaTime;

        if (!Attacking && Input.GetButtonDown(AttackButton))
            Attack();
    }

private void FixedUpdate()
    {
        if (!IsAlive) return;

        if (canMove && Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
        {
            //TODO: Acá tenemos que hacer que la key se libere al momento de iniciar correr, solo cuando volvemos a pulsar el shift
            //Deberíamos reanudar la acción de correr.

            _running = Input.GetKey(KeyCode.LeftShift);

            if (Stamina > 0f)
                Move(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), _running ? RunSpeed : Speed);
        }
        else
        {
            _am.SetFloat("VelX", Input.GetAxisRaw("Vertical"));
            _am.SetFloat("VelY", Input.GetAxisRaw("Horizontal"));
        }

        //Rool
        if (!rolling && Stamina >= rollCost && Input.GetKeyDown(KeyCode.Space) && canMove)
            RoolExecute(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.matrix *= Matrix4x4.Scale(new Vector3(1, 0, 1));
        Gizmos.DrawWireSphere(transform.position, rollRange);
    }

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


    private void RoolExecute(float AxisX, float AxisY)
    {
        Stamina -= rollCost;

        //Calculamos la dirección y el punto final.
        Vector3 rollDirection = WorldForward.forward * AxisY + WorldForward.right * AxisX;
        Vector3 FinalPos = Vector3.zero;

        //Necesito saber cual es el rango real de mi desplazamiento.
        float realRange = rollRange;
        RaycastHit obstacle;
        Ray ray = new Ray(transform.position, rollDirection);
        if (Physics.Raycast(ray, out obstacle, rollRange, RollObstacles))
        {
            //Golpeamos con algo y limitamos el desplazamiento, de acuerdo a un offset.
            if (obstacle.collider != null)
                realRange = Vector3.Distance(transform.position, obstacle.point);
        }

        FinalPos = transform.position + (rollDirection * realRange);

        //Arreglamos nuestra orientación.
        _dir = (FinalPos - transform.position).normalized;

        //Calculamos la velocidad del desplazamiento:
        float Velocity = rollVelocity * Time.deltaTime;

        _am.SetTrigger("RollAction");
        //Vamos a usar una corrutina
        StartCoroutine(Roll(FinalPos, Velocity));
    }

    public void Move(float AxisX, float AxisY,float movementSpeed)
    {
        _dir = WorldForward.forward * AxisY + WorldForward.right * AxisX;
        //_dir = WorldForward.forward;

        //Correcting Forward.
        Vector3 newForward = Vector3.Slerp(transform.forward, WorldForward.forward, 0.1f);
        transform.forward = newForward;
        //_rb.MoveRotation(Quaternion.Euler(newForward));
        //_rb.rotation = Quaternion.Euler(newForward);

        if (_running)
        {
            Stamina -= 20f * Time.deltaTime;
            transform.forward = _dir;
            _am.SetBool("Running", _running);
        }
        else
        {
            _running = false;
            _am.SetBool("Running", _running);
        }

        //Position
        //transform.position += _dir * movementSpeed * Time.deltaTime;

        _rb.MovePosition(transform.position + (_dir.normalized * movementSpeed * Time.deltaTime));
        _am.SetFloat("VelX", AxisY);
        _am.SetFloat("VelY", AxisX);
    }

    public void RotateWithCamera()
    {
        //float orientation = Input.GetAxisRaw("Mouse X");
        _dir = WorldForward.forward;
        transform.forward = Vector3.Slerp(transform.forward, _dir, 0.1f);

        //Quaternion deltaRotation = Quaternion.Euler(new Vector3(0, 1, 0) * Speed * orientation * Time.deltaTime);
        //Vector3 EulerRot = Vector3.Slerp(transform.forward, WorldForward.forward, 0.1f);

        //_rb.MoveRotation(Quaternion.Euler(EulerRot * Speed));
        //var rotación = _rb.rotation;
        //rotación = Quaternion.Euler(EulerRot);
        //_rb.MoveRotation(_rb.rotation * deltaRotation);

        //_rb.MoveRotation(_rb.rotation * Quaternion.Euler(_dir * Time.deltaTime));
    }


    IEnumerator Roll(Vector3 finalPos, float ScaledVelocity)
    {
        //Primero que nada avisamos que no podemos hacer otras acciones.
        canMove = false;

        //Debug - Init
        if (RollPosDebugObject)
        {
            RollPosDebugObject.SetActive(true);
            RollPosDebugObject.transform.position = finalPos;
        }


        rolling = true;
        

        while (rolling)
        {
            transform.forward = _dir;

            //Chequeamos si debemos seguir haciendo el roll.
            //Si mi posición es es igual a la posición objetivo rompo el ciclo.
            if (Vector3.Distance(transform.position, finalPos) < 0.5f )
            {
                rolling = false;
                break;
            }

            //Hacemos el desplazamiento
            transform.position = Vector3.Lerp(transform.position, finalPos, ScaledVelocity);
            yield return new WaitForEndOfFrame();
        }

        //Debug - Finit
        //if (RollPosDebugObject) RollPosDebugObject.SetActive(false);

        canMove = true;

        yield return new WaitForSeconds(0.1f);

        _dir = WorldForward.forward;
        transform.forward = _dir;
        //Volvemos a avisar que ya nos podemos mover.

        //Reactivamos el collider.
        _col.enabled = true;

        //Adicional poner el roll en enfriamiento.
    }

    /// <summary>
    /// Permite que esta unidad Recíba Daño.
    /// </summary>
    /// <param name="DamageStats"></param>
    public void GetDamage(params object[] DamageStats)
    {
        if (!invulnerable)
        {
            float Damage = (float)DamageStats[0];
            Health -= Damage;
            StartCoroutine(HurtFreeze());
        }
    }
    IEnumerator HurtFreeze()
    {
        canMove = false;
        _am.SetTrigger("hurted");

        yield return new WaitForSeconds(1f);

        //Muerte del Jugador
        if (!IsAlive)
        {
            canMove = false;
            _am.SetTrigger("died");
            print("Estas Muerto Wey");
        }
        else
            canMove = true;
    }

    /// <summary>
    /// Retorna las estadísticas de daño de esta Unidad.
    /// </summary>
    public object[] GetDamageStats()
    {
        return new object[1]{ currentDamage };
    }
}
