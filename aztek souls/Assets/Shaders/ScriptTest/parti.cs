using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class parti : MonoBehaviour
{
    public ParticleSystem particleLauncher;
    private void Awake()
    {
        
    }
    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            particleLauncher.Emit(1);

        }
        
    }

    
}
