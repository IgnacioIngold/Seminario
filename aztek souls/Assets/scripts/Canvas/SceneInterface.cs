using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Core.Definitions;

namespace Core
{
    public class SceneInterface : MonoBehaviour
    {
        public string PauseMenuButton;
        public Hero Player;
        public Cursed MainEnemy;
        public GameObject PauseMenuPanel;

        // Update is called once per frame
        void Update()
        {
            if (Input.GetButtonDown(PauseMenuButton))
                if (!Context.Paused)
                    PauseEverything(true);
                else
                    PauseEverything(false);
        }


        void PauseEverything(bool state)
        {
            Context.Paused = state;
            Time.timeScale = state ? 0 : 1;

            Cursor.visible = state;
            Cursor.lockState = state ? CursorLockMode.None : CursorLockMode.Locked;

            PauseMenuPanel.SetActive(state);

            Player.enabled = !state;
            MainEnemy.enabled = !state;
        }
    }

    public static class Context
    {
        public static bool Paused = false;
    }
}

