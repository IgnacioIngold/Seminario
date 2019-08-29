using UnityEngine;

public class FollowWorld : MonoBehaviour
{
    public Transform lookAt;
    public Vector3 Offset;

    private Camera cam;
    // Start is called before the first frame update
    void Awake()
    {
        cam = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pos =cam.WorldToScreenPoint(lookAt.position + Offset);
        transform.position = pos;
    }
}
