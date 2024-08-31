using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Progress;

namespace SlicePuzzle
{
    public class PuzzleInteract : InteractableObject
    {
        public static PuzzleInteract Instance {get; private set;}
        public Canvas slicePuzzleCanvas;
        public GameObject clearPuzzleImage;
        public GameObject text;
        public ItemData key_3F;
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
        }
        protected override void ActOnTrigger(Collider2D other)
        {
            if (!isClear)
            {
                slicePuzzleCanvas.gameObject.SetActive(true);
                Time.timeScale = 0f;
            }
        }

        protected override void OnTriggerEnter2D(Collider2D other)
        {
            if (!isClear)
            {
                if (other.gameObject.tag.Equals("Player"))
                {
                    text.SetActive(true);
                }
            }
        }

        public void ClearPuzzle()
        {
            isClear = true;
            slicePuzzleCanvas.gameObject.SetActive(false);
            //clearPuzzleImage.SetActive(true);
            StartCoroutine(EquipKey());
        }

        private IEnumerator EquipKey()
        {
            yield return new WaitUntil(() => InputManager.Instance.GetKeyDown(ActionCode.Interaction));
            Inventory.instance.AddItem(key_3F);
            Time.timeScale = 1f;
        }
    }
}

