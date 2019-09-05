using System;
using UnityEngine;

namespace Core
{
    public class SceneInterface : MonoBehaviour
    {
        public event Action OnGamePause = delegate { };

        public string PauseMenuButton;
        public GameObject PlayerObject;
        public BigCursed[] Enemies;
        public GameObject PauseMenuPanel;

        IPlayerController Player;

        private void Awake()
        {
            Player = PlayerObject.GetComponent<IPlayerController>();
        }

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

            Player.active = !state;

            foreach (BigCursed enemy in Enemies)
            {
                enemy.enabled = !state;
            }

            OnGamePause();
        }
    }

    public static class Context
    {
        public static bool Paused = false;
    }
}

