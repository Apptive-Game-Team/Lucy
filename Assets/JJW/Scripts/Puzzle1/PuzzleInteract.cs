using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Progress;

namespace SlicePuzzle
{
    public class PuzzleInteract : MonoBehaviour
    {
        public Canvas slicePuzzleCanvas;
        public GameObject text;

        private void Start()
        {
            text.SetActive(false);
            slicePuzzleCanvas.gameObject.SetActive(false);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.tag.Equals("Player"))
            {
                text.SetActive(true);
            }
        }
        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.gameObject.tag.Equals("Player"))
            {
                if (InputManager.Instance.GetKeyDown(ActionCode.Interaction))
                {
                    slicePuzzleCanvas.gameObject.SetActive(true);
                    Time.timeScale = 0f;
                }
            }
        }
        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject.tag.Equals("Player"))
            {
                text.SetActive(false);
            }
        }
    }
}

