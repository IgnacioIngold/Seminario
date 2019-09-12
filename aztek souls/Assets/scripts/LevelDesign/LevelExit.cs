using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Definitions;

public class LevelExit : MonoBehaviour
{
    public GameObject PlayerObject;
    public SceneWrapp levelWrap;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == PlayerObject)
        {
            print("LLegué al final del level");
            levelWrap.ChangeToScene(0);
        }
    }
}
