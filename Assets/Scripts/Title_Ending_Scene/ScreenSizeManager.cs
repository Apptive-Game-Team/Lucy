using UnityEngine;
using UnityEngine.SceneManagement;

namespace Title_Ending_Scene
{
    public class ScreenSizeManager : MonoBehaviour
    {
        public int titleWidth = 1080;
        public int titleHeight = 1080;
        public int defaultWidth = 1920;
        public int defaultHeight = 1080;
        public bool fullscreen = false;

        void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;

            OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name == "Title")
            {
                Screen.SetResolution(titleWidth, titleHeight, fullscreen);
            }
            else
            {
                Screen.SetResolution(defaultWidth, defaultHeight, fullscreen);
            }
        }

        void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}
