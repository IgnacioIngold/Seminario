using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Este script lo vamos a utlizar para los cambios de escena principalmente
/// </summary>

namespace Core.Definitions
{
    public class SceneWrapp : MonoBehaviour
    {
        public void ChangeToScene(int index)
        {
            SceneManager.LoadScene(index);
        }

        public void CloseGame()
        {
            Application.Quit();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        }
    }

    /// <summary>
    /// Contiene los índices de cada escena con un nombre intuitivo
    /// </summary>
    public static class Definitions
    {
        public const short MainMenu = 0;
        public const short Credits = 1;
        public const short Level1 = 2;
        public const short WinScene = 5;
        public const short DefeatScene = 6;
    }
}
