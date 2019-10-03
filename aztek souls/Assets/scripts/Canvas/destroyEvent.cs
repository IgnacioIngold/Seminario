using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class destroyEvent : MonoBehaviour
{
    void DisableThis()
    {
        this.gameObject.SetActive(false);
    }
}
