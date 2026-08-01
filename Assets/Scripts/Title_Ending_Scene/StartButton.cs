using UnityEngine;
using UnityEngine.SceneManagement;

namespace Title_Ending_Scene
{
    public class StartButton : MonoBehaviour
    {
        // Serialized so a renamed scene is fixed in the inspector instead of in code.
        // The old hardcoded "Test240819" scene no longer exists in the project.
        [SerializeField] private string gameSceneName = "0101_09.18";

        public void OnButtonClick()
        {
            if (Application.CanStreamedLevelBeLoaded(gameSceneName))
            {
                SceneManager.LoadScene(gameSceneName);
                return;
            }
            Debug.LogError($"Scene '{gameSceneName}' is not in Build Settings");
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
