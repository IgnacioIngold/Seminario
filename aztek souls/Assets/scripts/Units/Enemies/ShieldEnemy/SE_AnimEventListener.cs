﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SE_AnimEventListener : MonoBehaviour
{
    public Collider coll;
    public ParticleSystem marker;

    private void Awake()
    {
        //coll = GetComponent<Collider>();
    }

    public void EnableDamage()
    {
        coll.enabled = true;
    }

    public void DisableDamage()
    {
        coll.enabled = false;
    }
    public void hiteable()
    {
        marker.Play();
    }
}
