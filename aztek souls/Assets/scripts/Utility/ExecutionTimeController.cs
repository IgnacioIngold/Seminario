using UnityEngine;

public class ExecutionTimeController : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad0))
            ChangeTimeScale(0f);
        if (Input.GetKeyDown(KeyCode.Alpha0))
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

    public void ChangeTimeScale(float Ammount)
    {
        Time.timeScale = Ammount;
        Debug.LogWarning("La escala de tiempo ha sido modificada a :" + Time.timeScale);
    }
}
