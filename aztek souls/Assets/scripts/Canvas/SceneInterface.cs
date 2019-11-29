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

            Action EnableG = () => { EnableGameplay(true); };
            LevelUpPanel.OnAccept += EnableG;
            LevelUpPanel.OnCancel += EnableG;

            //Relleno mi array de enemigos.
            Enemies = FindObjectsOfType<BaseUnit>();
            if (LevelUpPanel != null)
                LevelUpPanel.SetAndLoad();
        }

        // Update is called once per frame
        void Update()
        {
            //Muestro o oculto el cursor.
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

            //Pausa.
            if (Input.GetButtonDown(PauseMenuButton))
            {
                Context.Paused = !Context.Paused;
                PauseEverything(Context.Paused);

                if (Context.Paused && LevelUpPanelWasOpened)
                    EnableDisableLevelUpPanel(false, false);
                else if(!Context.Paused && LevelUpPanelWasOpened)
                    EnableDisableLevelUpPanel(true, false);
            }

            //Level Up Panel.
            //if (Input.GetButtonDown(LevelUpMenuButton) && !Context.Paused)
            //{
            //    Context.LevelupPanel = !Context.LevelupPanel;
            //    LevelUpPanelWasOpened = Context.LevelupPanel;

            //    EnableDisableLevelUpPanel(Context.LevelupPanel);
            //}
        }

        /// <summary>
        /// Activa o Desactiva el Panel de Lvl Up.
        /// </summary>
        /// <param name="enable">True para habiltar, False para deshabilitar.</param>
        /// <param name="resetData">Si queremos que se reinicien todos los datos o no.</param>
        public void EnableDisableLevelUpPanel(bool enable, bool resetData = true)
        {
            EnableGameplay(!enable);

            if (enable)
            {
                if (resetData) LevelUpPanel.OpenAndLoad(); 
                else LevelUpPanel.Open();
            }
            else
            {
                if (resetData) LevelUpPanel.CancelAndClose(); 
                else LevelUpPanel.Close();
            }
        }

        void PauseEverything(bool paused)
        {
            Context.Paused = paused;
            Time.timeScale = paused ? 0 : 1;

            PauseMenuPanel.SetActive(paused);

            EnableGameplay(!paused);

            OnGamePause();
        }

        void EnableGameplay(bool enabled)
        {
            Player.active = enabled;
            mainCamB.EnableRotation(enabled);

            foreach (BaseUnit enemy in Enemies)
                enemy.enabled = enabled;
        }
    }

    public static class Context
    {
        public static bool Paused = false;
        public static bool LevelupPanel = false;
    }
}

