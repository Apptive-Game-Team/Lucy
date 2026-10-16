using System.Collections;
using System.Collections.Generic;
using Lucy;
using ScriptableObjects.ScriptableObject_items.Script;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace invertoryAndItem
{
    /// <summary>
    /// Represents a single slot in the inventory containing an item and its quantity.
    /// </summary>
    [System.Serializable]
    public class ItemSlot
    {
        public ItemData item;
        public int quantity;
    }

    /// <summary>
    /// Manages the player's inventory system including item storage, equipment, and usage.
    /// Handles item stacking, equipment management, and item-specific effects like flashlight and battery.
    /// </summary>
    public class Inventory : MonoBehaviour
    {
        public ItemSlotUI[] uidSlot;     
        public ItemSlot[] slots;            

        public GameObject inventoryWindow;     

        [Header("Selected Item")]
        private ItemSlot selectedItem;
        private int selectedItemIndex;
        public TextMeshProUGUI selectedItemName;
        public TextMeshProUGUI selectedItemDescription;
        public GameObject useButton;
        public GameObject equipButton;
        public GameObject unEquipButton;
        public ItemSlot[] curEquipped;
        public List<Image> seperatingImages;

        public static Inventory instance;
        public ItemData battery;
        private bool separating;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                // Inventory lives on its own canvas. Remove the duplicate UI, not just this component.
                // Disable its external toggle listeners too, so they cannot reopen a destroyed window.
                foreach (var toggle in FindObjectsOfType<InventoryToggleManager>(true))
                {
                    if (toggle.inventoryWindow != gameObject) continue;
                    toggle.enabled = false;
                    Destroy(toggle);
                }
                gameObject.SetActive(false);
                Destroy(gameObject);
                return;
            }
            instance = this;
        }

        private void OnDestroy() { if (instance == this) instance = null; }

        private void OnDisable()
        {
            separating = false;
            if (seperatingImages == null) return;
            foreach (var image in seperatingImages)
                if (image != null) image.gameObject.SetActive(false);
        }

        // Start is called before the first frame update
        void Start()
        {
            inventoryWindow.SetActive(false);
            slots = new ItemSlot[uidSlot.Length];

            curEquipped = new ItemSlot[100];

            for (int i = 0; i < slots.Length; i++)
            {
                slots[i] = new ItemSlot();
                uidSlot[i].index = i;
                uidSlot[i].Clear();
            }

            useButton.SetActive(false);
            equipButton.SetActive(false);
            unEquipButton.SetActive(false);
            for (int i = 0;i < seperatingImages.Count;i++)
            {
                seperatingImages[i].gameObject.SetActive(false);
            }
            ClearSelectItemWindow();
            Lucy.Presentation.HospitalInventoryView.Attach(this);
        }

        /// <summary>
        /// Adds an item to the inventory, attempting to stack with existing items first.
        /// </summary>
        /// <param name="item">The item to add</param>
        /// <returns>True if item was successfully added, false if inventory is full</returns>
        public bool AddItem(ItemData item)
        {
            if (item == null || slots == null) return false;
            if (item.canStack)
            {
                ItemSlot slotToStackTo = GetItemStack(item);
                if (slotToStackTo != null)
                {
                    slotToStackTo.quantity++;
                    UpdateUI();
                    return true;  
                }
            }

            ItemSlot emptySlot = GetEmptySlot();

            if (emptySlot != null)
            {
                emptySlot.item = item;
                emptySlot.quantity = 1;
                UpdateUI();
                return true;  
            }
            return false;
        }

        void UpdateUI()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].item != null)
                    uidSlot[i].Set(slots[i]);
                else
                    uidSlot[i].Clear();
            }
        }

        ItemSlot GetItemStack(ItemData item)
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].item == item && slots[i].quantity < item.maxStackAmount)
                    return slots[i];
            }
            return null;
        }

        ItemSlot GetEmptySlot()
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].item == null)
                    return slots[i];
            }
            return null;
        }

        public void SelectItem(int index)
        {
            if (separating || slots == null || index < 0 || index >= slots.Length) return;
            if (slots[index].item == null) return;

            selectedItem = slots[index];
            selectedItemIndex = index;

            selectedItemName.text = selectedItem.item.displayName;
            selectedItemDescription.text = selectedItem.item.description;
            useButton.SetActive(false);
            equipButton.SetActive(false);
            unEquipButton.SetActive(false);
        
            if(selectedItem.item.type == ItemType.CONSUMABLE)
            {
                useButton.SetActive(true);
                equipButton.SetActive(false);
                unEquipButton.SetActive(false);
            }
            if(selectedItem.item.type == ItemType.EQUIPABLE && !uidSlot[index].equipped)
            {
                useButton.SetActive(false);
                equipButton.SetActive(true);
                unEquipButton.SetActive(false);
            }
            if(selectedItem.item.type == ItemType.EQUIPABLE && uidSlot[index].equipped)
            {
                useButton.SetActive(false);
                equipButton.SetActive(false);
                unEquipButton.SetActive(true);
            }
        }

        public void ClearSelectItemWindow()
        {
            selectedItem = null;
            useButton.SetActive(false);
            equipButton.SetActive(false);
            unEquipButton.SetActive(false);
            selectedItemName.text = string.Empty;
            selectedItemDescription.text = string.Empty;
        }

        public void OnUseButton()
        {
            if (separating || Lucy.Presentation.GameSession.BlocksGameplayInput || selectedItem == null ||
                selectedItem.item == null || selectedItem.item.type != ItemType.CONSUMABLE) return;
            if (selectedItem.item.itemId == ItemID.RemoteController)
            {
                // A single remote frees its own slot; stacked remotes need space for the reward.
                if (battery == null || (selectedItem.quantity > 1 && GetEmptySlot() == null &&
                    (!battery.canStack || GetItemStack(battery) == null))) return;
                RemoveSelectedItem();
                AddItem(battery);
                StartCoroutine(GetBatteryEvent());
                return;
            }
            bool applied = false;
            var stats = CharacterStat.instance;
            var effects = selectedItem.item.consumables;
            if (effects == null) return;
            foreach (var effect in effects)
            {
                if (effect == null || effect.value <= 0) continue;
                switch (effect.type)
                {
                    case ConsumableType.Battery:
                        if (FlashLight.instance != null) applied |= FlashLight.instance.Refill(Mathf.CeilToInt(effect.value));
                        break;
                    case ConsumableType.CurMental:
                        if (stats != null && stats.curMental < stats.maxMental)
                        { stats.curMental += effect.value; applied = true; }
                        break;
                    case ConsumableType.MaxMental:
                        if (stats != null)
                        { stats.maxMental += effect.value; stats.curMental += effect.value; applied = true; }
                        break;
                    case ConsumableType.Stamina:
                        if (stats != null && stats.curStamina < stats.maxStamina)
                        { stats.curStamina += effect.value; applied = true; }
                        break;
                }
            }
            if (stats != null) stats.UpdateStats();
            if (applied) RemoveSelectedItem();
        }
        public void OnEquipButton()
        {
            if (!separating && !Lucy.Presentation.GameSession.BlocksGameplayInput && selectedItem != null && selectedItem.item != null && selectedItem.item.type == ItemType.EQUIPABLE)
            {
                Equip(selectedItemIndex); 
            }
        }

        /// <summary>
        /// Handles item-specific side effects when equipping or unequipping.
        /// For example, flashlight affects light source and mental reduction.
        /// </summary>
        /// <param name="itemId">The ID of the item being equipped/unequipped</param>
        /// <param name="isEquipping">True if equipping, false if unequipping</param>
        private void HandleItemSpecificEquipLogic(ItemID itemId, bool isEquipping)
        {
            if (itemId == ItemID.FLASHLIGHT && FlashLight.instance != null && HandLightSwitch.instance != null && CharacterStat.instance != null)
            {
                if (isEquipping)
                {
                    HandLightSwitch.instance.TurnOnHandLight();
                    FlashLight.instance.SetUi();
                    FlashLight.instance.StartConsumeBattery();
                    if (FlashLight.instance.battery > 0) CharacterStat.instance.StopMentalReduce();
                }
                else
                {
                    HandLightSwitch.instance.TurnOffHandLight();
                    FlashLight.instance.TurnOffUi();
                    FlashLight.instance.StopConsumeBattery();
                    CharacterStat.instance.StartMentalReduce();
                }
            }
        }

        void Equip(int index)
        {
            if (uidSlot[index].equipped) return;
            HandleItemSpecificEquipLogic(slots[index].item.itemId, true);

            for (int i = 0; i < curEquipped.Length; i++)
            {
                if (curEquipped[i] == null)
                {
                    curEquipped[i] = slots[index];
                    uidSlot[index].equipped = true;
                    equipButton.SetActive(false);
                    unEquipButton.SetActive(true);
                    return;
                }
            }
        }

        public void OnUnEquipButton()
        {
            if (!separating && !Lucy.Presentation.GameSession.BlocksGameplayInput && selectedItem != null && selectedItem.item != null && selectedItem.item.type == ItemType.EQUIPABLE)
            {
                UnEquip(selectedItemIndex);
            }
        }

        void UnEquip(int index)
        {
            HandleItemSpecificEquipLogic(slots[index].item.itemId, false);
        
            for (int i = 0; i < curEquipped.Length; i++)
            {
                if (curEquipped[i] == slots[index])
                {
                    curEquipped[i] = null;
                    uidSlot[index].equipped = false;

                    equipButton.SetActive(true);
                    unEquipButton.SetActive(false); 
                    return;
                }
            }
        }
        public bool IsItemEquipped(ItemData item)
        {
            if (item == null || curEquipped == null) return false;
            for (int i = 0; i < curEquipped.Length; i++)
            {
                if (curEquipped[i] != null && curEquipped[i].item == item)
                {
                    return true;
                }
            }
            return false;
        }

        private void RemoveSelectedItem()
        {
            selectedItem.quantity--;    
            if (selectedItem.quantity <= 0)
            {
                if (uidSlot[selectedItemIndex].equipped) UnEquip(selectedItemIndex);
                selectedItem.item = null;
                ClearSelectItemWindow();
            }

            UpdateUI();
        }
        public bool HasItems(ItemData item, int quantity)
        {
            int totalQuantity = 0;

            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].item == item)
                {
                    totalQuantity += slots[i].quantity;

                    if (totalQuantity >= quantity)
                        return true;
                }
            }
            return false;
        }
    
        private IEnumerator GetBatteryEvent()
        {
            separating = true;
            foreach (var image in seperatingImages)
            {
                if (image == null) continue;
                image.gameObject.SetActive(true);
                yield return new WaitForSeconds(1f);
                image.gameObject.SetActive(false);
            }
            separating = false;
        }
    }
}
