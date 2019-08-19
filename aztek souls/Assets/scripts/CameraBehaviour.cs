using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CameraBehaviour : MonoBehaviour
{
    public Transform Target;
    public Transform LocalCam;
    public Vector3 camOffSet = Vector3.zero;
    public float transitionVelocity;
    public float RotationSpeed;

    //TODO: Rotación de cámara

    // Update is called once per frame
    void Update()
    {
        if (Target != null)
        {
            transform.position = Vector3.Slerp(transform.position, Target.position, transitionVelocity); //La posición del pivot es el mismo que del target.

            if (Input.GetButton("CameraRotation"))
            {
                float value = Input.GetAxisRaw("CameraRotation");
                print("Esta funcando Wey" + value);
                transform.Rotate(Vector3.up, RotationSpeed * value * Time.deltaTime, Space.Self);
            }
        }
    }
}
