using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamBehaviour2 : MonoBehaviour
{
    public Transform Target;
    public Transform LocalCam;
    public Vector3 camOffSet = Vector3.zero;
    public float transitionVelocity;
    public float RotationSpeed;

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
}
