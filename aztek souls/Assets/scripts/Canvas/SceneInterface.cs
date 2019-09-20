using System;
using UnityEngine;
using UnityEngine.UI;

namespace Core
{
    public class SceneInterface : MonoBehaviour
    {
        public event Action OnGamePause = delegate { };
        public event Action OnEnableDisableLevelUpPanel = delegate { };

        public string PauseMenuButton;
        public string LevelUpMenuButton;
        public BaseUnit[] Enemies;
        public Image FadeBackGround;
        public GameObject PauseMenuPanel;
        public LevelUpPanel LevelUpPanel;

        Player Player;
        MainCamBehaviour mainCamB;

        private void Awake()
        {
            Player = FindObjectOfType<Player>();
            mainCamB = Camera.main.GetComponentInParent<MainCamBehaviour>();

            //Relleno mi array de enemigos.
            Enemies = FindObjectsOfType<BaseUnit>();

            LevelUpPanel.OnCancel += () => { EnableDisableLevelUpPanel(false); };
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetButtonDown(PauseMenuButton))
                if (!Context.Paused)
                    PauseEverything(true);
                else
                    PauseEverything(false);

            if (Input.GetButtonDown(LevelUpMenuButton))
                if (!Context.LevelupPanel)
                    EnableDisableLevelUpPanel(true);
                else
                    EnableDisableLevelUpPanel(false);
        }

        public void EnableDisableLevelUpPanel(bool active)
        {
            Player.active = !active;
            mainCamB.EnableRotation(!active);
            EnableDisableEnemies(!active);

            LevelUpPanel.gameObject.SetActive(active);
            LevelUpPanel.LoadData();

            Cursor.visible = active;
            Cursor.lockState = active ? CursorLockMode.None : CursorLockMode.Locked;

            OnEnableDisableLevelUpPanel();
        }

        void PauseEverything(bool paused)
        {
            Context.Paused = paused;
            Time.timeScale = paused ? 0 : 1;

            Cursor.visible = paused;
            Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;

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

