using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Behaviour Original de la cámara.
public class CameraBehaviour : MonoBehaviour
{
    public Transform Target;
    public Transform LocalCam;

    public Vector3 camOffSet = Vector3.zero;

    public float transitionVelocity = 0.5f;
    public float RotationSpeed = 10f; 


    // Update is called once per frame
    //void Update()
    //{
    //    MoveCamera();
    //    RotateCamera("CameraRotation");
    //}

    public void MoveCamera()
    {
        //La posición del pivot es el mismo que del target.
        if (Target != null)
            transform.position = Vector3.Slerp(transform.position, Target.position, transitionVelocity);
    }

    public void RotateCamera(string CameraAxis)
    {
        if (Target != null)
        {
            //print("Behaviour 2");
            float value = Input.GetAxisRaw(CameraAxis);
            transform.Rotate(Vector3.up, RotationSpeed * value * Time.deltaTime, Space.Self);
        }
    }
}
