using UnityEngine;
using UnityEngine.SceneManagement;

namespace Title_Ending_Scene
{
    public class StartButton : MonoBehaviour
    {
        public void OnButtonClick()
        {
            SceneManager.LoadScene("Test240819");
        }
    }
}
