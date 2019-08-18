using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero : MonoBehaviour
{
    public float speed;
    Animator _am;
    float _dirX;
    float _dirY;
    Vector3 _dir;


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
        _dirX = Input.GetAxis("Horizontal");
        _dirY = Input.GetAxis("Vertical");
        _dir = Vector3.Normalize(new Vector3(_dirX, 0, _dirY));

        Debug.Log("Vertical" + Input.GetAxis("Vertical") + "Horizontal" + Input.GetAxis("Horizontal"));

        if (Mathf.Abs(Input.GetAxis("Horizontal"))
             >0.5 && Mathf.Abs(Input.GetAxis("Vertical")) >0.5)
        {
            transform.position += _dir * speed / 2;
            
        }
            

        transform.position += _dir * speed;

        _am.SetFloat("VelY", _dirY,0.2f,Time.deltaTime*2);
        _am.SetFloat("VelX", _dirX, 0.2f, Time.deltaTime * 2);
    }
}
