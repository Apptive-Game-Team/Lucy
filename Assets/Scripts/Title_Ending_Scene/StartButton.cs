using UnityEngine;
using UnityEngine.SceneManagement;

namespace Title_Ending_Scene
{
    public class StartButton : MonoBehaviour
    {
        public void OnButtonClick()
        {
            Lucy.Presentation.GameSession.Instance.StartNewGame();
        }
    }
}
