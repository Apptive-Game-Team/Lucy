using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace invertoryAndItem
{
    public class ItemSlotUI : MonoBehaviour
    {
        public Button button;
        public Image icon;
        public TextMeshProUGUI quantityText;
        private ItemSlot curSlot;

        public int index;
        public bool equipped;

        // ������ Slot ���� ����
        public void Set(ItemSlot slot)
        {
            curSlot = slot;
            icon.gameObject.SetActive(true);
            icon.sprite = slot.item.icon;
            quantityText.text = slot.quantity > 1 ? slot.quantity.ToString() : string.Empty;
        }

        public void Clear()
        {
            curSlot = null;
            icon.gameObject.SetActive(false);
            quantityText.text = string.Empty;
        }

        public void OnButtonClick()
        {
            Inventory.instance.SelectItem(index);
        }
    }
}