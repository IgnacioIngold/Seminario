using UnityEngine;

public class FollowWorld : MonoBehaviour
{
    public Transform lookAt;
    public Vector3 Offset;
    Vector3 pos = Vector3.zero;

    private Camera cam;
    RectTransform myPos;
    // Start is called before the first frame update
    void Awake()
    {
        cam = Camera.main;
        myPos = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        pos = cam.WorldToScreenPoint(lookAt.position + Offset);
        myPos.position = pos + Offset;
    }
}
