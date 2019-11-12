using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugTime : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad0))
            ChangeTimeScale(1f);
        if (Input.GetKeyDown(KeyCode.Keypad1))
            ChangeTimeScale(0.1f);
        if (Input.GetKeyDown(KeyCode.Keypad2))
            ChangeTimeScale(0.2f);
        if (Input.GetKeyDown(KeyCode.Keypad3))
            ChangeTimeScale(0.3f);
        if (Input.GetKeyDown(KeyCode.Keypad4))
            ChangeTimeScale(0.4f);
        if (Input.GetKeyDown(KeyCode.Keypad5))
            ChangeTimeScale(0.5f);
        if (Input.GetKeyDown(KeyCode.Keypad6))
            ChangeTimeScale(0.6f);
        if (Input.GetKeyDown(KeyCode.Keypad7))
            ChangeTimeScale(0.7f);
        if (Input.GetKeyDown(KeyCode.Keypad8))
            ChangeTimeScale(0.8f);
        if (Input.GetKeyDown(KeyCode.Keypad9))
            ChangeTimeScale(0.9f);
    }

    void ChangeTimeScale(float scale)
    {
        Time.timeScale = scale;
        print(string.Format("Time Scale modificado\nEscala actual es {0}", Time.timeScale));
    }
}
