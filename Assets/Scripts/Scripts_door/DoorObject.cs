using System;
using System.Collections;
using invertoryAndItem;
using ReferenceSystem;
using ScriptableObjects.ScriptableObject_items.Script;
using Scripts_Creatures;
using soundSystem_;
using UnityEngine;

namespace Scripts_door
{
    [System.Serializable]
    public class DoorInfo
    {
        public ItemID keyId;
        public int DialogueId;
        public bool isLocked;
        public bool isOpened;

        public bool CheckCanOpen()
        {
            return !isLocked || (Inventory.instance != null && Inventory.instance.slots != null &&
                Array.Exists(Inventory.instance.slots, itemSlot => itemSlot != null && itemSlot.item != null &&
                    itemSlot.quantity > 0 && itemSlot.item.itemId == keyId));
        }
    }

    public class DoorObject : InteractableObject
    {
        private ToggleableObjectSoundController soundController;
        private CreatureManager creatureManager;
        [SerializeField] DoorInfo doorInfo;
        [SerializeField] GameObject door;
        [SerializeField] bool sideFacing;
        private Lucy.Presentation.HospitalDoorView view;
        private Bounds closedBounds;
        private bool busy;
        private float blockedUntil;
        public bool IsOpen => doorInfo != null && doorInfo.isOpened;
        public string InteractionLabel => busy ? "문이 움직이고 있습니다" : Time.time < blockedUntil
            ? "문 앞에서 비켜주세요" : doorInfo != null && !doorInfo.CheckCanOpen()
            ? "잠긴 문 · 열쇠 필요" : IsOpen ? "문 닫기" : "문 열기";

        protected override void ActOnTrigger(Collider2D other)
        {
            if (busy || door == null || doorInfo == null || !doorInfo.CheckCanOpen()) return;
            if (IsOpen && IsDoorwayOccupied()) { blockedUntil = Time.time + 1.5f; return; }
            StartCoroutine(ChangeDoor(!IsOpen));
        }

        private bool IsDoorwayOccupied()
        {
            foreach (var hit in Physics2D.OverlapBoxAll(closedBounds.center,closedBounds.size,0))
                if (hit.CompareTag("Player") || hit.GetComponentInParent<Scripts_Creatures.Creatures.Creature>() != null)
                    return true;
            return false;
        }

        private IEnumerator ChangeDoor(bool open)
        {
            busy = true;
            if (soundController != null) soundController.PlaySound(open);
            float elapsed = 0;
            while (elapsed < .22f)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.SmoothStep(0,1,Mathf.Clamp01(elapsed/.22f));
                if (view != null) view.SetOpenAmount(open ? progress : 1-progress);
                yield return null;
            }
            // An actor may have stepped into the opening during the closing transition.
            if (!open && IsDoorwayOccupied()) { open = true; blockedUntil = Time.time + 1.5f; }
            doorInfo.isOpened = open;
            if (open) Lucy.Presentation.HospitalTutorial.DoorOpened(gameObject.scene.name);
            door.SetActive(!open);
            if (view != null) view.SetOpenAmount(open ? 1 : 0);
            if (creatureManager != null) creatureManager.UpdateMap();
            busy = false;
        }

        public IEnumerator OpenForPassage()
        {
            while (busy) yield return null;
            if (!IsOpen && doorInfo!=null && doorInfo.CheckCanOpen()) yield return ChangeDoor(true);
        }

        private void Awake()
        {
            soundController = GetComponent<ToggleableObjectSoundController>();
        }

        protected override void Start()
        {
            base.Start();
            creatureManager = ReferenceManager.Instance.FindComponentByName<CreatureManager>("CreatureManager");
            if (doorInfo == null) doorInfo = new DoorInfo();
            if (door == null) return;
            bool initiallyOpen = doorInfo.isOpened || !door.activeSelf;
            door.SetActive(true);
            var blocker = door.GetComponent<BoxCollider2D>();
            if (blocker != null)
            {
                if (sideFacing) blocker.size = new Vector2(.35f,blocker.size.y);
                Physics2D.SyncTransforms();
                closedBounds = blocker.bounds;
            }
            var original = door.GetComponent<SpriteRenderer>();
            if (original != null)
            {
                var art = new GameObject("Ward door presentation");
                art.transform.SetParent(transform,false);
                art.transform.localPosition = door.transform.localPosition;
                view = art.AddComponent<Lucy.Presentation.HospitalDoorView>();
                view.Initialize(original,sideFacing);
            }
            doorInfo.isOpened = initiallyOpen;
            door.SetActive(!initiallyOpen);
            if (view != null) view.SetOpenAmount(initiallyOpen ? 1 : 0);
            if(doorInfo.isLocked && doorInfo.keyId == ItemID.NONE)  
            {
                Debug.LogWarning("This Door is Locked, But Has not Key. Please Door");
            }
            if (soundController != null) soundController.SetType("Door");
        }
    }
}
