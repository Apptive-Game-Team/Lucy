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
            // ��ư�� Ŭ���Ǹ� Canvas�� Ȱ��ȭ�մϴ�.
            if (slicePuzzleCanvas != null)
            {
                slicePuzzleCanvas.gameObject.SetActive(false);
                Time.timeScale = 1f;
            }
        }
    }
}

