using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class ActivateTimeline : MonoBehaviour
{
    public PlayableDirector mytimeline;
    bool HasBeenActive;
    public GameObject block;
    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<Player>() != null && !HasBeenActive)
        {
            mytimeline.Play();
            block.SetActive(true);
            HasBeenActive = true;
        }
    }
}
