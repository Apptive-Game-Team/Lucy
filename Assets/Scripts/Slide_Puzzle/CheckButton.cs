using UnityEngine;
using UnityEngine.UI;

namespace Slide_Puzzle
{
    public class CheckButton : MonoBehaviour
    {
        public GameObject buttonPanel;

        public Button[] buttons;
        private Vector2 checkPosition = new();

        public void OnButtonClick()
        {
            if (buttons == null || buttons.Length != 9) return;
            for (int i = 0; i < 9; i++)
            {
                if (buttons[i] == null) return;
                checkPosition.Set(125+i%3*250,-125-i/3*250);
                if (buttons[i].GetComponent<RectTransform>().anchoredPosition != checkPosition)
                {
                    Debug.Log("오답");
                    return;
                }
            }
            Debug.Log("정답");
            if (PuzzleInteract.Instance != null) PuzzleInteract.Instance.ClearPuzzle();
            return;
        }
    }
}
