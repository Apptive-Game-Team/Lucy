using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SlicePuzzle
{
    public class StartButton : MonoBehaviour
    {
        public Canvas slicePuzzleCanvas;  // 활성화할 Canvas
        public Button startButton;        // Start 버튼

        void Start()
        {
            if (startButton != null)
            {
                // Start 버튼에 클릭 리스너를 추가합니다.
                startButton.onClick.AddListener(OnButtonClick);
            }

            if (slicePuzzleCanvas != null)
            {
                // 처음에는 Canvas를 비활성화합니다.
                slicePuzzleCanvas.gameObject.SetActive(false);
            }
        }

        public void OnButtonClick()
        {
            // 버튼이 클릭되면 Canvas를 활성화합니다.
            if (slicePuzzleCanvas != null)
            {
                slicePuzzleCanvas.gameObject.SetActive(true);
            }
        }
    }
}

