using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Hero : MonoBehaviour
{
    public float Speed = 4f;
    public float rollRange = 5f;
    public float rollVelocity = 10f;
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

    //----------------------------------------- Unity Functions -------------------------------------------------------

    void Awake()
    {
        _am = GetComponentInChildren<Animator>();
        _col = GetComponent<Collider>();
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
            Move(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            RotateCam();
        }
    }

    private void FixedUpdate()
    {
        if (CameraBehaviour == CamType.Free)
            ProjectMouseToWorld();

        //Rool
        if (!rolling && Input.GetKeyDown(KeyCode.Space))
            Rool(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));

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

    private void Rool(float AxisX, float AxisY)
    {
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

    public void Move(float AxisX, float AxisY)
    {
        _dir = WorldForward.forward * AxisY + WorldForward.right * AxisX;

        transform.position += _dir * Speed * Time.deltaTime;

        _am.SetFloat("VelY", AxisX);
        _am.SetFloat("VelX", AxisY);
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
}
