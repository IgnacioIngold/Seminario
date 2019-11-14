using UnityEngine;
using Core;
using Core.Entities;

public class Bullet : MonoBehaviour
{
    [SerializeField] GameObject _root;

    [Header("Collision Layers")]
    public int WorldCollisionLayer;
    public int ObstacleCollisionLayer;
    public int FloorCollisionLayer;
    public int DamageableCollisionLayer;

    [Header("Stats")]
    public float Duration;
    public float Damage;
    public float Speed;

    IDamageable<HitData, HitResult> _owner;
    GameObject _ownerGameObject;
    Transform _locator;

    private void Awake()
    {
        _locator = GetComponentInParent<Transform>();
    }

    public void SetOwner(GameObject Owner)
    {
        _ownerGameObject = Owner;
        _ownerGameObject.TryGetComponent(out _owner);
    }

    // Update is called once per frame
    void Update()
    {
        _locator.position += _locator.forward * Speed * Time.deltaTime;
        Duration -= Time.deltaTime;
        if (Duration <= 0)
            Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        //print(string.Format("Collisione con {0}, y su layer es {1}", other.gameObject.name, other.gameObject.layer));

        if (other.gameObject == _ownerGameObject) return;

        if (other.gameObject.layer == DamageableCollisionLayer)
        {
            IDamageable<HitData, HitResult> Target = other.GetComponentInParent<IDamageable<HitData, HitResult>>();

            if (Target != null && _owner != null)
                Target.Hit(new HitData() { Damage = Damage, BreakDefence = false });
        }

        Destroy(_root);
    }
}
