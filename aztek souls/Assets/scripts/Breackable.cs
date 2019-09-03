using Core.Entities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breackable : MonoBehaviour, IKilleable
{
    public GameObject BreackObject;

    public bool IsAlive => true;

    public bool invulnerable => true;

    private void Awake()
    {
        //StartCoroutine(Breakpott());
    }
    public void Break()
    {
        Debug.Log("entre");
        Instantiate(BreackObject, transform.position, transform.rotation);
        Destroy(gameObject);
        
    }

    IEnumerator Breakpott()
    {
        Debug.Log("entre");
        yield return new WaitForSeconds(1f);
        Break();

    }

    public void GetDamage(params object[] DamageStats)
    {
        Break();
    }
}
