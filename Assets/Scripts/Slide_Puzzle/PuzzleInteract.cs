using System.Collections;
using InputSystem;
using invertoryAndItem;
using ScriptableObjects.ScriptableObject_items.Script;
using UnityEngine;

namespace Slide_Puzzle
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
                Object.Destroy(this);
            }
        }

        protected override void Start()
        {
            // Without base.Start() this never registered as a key listener,
            // so ActOnTrigger was never called and the puzzle could not be opened.
            base.Start();
            text.SetActive(false);
            slicePuzzleCanvas.gameObject.SetActive(false);
            clearPuzzleImage.SetActive(false);
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
            // base tracks the collider that ActOnTrigger needs.
            base.OnTriggerEnter2D(other);
            if (!isClear && other.gameObject.tag.Equals("Player"))
            {
                text.SetActive(true);
            }
        }

        public void ClearPuzzle()
        {
            isClear = true;
            slicePuzzleCanvas.gameObject.SetActive(false);
            clearPuzzleImage.SetActive(true);
            StartCoroutine(EquipKey());
        }

        private IEnumerator EquipKey()
        {
            yield return new WaitUntil(() => InputManager.Instance.GetKeyDown(ActionCode.Interaction));
            Inventory.instance.AddItem(key_3F);
            clearPuzzleImage.SetActive(false);
            Time.timeScale = 1f;
        }
    }
}

