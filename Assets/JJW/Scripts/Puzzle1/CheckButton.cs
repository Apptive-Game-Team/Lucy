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

        public void OnButtonClick()
        {
            for (int i = 0; i < 9; i++)
            {
                if (buttons[i].GetComponent<RectTransform>().anchoredPosition != new Vector2(125+(i%3)*250,-125-i/3*250))
                {
                    Debug.Log("오답");
                    return;
                }
            }
            Debug.Log("정답");
            return;
        }
    }
}
