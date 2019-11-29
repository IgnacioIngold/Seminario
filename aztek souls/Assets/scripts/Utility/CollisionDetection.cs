using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Entities;

[RequireComponent(typeof(Collider))]
public class CollisionDetection : MonoBehaviour
{
    public int Layer_Wall = 0;
    public event Action OnCollide = delegate {};

    GameObject self;

#if UNITY_EDITOR

    public bool DebugMessages = true;

#endif

    private void Awake()
    {
        self = gameObject;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == Layer_Wall)
        {
            print("EL colisionador choco con algo... " + other.gameObject.name);

            if (other.gameObject == self) return;

            IKilleable killeable = other.gameObject.GetComponent<IKilleable>();
            if (killeable != null)
            {
                if (killeable.IsAlive && !killeable.invulnerable)
                {
#if UNITY_EDITOR
                    if (DebugMessages)
                        print("EL colisionador choco con algo KILLEABLE: " + other.gameObject.name); 
#endif
                    OnCollide();
                    return;
                }
            }

#if UNITY_EDITOR
            if (DebugMessages)
                print("EL colisionador choco con algo que no es KILLEABLE: " + other.gameObject.name);
#endif
            OnCollide();
        }
    }
}
