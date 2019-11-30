using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class positions : MonoBehaviour
{
    public Shader shad;
    public Transform playerPos;
    public Material _mat;
    void Awake ()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        _mat.SetVector("_Pointposition",playerPos.position*Time.deltaTime);
    }
}
