﻿using System;
using System.Collections;
using UnityEngine;

//Behaviour Adicional para la cámara.
public class MainCamBehaviour : MonoBehaviour
{
    public string MouseAxis;
    public Transform LocalCam;
    [Tooltip("La diferencia de posición global que mantiene la cámara real de su pivot.")]
    public Vector3 camOffSet = Vector3.zero;
    public float transitionVelocity = 1f;
    public float RotationSpeed = 30;

    Transform Target;

    private void Awake()
    {
        Target = GameObject.FindGameObjectWithTag("Player").transform;
        if (camOffSet != Vector3.zero)
            LocalCam.localPosition = camOffSet;
    }

    private void Start()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void FixedUpdate()
    {
        MoveCamera();
        RotateCamera(MouseAxis);
    }

    public void MoveCamera()
    {
        //La posición del pivot es el mismo que del target.
        if (Target != null)
            transform.position = Vector3.Slerp(transform.position, Target.position, transitionVelocity);
    }

    public void RotateCamera(string MouseAxis)
    {
        if (Target != null)
        {
            //En este caso quiero que rote constantemente usando el mouse.
            float value = Input.GetAxisRaw(MouseAxis);
            transform.Rotate(Vector3.up, RotationSpeed * value * Time.deltaTime, Space.Self);
        }
    }

    [ContextMenu("Match Camera Position to Offset")]
    public void SetInitialPosition()
    {
        if (camOffSet != Vector3.zero)
            LocalCam.localPosition = camOffSet;
    }

    public Transform getPivotPosition()
    {
        return transform;
    }

}