using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackNode : MonoBehaviour
{
    public Rect myRect;
    public string nodeName;
    public string dialogo;
    public float duration;
    public List<AttackNode> connected;
    public bool OverNode { get; private set; }

    public AttackNode(Vector2 position, float Width, float Height, string name)
    {
        myRect = new Rect(position.x, position.y, Width, Height);
        nodeName = name;
    }
    public AttackNode(Rect transform, string name)
    {
        myRect = transform;
        nodeName = name;
    }

    public void CheckMouse(Event ev, Vector2 pan)
    {
        OverNode = myRect.Contains(ev.mousePosition - pan);
    }

}
