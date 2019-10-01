using System;
using UnityEngine;
using UnityEngine.UI;

namespace Core
{
    public class SceneInterface : MonoBehaviour
    {
        public event Action OnGamePause = delegate { };

        public string PauseMenuButton;
        public string LevelUpMenuButton;
        public BaseUnit[] Enemies;
        public Image FadeBackGround;
        public GameObject PauseMenuPanel;
        public LevelUpPanel LevelUpPanel;

        Player Player;
        MainCamBehaviour mainCamB;
        bool LevelUpPanelWasOpened = false;

        private void Awake()
        {
            Player = FindObjectOfType<Player>();
            mainCamB = Camera.main.GetComponentInParent<MainCamBehaviour>();

            //Relleno mi array de enemigos.
            Enemies = FindObjectsOfType<BaseUnit>();
            if (LevelUpPanel != null)
            {
                LevelUpPanel.OnAccept += () => { EnableDisableLevelUpPanel(false); };
                LevelUpPanel.OnCancel += () => { EnableDisableLevelUpPanel(false); };
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (Context.Paused || Context.LevelupPanel)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;

                FadeBackGround.gameObject.SetActive(true);
            }

            if (!Context.Paused && !Context.LevelupPanel)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;

                FadeBackGround.gameObject.SetActive(false);
            }

            if (Input.GetButtonDown(PauseMenuButton))
                if (!Context.Paused)
                {
                    PauseEverything(true);
                    if (Context.LevelupPanel) EnableDisableLevelUpPanel(false);
                }
                else
                {
                    PauseEverything(false);
                    if (LevelUpPanelWasOpened) EnableDisableLevelUpPanel(true);
                }


            if (!Context.Paused)
            {
                if (Input.GetButtonDown(LevelUpMenuButton))
                    if (!Context.LevelupPanel)
                    {
                        LevelUpPanelWasOpened = true;
                        EnableDisableLevelUpPanel(true, true);
                    }
                    else
                    {
                        LevelUpPanelWasOpened = false;
                        EnableDisableLevelUpPanel(false, true);
                    }
            }
            else
                EnableDisableLevelUpPanel(false);
        }

        public void EnableDisableLevelUpPanel(bool active, bool loadData = false)
        {
            Context.LevelupPanel = active;

            Player.active = !active;
            mainCamB.EnableRotation(!active);
            EnableDisableEnemies(!active);

            LevelUpPanel.gameObject.SetActive(active);
            if(loadData) LevelUpPanel.LoadData();
        }

        void PauseEverything(bool paused)
        {
            Context.Paused = paused;
            Time.timeScale = paused ? 0 : 1;

            PauseMenuPanel.SetActive(paused);

            Player.active = !paused;

            EnableDisableEnemies(!paused);

            OnGamePause();
        }

        void EnableDisableEnemies(bool state)
        {
            foreach (BaseUnit enemy in Enemies)
                enemy.enabled = state;
        }
    }

    public static class Context
    {
        public static bool Paused = false;
        public static bool LevelupPanel = false;
    }
}

