using TMPro;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class ItemSlot
{
    public ItemData item;
    public int quantity;
}

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

    private void Awake()
    {
        instance = this;
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
    }

    public bool AddItem(ItemData item)
    {
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
        if (slots[index].item == null) return;

        selectedItem = slots[index];
        selectedItemIndex = index;

        selectedItemName.text = selectedItem.item.displayName;
        selectedItemDescription.text = selectedItem.item.description;
        
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
        selectedItemName.text = string.Empty;
        selectedItemDescription.text = string.Empty;
    }

    public void OnUseButton()
    {
        if (selectedItem.item.type == ItemType.CONSUMABLE)
        {
            for (int i = 0; i < selectedItem.item.consumables.Length; i++)
            {
                switch (selectedItem.item.consumables[i].type)
                {
                    case ConsumableType.Battery:
                        if (FlashLight.instance.battery <= 0)
                        {
                            HandLightSwitch.instance.TurnOnHandLight();
                            FlashLight.instance.battery += 2;
                            FlashLight.instance.StartConsumeBattery();
                            CharacterStat.instance.StopMentalReduce();
                            FlashLight.instance.UpdateUi();
                            break;
                        }
                        else
                        {
                            FlashLight.instance.battery += 2;
                            FlashLight.instance.UpdateUi();
                            break;
                        }
                    case ConsumableType.CurMental:
                        CharacterStat.instance.curMental += 10;
                        CharacterStat.instance.UpdateStats();
                        break;
                    case ConsumableType.MaxMental:
                        CharacterStat.instance.maxMental += 10;
                        CharacterStat.instance.curMental += 10;
                        CharacterStat.instance.UpdateStats();
                        break;
                }
            }
        }

        if (selectedItem.item.itemId == ItemID.RemoteController)
        {
            StartCoroutine(GetBatteryEvent(battery));
        }

        RemoveSelectedItem();
    }
    public void OnEquipButton()
    {
        if (selectedItem != null && selectedItem.item.type == ItemType.EQUIPABLE)
        {
            Equip(selectedItemIndex); 
        }
    }

    void Equip(int index)
    {

        if (slots[index].item.itemId == ItemID.FLASHLIGHT)
        {
            HandLightSwitch.instance.TurnOnHandLight();
            FlashLight.instance.SetUi();
            FlashLight.instance.StartConsumeBattery();
            CharacterStat.instance.StopMentalReduce();
        }

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
        if (selectedItem != null && selectedItem.item.type == ItemType.EQUIPABLE)
        {
            UnEquip(selectedItemIndex);
        }
    }

    void UnEquip(int index)
    {
        if (slots[index].item.itemId == ItemID.FLASHLIGHT)
        {
            HandLightSwitch.instance.TurnOffHandLight();
            FlashLight.instance.TurnOffUi();
            FlashLight.instance.StopConsumeBattery();
            CharacterStat.instance.StartMentalReduce();
        }
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
        for (int i = 0; i < curEquipped.Length; i++)
        {
            if (curEquipped[i].item == item)
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
    
    private IEnumerator GetBatteryEvent(ItemData item)
    {
        //inventoryWindow.SetActive(false);
        Time.timeScale = 0;
        for (int i = 0; i < seperatingImages.Count; i++)
        {
            seperatingImages[i].gameObject.SetActive(true);
            yield return new WaitForSecondsRealtime(2f);
            seperatingImages[i].gameObject.SetActive(false);
        }
        Time.timeScale = 1;
        AddItem(item);
    }
}