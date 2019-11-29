using UnityEngine;

public class StairTrigger : MonoBehaviour
{
    Player p;
    public Transform StairOrientation;
    public string PlayerTag;

    private void Awake()
    {
        p = GameObject.FindObjectOfType<Player>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == PlayerTag)
        {
            p._isInStair = true;
            p.stairOrientation = StairOrientation;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == PlayerTag)
        {
            p._isInStair = false;
            p.stairOrientation = StairOrientation;
        }
    }
}
