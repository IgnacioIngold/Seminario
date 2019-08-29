using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class parti : MonoBehaviour
{
    public GameObject parent;
    public GameObject particleInstace;

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            Instantiate(particleInstace, parent.transform);
        }
    }
}
