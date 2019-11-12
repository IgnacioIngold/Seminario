using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugTime : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad0))
            Time.timeScale = 1;
        if (Input.GetKeyDown(KeyCode.Keypad1))
            Time.timeScale = 0.1f;
        if (Input.GetKeyDown(KeyCode.Keypad2))
            Time.timeScale = 0.2f;
        if (Input.GetKeyDown(KeyCode.Keypad3))
            Time.timeScale = 0.3f;
        if (Input.GetKeyDown(KeyCode.Keypad4))
            Time.timeScale = 0.4f;
        if (Input.GetKeyDown(KeyCode.Keypad5))
            Time.timeScale = 0.5f;
        if (Input.GetKeyDown(KeyCode.Keypad6))
            Time.timeScale = 0.6f;
        if (Input.GetKeyDown(KeyCode.Keypad7))
            Time.timeScale = 0.7f;
        if (Input.GetKeyDown(KeyCode.Keypad8))
            Time.timeScale = 0.8f;
        if (Input.GetKeyDown(KeyCode.Keypad9))
            Time.timeScale = 0.9f;
    }
}
