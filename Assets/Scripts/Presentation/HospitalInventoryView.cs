using invertoryAndItem;
using UnityEngine;
using UnityEngine.UI;

namespace Lucy.Presentation
{
    /// <summary>Presentation only: retains all authored inventory slots and action callbacks.</summary>
    public sealed class HospitalInventoryView : MonoBehaviour
    {
        private Inventory inventory;
        private Text emptyHint;
        private Text capacity;
        private readonly Color muted = new Color(.61f,.69f,.69f);

        public static void Attach(Inventory target)
        {
            if (target.GetComponent<HospitalInventoryView>() != null) return;
            var view = target.gameObject.AddComponent<HospitalInventoryView>();
            view.inventory = target;
            view.Build();
        }

        private void Build()
        {
            var font = Resources.Load<Font>("Presentation/LucyFont");
            if (font == null || inventory.uidSlot == null || inventory.uidSlot.Length == 0) return;
            var background = transform.Find("InventoryBackground");
            if (background != null && background.TryGetComponent<Image>(out var panel))
            {
                panel.color = new Color(.075f,.105f,.12f,.98f);
                var border = panel.GetComponent<Outline>();
                if (border == null) border = panel.gameObject.AddComponent<Outline>();
                border.effectColor = new Color(.27f,.36f,.37f,.8f);
                border.effectDistance = new Vector2(1,-1);
            }
            foreach (var image in GetComponentsInChildren<Image>(true))
                if (image.name == "InfoBackground") image.color = new Color(.10f,.14f,.16f,1f);
            var grid = inventory.uidSlot[0].transform.parent.GetComponent<GridLayoutGroup>();
            if (grid != null)
            {
                // Eight columns, three rows; leave a dedicated header above the existing details area.
                grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = 8;
                grid.cellSize = new Vector2(92,92);
                grid.spacing = new Vector2(12,12);
                var rect = grid.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(0,95);
                rect.sizeDelta = new Vector2(820,300);
            }
            foreach (var slot in inventory.uidSlot)
            {
                if (slot == null) continue;
                var button = slot.button != null ? slot.button : slot.GetComponent<Button>();
                if (button != null && button.targetGraphic != null) button.targetGraphic.color = Color.white;
                StyleButton(button, false);
                if (slot.quantityText != null) slot.quantityText.color = new Color(.88f,.94f,.92f);
                if (slot.icon != null) slot.icon.preserveAspect = true;
            }
            foreach (var action in new[] { inventory.useButton, inventory.equipButton, inventory.unEquipButton })
            {
                if (action == null) continue;
                StyleButton(action.GetComponent<Button>(), true);
                foreach (var label in action.GetComponentsInChildren<TMPro.TMP_Text>(true)) label.color = Color.white;
            }
            if (inventory.selectedItemName != null) inventory.selectedItemName.color = new Color(.90f,.96f,.93f);
            if (inventory.selectedItemDescription != null) inventory.selectedItemDescription.color = new Color(.73f,.80f,.80f);
            Label("Inventory title", "소지품", font, new Vector2(-310,276), new Vector2(220,38), 30, Color.white);
            capacity = Label("Inventory capacity", "", font, new Vector2(-70,274), new Vector2(230,30), 17, muted);
            var close = Label("Inventory close hint", "I  ·  닫기", font, new Vector2(325,274), new Vector2(180,30), 19, muted);
            close.alignment = TextAnchor.MiddleRight;
            emptyHint = Label("Inventory guidance", "", font, new Vector2(-50,-160), new Vector2(720,100), 23, muted);
            emptyHint.alignment = TextAnchor.MiddleCenter;
            Refresh();
        }

        private void StyleButton(Button button, bool action)
        {
            if (button == null) return;
            button.transition = Selectable.Transition.ColorTint;
            var colors = button.colors;
            colors.normalColor = action ? new Color(.19f,.30f,.30f) : new Color(.16f,.20f,.22f);
            colors.highlightedColor = new Color(.32f,.43f,.43f);
            colors.selectedColor = new Color(.28f,.42f,.41f);
            colors.pressedColor = new Color(.39f,.54f,.51f);
            colors.disabledColor = new Color(.12f,.15f,.16f,.6f);
            colors.colorMultiplier = 1;
            button.colors = colors;
        }

        private Text Label(string name, string content, Font font, Vector2 position, Vector2 size, int fontSize, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            go.transform.SetParent(transform, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f,.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            var text = go.GetComponent<Text>();
            text.font = font;
            text.text = content;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = TextAnchor.MiddleLeft;
            text.raycastTarget = false;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            return text;
        }

        private void LateUpdate() { Refresh(); }

        private void Refresh()
        {
            if (inventory == null || emptyHint == null) return;
            int occupied = 0;
            if (inventory.slots != null)
                foreach (var slot in inventory.slots) if (slot != null && slot.item != null) occupied++;
            capacity.text = "PERSONAL EFFECTS   " + occupied + " / " + inventory.uidSlot.Length;
            bool selected = inventory.selectedItemName != null && !string.IsNullOrEmpty(inventory.selectedItemName.text);
            emptyHint.gameObject.SetActive(!selected);
            if (!selected) emptyHint.text = occupied == 0 ? "아직 챙긴 물건이 없습니다.\n주변을 조사해 필요한 물건을 찾아보세요." : "물건을 선택하면 설명을 확인할 수 있습니다.\n손전등은 선택 후 장착하세요.";
        }
    }
}
