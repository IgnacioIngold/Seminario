using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero : MonoBehaviour
{
    public float Speed;
    public GameObject DebugGameObject;
    public Transform WorldForward;

    Animator _am;
    float _dirX;
    float _dirY;
    Vector3 _dir;


    RaycastHit hit;
    Vector3 MousePosInWorld = Vector3.zero;


    void Awake()
    {
        _am = GetComponentInChildren<Animator>();
    }

    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    public void Move()
    {
        
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            MousePosInWorld = hit.point;
            DebugGameObject.transform.position = hit.point;
        }

        _dirX = Input.GetAxis("Horizontal");
        _dirY = Input.GetAxis("Vertical");
        _dir = WorldForward.forward * _dirY + WorldForward.right * _dirX;


        //if (Mathf.Abs(Input.GetAxis("Horizontal"))
        //     >0.5 && Mathf.Abs(Input.GetAxis("Vertical")) >0.5)
        //{
        //    transform.position += _dir * speed * Time.deltaTime;
        //}
        transform.position += _dir * Speed * Time.deltaTime;

        _am.SetFloat("VelY", _dirY);
        _am.SetFloat("VelX", _dirX);
    }

    private void OnDrawGizmos()
    {
        if (MousePosInWorld != Vector3.zero)
        {
            Vector3 DesiredForward = (MousePosInWorld - transform.position).normalized;
            transform.forward = Vector3.Slerp(transform.forward, DesiredForward, 0.1f);
        }
    }
}
