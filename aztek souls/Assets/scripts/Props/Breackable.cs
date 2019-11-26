using Core.Entities;
using UnityEngine;
using Core;

public class Breackable : MonoBehaviour, IDamageable<HitData, HitResult>
{
    public GameObject BreackObject;
    public int bloodEarnedForBreack = 100;

    public void Break()
    {
        Debug.Log("entre");
        Instantiate(BreackObject, transform.position, transform.rotation);
        Destroy(gameObject);
    }

    public void GetHitResult(HitResult result)
    {
        print(string.Format("{0} Recibió Data de combate", gameObject.name));
    }
    public HitData DamageStats() { return HitData.Default(); }
    public HitResult Hit(HitData EntryData)
    {
        HitResult result = new HitResult() { bloodEarned = bloodEarnedForBreack, HitConnected = true, HitBlocked = false, TargetEliminated = true };
        Break();
        return result;
    }
}
