using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

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
    //public TextMeshProUGUI selectedItemStatName;
    //public TextMeshProUGUI selectedItemStatValue;
    public GameObject useButton;
    public GameObject equipButton;
    public GameObject unEquipButton;

    private int curEquipIndex;

    public static Inventory instance;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        inventoryWindow.SetActive(false);
        slots = new ItemSlot[uidSlot.Length];

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i] = new ItemSlot();
            uidSlot[i].index = i;
            uidSlot[i].Clear();
        }

        useButton.SetActive(false);
        equipButton.SetActive(false);
        unEquipButton.SetActive(false);
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

        // 선택한 아이템 정보 가져오기
        selectedItem = slots[index];
        selectedItemIndex = index;

        selectedItemName.text = selectedItem.item.displayName;
        selectedItemDescription.text = selectedItem.item.description;
        //selectedItemStatName.text = string.Empty;
        //selectedItemStatValue.text = string.Empty;

        /*for (int i = 0; i < selectedItem.item.consumables.Length; i++)
        {
            // 먹을 수 있는 아이템일 경우 채워주는 체력과 배고픔을 UI 상에 표시해주기 위한 코드
            selectedItemStatName.text += selectedItem.item.consumables[i].type.ToString() + "\n";
            selectedItemStatValue.text += selectedItem.item.consumables[i].value.ToString() + "\n";
        }*/

        // 아이템 타입을 체크하여 버튼들 활성화
        if(selectedItem.item.type == ItemType.Consumable)
        {
            useButton.SetActive(true);
            equipButton.SetActive(false);
            unEquipButton.SetActive(false);
        }
        if(selectedItem.item.type == ItemType.Equipable && !uidSlot[index].equipped)
        {
            useButton.SetActive(false);
            equipButton.SetActive(true);
            unEquipButton.SetActive(false);
        }
        if(selectedItem.item.type == ItemType.Equipable && uidSlot[index].equipped)
        {
            useButton.SetActive(false);
            equipButton.SetActive(false);
            unEquipButton.SetActive(true);
        }
    }

    public void ClearSelectItemWindow()
    {
        // 아이템 초기화
        selectedItem = null;
        selectedItemName.text = string.Empty;
        selectedItemDescription.text = string.Empty;
        //selectedItemStatName.text = string.Empty;
        //selectedItemStatValue.text = string.Empty;
    }

    public void OnUseButton()
    {
        // 아이템 타입이 사용 가능할 경우
        /*if (selectedItem.item.type == ItemType.Consumable)
        {
            for (int i = 0; i < selectedItem.item.consumables.Length; i++)
            {
                switch (selectedItem.item.consumables[i].type)
                {
                    // consumables 타입에 따라 Heal과 Eat
                    case ConsumableType.Battery:
                        conditions.Heal(selectedItem.item.consumables[i].value);
                        break;
                    case ConsumableType.Hunger:
                        conditions.Eat(selectedItem.item.consumables[i].value);
                        break;
                }
            }
        }*/
        RemoveSelectedItem();
    }
    public void OnEquipButton()
    {
        if (selectedItem != null && selectedItem.item.type == ItemType.Equipable)
        {
            Equip(selectedItemIndex);
        }
    }

    void Equip(int index)
    {
        if (curEquipIndex >= 0 && curEquipIndex < slots.Length && uidSlot[curEquipIndex].equipped)
        {
            UnEquip(curEquipIndex);
        }

        curEquipIndex = index;
        uidSlot[curEquipIndex].equipped = true;

        equipButton.SetActive(false);
        unEquipButton.SetActive(true);
    }

    public void OnUnEquipButton()
    {
        if (selectedItem != null && selectedItem.item.type == ItemType.Equipable)
        {
            UnEquip(selectedItemIndex);
        }
    }

    void UnEquip(int index)
    {
        if (index >= 0 && index < slots.Length && uidSlot[index].equipped)
        {
            uidSlot[index].equipped = false;

            equipButton.SetActive(true);
            unEquipButton.SetActive(false);

            Debug.Log($"{slots[index].item.displayName} unequipped.");
        }
    }

    private void RemoveSelectedItem()
    {
        selectedItem.quantity--;    // 수량 깎기.

        // 아이템의 남은 수량이 0이 되면
        if (selectedItem.quantity <= 0)
        {
            // 만약 버린 아이템이 장착 중인 아이템일 경우 해제 시키기
            if (uidSlot[selectedItemIndex].equipped) UnEquip(selectedItemIndex);

            // 아이템 제거 및 UI에서도 아이템 정보 지우기
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
}