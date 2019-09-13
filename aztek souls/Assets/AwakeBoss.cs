using UnityEngine;
using UnityEngine.Events;

public class AwakeBoss : MonoBehaviour
{
    Player p;
    public UnityEvent OnPlayerIsInPlace;

    private void Awake()
    {
        p = FindObjectOfType<Player>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<Player>() != null)
            OnPlayerIsInPlace.Invoke();
    }
}
