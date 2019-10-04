using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class replacement : MonoBehaviour
{
    public Shader XRayShader;
    private void OnEnable()
    {
        GetComponent<Camera>().SetReplacementShader(XRayShader, "XRay");
    }
}
