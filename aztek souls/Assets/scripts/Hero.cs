using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using IA.StateMachine.Generic;
using Core.Entities;


[RequireComponent(typeof(Collider))]
public class Hero : MonoBehaviour, IKilleable
{
    [Header("Main Stats")]
    [SerializeField] float _hp = 100f;
    public float Health
    {
        get { return _hp; }
        set
        {
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

    private struct AttackNode
    {
        float duration;
        float buttonWindow;
    }

    private enum attackMode
    {
        idle,
        light1,
        light2,
        light3,
        parry
    }
    GenericFSM<attackMode> attack;
    Dictionary<attackMode, AttackNode> nodeStats;


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

    public bool IsAlive => _hp > 0;

    //----------------------------------------- Unity Functions -------------------------------------------------------

    void Awake()
    {
        _am = GetComponentInChildren<Animator>();
        _col = GetComponent<Collider>();

        //Starting Display
        HealthText.text = "Health: " + _hp;
        StaminaText.text = "Stamina: " + _st;

        //State Machine de modo de ataque.
        var idle = new State<attackMode>("Idle");
        var light1 = new State<attackMode>("Light1");
        var light2 = new State<attackMode>("Light2");
        var light3 = new State<attackMode>("Light3");
        var parry = new State<attackMode>("Parry");

        idle.OnUpdate += () =>
        {
            //Si presiono la tecla correspondiente entro al primer ataque.
        };

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
            Move(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"),Speed,false);
            if(Stamina >= 0 && Input.GetKey(KeyCode.LeftShift))
            {
                Move(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), RunSpeed, true);
            }
            
        }
        RotateCam();
        if (Stamina < MaxStamina)
            Stamina += StaminaRegeneration * Time.deltaTime;
    }

    private void FixedUpdate()
    {
        if (CameraBehaviour == CamType.Free)
            ProjectMouseToWorld();

        //Rool
        if (!rolling && Stamina >= rollCost && Input.GetKeyDown(KeyCode.Space))
            RoolExecute(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));


        if (rolling)
            transform.forward = Vector3.Slerp(transform.forward, _dir, 0.2f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.matrix *= Matrix4x4.Scale(new Vector3(1, 0, 1));
        Gizmos.DrawWireSphere(transform.position, rollRange);
    }

    //-----------------------------------------------------------------------------------------------------------------

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

                transform.forward = Vector3.Slerp(transform.forward, WorldForward.forward, 0.1f);

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

        //Volvemos a avisar que ya nos podemos mover.
        canMove = true;
        //Reactivamos el collider.
        _col.enabled = true;

        //Adicional poner el roll en enfriamiento.
    }

    public void GetDamage(params object[] DamageStats)
    {
        float Damage = (float)DamageStats[0];
        Health -= Damage;

        //Muerte del Jugador
        if (Health <= 0)
        {
            print("Estas Muerto Wey");
        }
    }
}
