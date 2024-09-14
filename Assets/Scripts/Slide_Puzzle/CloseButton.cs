using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SlicePuzzle
{
    public class CloseButton : MonoBehaviour
    {
        public Canvas slicePuzzleCanvas;

        public void OnButtonClick()
        {
            // 버튼이 클릭되면 Canvas를 활성화합니다.
            if (slicePuzzleCanvas != null)
            {
                slicePuzzleCanvas.gameObject.SetActive(false);
                Time.timeScale = 1f;
            }
        }
    }
}

