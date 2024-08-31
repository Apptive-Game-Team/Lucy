using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SlicePuzzle
{
    public class CheckButton : MonoBehaviour
    {
        public GameObject buttonPanel;

        public Button[] buttons;
        private Vector2 checkPosition = new();

        public void OnButtonClick()
        {
            for (int i = 0; i < 9; i++)
            {
                checkPosition.Set(125+i%3*250,-125-i/3*250);
                if (buttons[i].GetComponent<RectTransform>().anchoredPosition != checkPosition)
                {
                    Debug.Log("오답");
                    return;
                }
            }
            Debug.Log("정답");
            PuzzleInteract.Instance.ClearPuzzle();
            return;
        }
    }
}
