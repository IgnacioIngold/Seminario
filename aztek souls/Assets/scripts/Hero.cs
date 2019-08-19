using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero : MonoBehaviour
{
    public float Speed;
    public GameObject DebugGameObject;
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

    Animator _am;
    Camera cam;
    float _dirX;
    float _dirY;
    Vector3 _dir;


    RaycastHit hit;
    Vector3 MousePosInWorld = Vector3.zero;


    void Awake()
    {
        _am = GetComponentInChildren<Animator>();
    }


    void Start()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        RotateCam();
    }

    private void FixedUpdate()
    {
        if (CameraBehaviour == CamType.Free)
            ProjectMouseToWorld();
    }

    /// <summary>
    /// Proyecta y coloca un Objeto en el mundo de acuerdo a la posición del Mouse.
    /// </summary>
    private void ProjectMouseToWorld()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            MousePosInWorld = hit.point;
            DebugGameObject.transform.position = hit.point;
        }

        #region Forward Setting
        if (MousePosInWorld != Vector3.zero)
        {
            Vector3 DesiredForward = (MousePosInWorld - transform.position).normalized;
            transform.forward = Vector3.Slerp(transform.forward, DesiredForward, 0.1f);
        } 
        #endregion
    }

    public void Move()
    {
        _dirX = Input.GetAxis("Horizontal");
        _dirY = Input.GetAxis("Vertical");
        _dir = WorldForward.forward * _dirY + WorldForward.right * _dirX;

        transform.position += _dir * Speed * Time.deltaTime;

        _am.SetFloat("VelY", _dirY);
        _am.SetFloat("VelX", _dirX);
    }

    public void RotateCam()
    {
        //Mover Rotar la cámara 
        switch (CameraBehaviour)
        {
            case CamType.Free:
                behaviour1.enabled = false;
                DebugGameObject.SetActive(true);

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

                DebugGameObject.SetActive(false);
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;

                transform.forward = Vector3.Slerp(transform.forward, WorldForward.forward, 0.1f);

                break;
            default:
                break;
        }
    }
}
