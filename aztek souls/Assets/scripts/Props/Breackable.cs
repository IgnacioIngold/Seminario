using Core.Entities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breackable : MonoBehaviour, IDamageable
{
    public GameObject BreackObject;

    public void Break()
    {
        Debug.Log("entre");
        Instantiate(BreackObject, transform.position, transform.rotation);
        Destroy(gameObject);
    }

    public void GetDamage(params object[] DamageStats)
    {
        Break();
    }
}
