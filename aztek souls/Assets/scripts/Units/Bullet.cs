using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core;
using Core.Entities;

public class Bullet : MonoBehaviour
{
    public float Duration;
    public float Damage;
    public float Speed;
    private IDamageable<HitData, HitResult> _owner;
    private GameObject ownerGameObject;

    private Transform locator;

    private void Awake()
    {
        locator = GetComponentInParent<Transform>();
    }

    public void SetOwner(GameObject Owner)
    {
        ownerGameObject = Owner;
        ownerGameObject.TryGetComponent(out _owner);
    }

    // Update is called once per frame
    void Update()
    {
        locator.position += locator.forward * Speed * Time.deltaTime;
        Duration -= Time.deltaTime;
        if (Duration <= 0)
            Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == ownerGameObject) return;

        IDamageable<HitData, HitResult> Target = other.GetComponentInParent<IDamageable<HitData, HitResult>>();

        if (Target != null && _owner != null)
            Target.Hit(new HitData() { Damage = Damage, BreakDefence = false });
    }
}
