using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class LAWEA : MonoBehaviour, CamTarget
{
    public event Action OnPositionIsUpdated = delegate { };

    public Transform AxisOrientation;
    public Vector3 _dir = Vector3.zero;

    public float Stamina = 100f;
    public float walkSpeed = 4f;

    public float runSpeed = 20f;
    public float runCost = 20;

    public bool _running = false;
    public bool canMove = true;
    public bool _moving = false;
    public bool _roll = false;


    Rigidbody _rb;
    public bool _rolling;
    public float rollCost;
    public Vector3 _rollDir;
    public float rollSpeed;
    public float rollDuration;


    // Start is called before the first frame update
    void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {

            float AxisY = Input.GetAxis("Vertical");
            float AxisX = Input.GetAxis("Horizontal");

            _rollDir = AxisOrientation.forward * AxisY + AxisOrientation.right * AxisX;

            StartCoroutine(Roll());
            return;
        }

        if (canMove)
        {
            if (Input.GetButtonDown("Run")) _running = true;
            if (Input.GetButtonUp("Run")) _running = false;

            if (Input.GetButton("Horizontal") || Input.GetButton("Vertical"))
            {
                _moving = true;
                float AxisY = Input.GetAxis("Vertical");
                float AxisX = Input.GetAxis("Horizontal");
                //WorldForward es un objeto que usamos para determinar nuestra orientacón en el mundo.
                //En este caso se trata del objeto Pivot de la cámara.

                _dir = AxisOrientation.forward * AxisY + AxisOrientation.right * AxisX;
            }
            else _moving = false;
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
        _rb.MovePosition(transform.position + (_dir.normalized * movementSpeed * Time.deltaTime));
        OnPositionIsUpdated();
    }

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
        yield return new WaitForSeconds(0.1f);

        //End of Roll.
        _rolling = false;
        canMove = true;                      // Avisamos que ya nos podemos mover.
        //_dir = WorldForward.forward;       // Calculamos nuestra orientación...
        //transform.forward = _dir;          // Seteamos la orientación como se debe.
        //_col.enabled = true;                 // Reactivamos el collider.
        //States.Feed(CharacterState.idle);

        // Adicional poner el roll en enfriamiento.
    }
}
