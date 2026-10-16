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
        private bool rewardPending;
        private bool showingReward;

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
            base.Start();
            if (text != null) text.SetActive(false);
            if (slicePuzzleCanvas != null) slicePuzzleCanvas.gameObject.SetActive(false);
            if (clearPuzzleImage != null) clearPuzzleImage.SetActive(false);
        }
        protected override void ActOnTrigger(Collider2D other)
        {
            if (rewardPending && !showingReward)
            {
                TryEquipKey();
            }
            else if (!isClear && slicePuzzleCanvas != null)
            {
                slicePuzzleCanvas.gameObject.SetActive(true);
                Time.timeScale = 0f;
            }
        }

        protected override void OnTriggerEnter2D(Collider2D other)
        {
            base.OnTriggerEnter2D(other);
            if (!isClear || rewardPending)
            {
                if (other.gameObject.tag.Equals("Player"))
                {
                    if (text != null) text.SetActive(true);
                }
            }
        }

        protected override void OnTriggerExit2D(Collider2D other)
        {
            base.OnTriggerExit2D(other);
            if (other.CompareTag("Player") && text != null) text.SetActive(false);
        }

        public void ClearPuzzle()
        {
            if (isClear) return;
            isClear = true;
            rewardPending = true;
            if (text != null) text.SetActive(false);
            if (slicePuzzleCanvas != null) slicePuzzleCanvas.gameObject.SetActive(false);
            if (clearPuzzleImage != null) clearPuzzleImage.SetActive(true);
            showingReward = true;
            StartCoroutine(EquipKey());
        }

        private IEnumerator EquipKey()
        {
            yield return new WaitUntil(() => InputManager.Instance.GetKeyDown(ActionCode.Interaction));
            TryEquipKey();
            if (clearPuzzleImage != null) clearPuzzleImage.SetActive(false);
            showingReward = false;
            Time.timeScale = 1f;
        }

        private void TryEquipKey()
        {
            if (!rewardPending) return;
            if (Inventory.instance != null && Inventory.instance.AddItem(key_3F))
            {
                rewardPending = false;
                if (text != null) text.SetActive(false);
            }
            else
            {
                // Leave the reward here so a full inventory never traps the player in paused UI.
                if (text != null) text.SetActive(true);
                Debug.Log("Make room in the inventory, then interact again to collect the key.");
            }
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}

