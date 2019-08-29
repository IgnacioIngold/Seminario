using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Entities;

[RequireComponent(typeof(Collider))]
public class CollisionDetection : MonoBehaviour
{
    public float layer = 1;
    public event Action OnCollide = delegate {};
    public Collider ObservedCollider;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == layer)
        {
            //print("EL colisionador choco con algo... " + other.gameObject.name);

            IKilleable killeable = other.gameObject.GetComponent<IKilleable>();
            if (killeable != null)
            {
                if (killeable.IsAlive && !killeable.invulnerable)
                {
                    print("EL colisionador choco con algo KILLEABLE: " + other.gameObject.name);
                    ObservedCollider.enabled = false;

                    OnCollide();
                    return;
                }
            }

            if (killeable == null)
            {
                //print("EL colisionador choco con algo que no es KILLEABLE: " + other.gameObject.name);
                ObservedCollider.enabled = false;
                OnCollide();
            }
        }
    }
}
