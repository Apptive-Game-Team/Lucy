using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Progress;

namespace SlicePuzzle
{
    public class PuzzleInteract : MonoBehaviour
    {
        public static PuzzleInteract Instance {get; private set;}
        public Canvas slicePuzzleCanvas;
        public GameObject clearPuzzleImage;
        public GameObject text;
        public GameObject key_3F;
        public bool isClear = false;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this);
            }
        }

        private void Start()
        {
            text.SetActive(false);
            slicePuzzleCanvas.gameObject.SetActive(false);
            //clearPuzzleImage.SetActive(false);
            key_3F.SetActive(false);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!isClear)
            {
                if (other.gameObject.tag.Equals("Player"))
                {
                    text.SetActive(true);
                }
            }
        }
        private void OnTriggerStay2D(Collider2D other)
        {
            if (other.gameObject.tag.Equals("Player"))
            {
                if (InputManager.Instance.GetKeyDown(ActionCode.Interaction))
                {
                    if (!isClear)
                    {
                        slicePuzzleCanvas.gameObject.SetActive(true);
                        Time.timeScale = 0f;
                    }
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

        public void ClearPuzzle()
        {
            isClear = true;
            slicePuzzleCanvas.gameObject.SetActive(false);
            //clearPuzzleImage.SetActive(true);
            key_3F.SetActive(true);
            StartCoroutine(EquipKey());
        }

        private IEnumerator EquipKey()
        {
            yield return new WaitUntil(() => InputManager.Instance.GetKeyDown(ActionCode.Interaction));
            Time.timeScale = 1f;
        }
    }
}

