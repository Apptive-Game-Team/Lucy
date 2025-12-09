using UnityEngine;
using UnityEngine.UI;

namespace Slide_Puzzle
{
    public class StartButton : MonoBehaviour
    {
        public Canvas slicePuzzleCanvas;  // Ȱ��ȭ�� Canvas
        public Button startButton;        // Start ��ư

        void Start()
        {
            if (startButton != null)
            {
                // Start ��ư�� Ŭ�� �����ʸ� �߰��մϴ�.
                startButton.onClick.AddListener(OnButtonClick);
            }

            if (slicePuzzleCanvas != null)
            {
                // ó������ Canvas�� ��Ȱ��ȭ�մϴ�.
                slicePuzzleCanvas.gameObject.SetActive(false);
            }
        }

        public void OnButtonClick()
        {
            // ��ư�� Ŭ���Ǹ� Canvas�� Ȱ��ȭ�մϴ�.
            if (slicePuzzleCanvas != null)
            {
                slicePuzzleCanvas.gameObject.SetActive(true);
            }
        }
    }
}

