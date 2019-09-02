using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breackable : MonoBehaviour
{
    public GameObject BreackObject;

    private void Awake()
    {
        StartCoroutine(Breakpott());
    }
    public void Break()
    {
        Instantiate(BreackObject, transform.position, transform.rotation);
        Destroy(gameObject);
    }

    IEnumerator Breakpott()
    {
        Debug.Log("entre");
        yield return new WaitForSeconds(1f);
        Break();

    }
   
}
