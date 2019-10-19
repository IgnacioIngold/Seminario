using Core.Entities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core;

public class Breackable : MonoBehaviour, IDamageable<HitData, HitResult>
{
    public GameObject BreackObject;
    
    public void Break()
    {
        Debug.Log("entre");
        Instantiate(BreackObject, transform.position, transform.rotation);
        
        Destroy(gameObject);
    }

    public void FeedHitResult(HitResult result) { print(string.Format("{0} Recibió Data de combate", gameObject.name)); }
    public HitData GetDamageStats() { return HitData.Empty(); }
    public HitResult Hit(HitData EntryData)
    {
        Break();
        return HitResult.Empty();
    }
}
