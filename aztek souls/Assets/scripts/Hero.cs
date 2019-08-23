using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Core.Entities;


[RequireComponent(typeof(Collider))]
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
            HealthText.text = "Health: " + (int)_hp;
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
            //Display Value
            _st = value;
            StaminaText.text = "Stamina: " + (int)_st;
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

    public Text HealthText;
    public Text StaminaText;
    public GameObject MousePosDebugObject;
    public GameObject RollPosDebugObject;
    public LayerMask RollObstacles;
    public Transform WorldForward;

    #region DebugCamara

    public enum CamType
    {
        Free,
        Fixed
    }
    public CamType CameraBehaviour;
    public CameraBehaviour behaviour1;
    public CamBehaviour2 behaviour2;

    #endregion

    //----------------Private Members---------------

    Animator _am;
    Camera cam;
    Collider _col;

    bool canMove = true;
    bool rolling = false;
    float _dirX;
    float _dirY;
    Vector3 _dir;


    RaycastHit ProjectMouseToWorld_RayHit;
    Vector3 MousePosInWorld = Vector3.zero;
    private bool Attacking;
    private int ComboCounter;

    public bool IsAlive => _hp > 0;
    int[] executionStack = new int[3];

    //----------------------------------------- Unity Functions -------------------------------------------------------

    void Awake()
    {
        _am = GetComponentInChildren<Animator>();
        _col = GetComponent<Collider>();
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
        if (canMove)
        {
            //TODO: Acá tenemos que hacer que la key se libere al momento de iniciar correr, solo cuando volvemos a pulsar el shift
            //Deberíamos reanudar la acción de correr.

            if (Stamina > 0f && Input.GetKey(KeyCode.LeftShift))
            {
                Move(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), RunSpeed, true);
                Stamina -= 20f * Time.deltaTime;
            }
            else
                Move(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"),Speed,false);
        }

        RotateCam();
        if (Stamina < MaxStamina)
            Stamina += StaminaRegeneration * Time.deltaTime;

        if (!Attacking && Input.GetButtonDown(AttackButton))
            Attack();
    }

    private void FixedUpdate()
    {
        if (CameraBehaviour == CamType.Free)
            ProjectMouseToWorld();

        //Rool
        if (!rolling && Stamina >= rollCost && Input.GetKeyDown(KeyCode.Space))
            RoolExecute(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));


        if (rolling)
            transform.forward = _dir;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.matrix *= Matrix4x4.Scale(new Vector3(1, 0, 1));
        Gizmos.DrawWireSphere(transform.position, rollRange);
    }

    //-----------------------------------------------------------------------------------------------------------------
    private void Attack()
    {
        //Rellenar el Stack.
        executionStack = new int[] { 1, 0, 0 };

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
        print("Fin del Combo");
    }

    public void ExecuteAnimation(int animationID)
    {
        switch (animationID)
        {
            case 1:
                //Attaque básico.
                print("Ataque básico");
                break;
            case 2:
                //Combo 1
                print("Combo 1");
                break;
            case 3:
                //Combo 2
                print("Combo 2");
                //Añado el bono.
                break;

            default:
                break;
        }
    }


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


    /// <summary>
    /// Proyecta y coloca un Objeto en el mundo de acuerdo a la posición del Mouse.
    /// </summary>
    private void ProjectMouseToWorld()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out ProjectMouseToWorld_RayHit))
        {
            MousePosInWorld = ProjectMouseToWorld_RayHit.point;
            MousePosDebugObject.transform.position = ProjectMouseToWorld_RayHit.point;
        }

        #region Forward Setting
        if (MousePosInWorld != Vector3.zero)
        {
            Vector3 DesiredForward = (MousePosInWorld - transform.position).normalized;
            transform.forward = Vector3.Slerp(transform.forward, DesiredForward, 0.1f);
        } 
        #endregion
    }

    public void Move(float AxisX, float AxisY,float s,bool running)
    {
        _dir = WorldForward.forward * AxisY + WorldForward.right * AxisX;

        //Correcting Forward.
        transform.forward = Vector3.Slerp(transform.forward, WorldForward.forward, 0.1f);

        transform.position += _dir * s * Time.deltaTime;

        _am.SetFloat("VelY", AxisX);
        _am.SetFloat("VelX", AxisY);

        _am.SetBool("Running", running);
    }
   

    public void RotateCam()
    {
        //Mover Rotar la cámara 
        switch (CameraBehaviour)
        {
            case CamType.Free:
                behaviour1.enabled = false;
                MousePosDebugObject.SetActive(true);

                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;

                behaviour2.enabled = true;
                behaviour2.MoveCamera();
                behaviour2.RotateCamera("CameraRotation");
                break;

            case CamType.Fixed:
                behaviour2.enabled = false;

                behaviour1.enabled = true;
                behaviour1.MoveCamera();
                behaviour1.RotateCamera("Mouse X");

                MousePosDebugObject.SetActive(false);
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                break;
            default:
                break;
        }
    }


    IEnumerator Roll(Vector3 finalPos, float ScaledVelocity)
    {
        //Primero que nada avisamos que no podemos hacer otras acciones.
        canMove = false;
        //Desactivamos el collider.
        _col.enabled = false;

        //Debug - Init
        RollPosDebugObject.SetActive(true);
        RollPosDebugObject.transform.position = finalPos;


        rolling = true;
        while (rolling)
        {
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
        RollPosDebugObject.SetActive(false);

        canMove = true;
        yield return new WaitForSeconds(0.5f);

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
        float Damage = (float)DamageStats[0];
        Health -= Damage;

        _am.SetTrigger("hurted");

        //Muerte del Jugador
        if (Health <= 0)
        {
            print("Estas Muerto Wey");
            _am.SetTrigger("died");
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
