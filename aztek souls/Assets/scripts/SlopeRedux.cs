using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SlopeRedux : MonoBehaviour
{
    public float gravityIncrease = 100;

    private void OnTriggerEnter(Collider other)
    {
        var rb = other.attachedRigidbody;
        if (rb != null) rb.velocity = Vector3.zero;
    }
    private void OnTriggerExit(Collider other)
    {
        //var rb = other.attachedRigidbody;
    }
}
