using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Definitions;

public class LevelExit : MonoBehaviour
{
    [SerializeField]GameObject PlayerObject;
    [SerializeField]SceneWrapp levelWrap;

    private void Awake()
    {
        PlayerObject = FindObjectOfType<Player>().gameObject;
        levelWrap = FindObjectOfType<SceneWrapp>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == PlayerObject)
        {
            print("LLegué al final del level");
            levelWrap.ChangeToScene(0);
        }
    }
}
