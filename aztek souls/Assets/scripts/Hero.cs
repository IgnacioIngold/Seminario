using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero : MonoBehaviour
{
    public float Speed;
    Animator _am;

    void Awake()
    {
        _am = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    public void Move()
    {
        float dirX = Input.GetAxis("Horizontal");
        float dirY = Input.GetAxis("Vertical");
        var dir = new Vector3(dirX,0,dirY);

        transform.position += dir * Speed;

        _am.SetFloat("VelY", dirY);
        _am.SetFloat("VelX", dirX);
    }
}
