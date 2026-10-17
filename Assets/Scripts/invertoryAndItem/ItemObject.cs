using ScriptableObjects.ScriptableObject_items.Script;
using UnityEngine;

namespace invertoryAndItem
{
    public class ItemObject : InteractableObject
    {
        public ItemData item;
        public GameObject Text;

        protected override void Start()
        {
            base.Start();
            if (Text != null) Text.SetActive(false);
            RefreshWorldVisual();
        }

        private void RefreshWorldVisual()
        {
            if (item == null) return;
            var source = GetComponent<SpriteRenderer>();
            if (source == null) return;
            Sprite icon = item.icon;
            if (icon == null)
            {
                // Older item assets contain missing GUIDs. Use their existing authored resource art.
                string path = null;
                switch (item.itemId)
                {
                    case ItemID.FLASHLIGHT: path = "Presentation/FlashlightIcon"; break;
                    case ItemID.BATTERY: path = "Image_BSY/item_battery"; break;
                    case ItemID.CANDY: path = "Image_BSY/item_candy"; break;
                    case ItemID.DOLL: path = "Image_BSY/item_doll"; break;
                    case ItemID.KEY_3F: path = "Sprite_Key"; break;
                    case ItemID.RemoteController: path = "Presentation/RemoteIcon"; break;
                }
                if (path != null) icon = Resources.Load<Sprite>(path);
            }
            if (icon == null) return;

            // A visual-only child keeps the pickup's authored transform, trigger and physics shape intact.
            var visual = new GameObject("Pickup item artwork");
            visual.transform.SetParent(transform, false);
            var spriteSize = icon.bounds.size;
            var parentScale = transform.lossyScale;
            float sx = Mathf.Max(.001f, Mathf.Abs(parentScale.x));
            float sy = Mathf.Max(.001f, Mathf.Abs(parentScale.y));
            float authoredFootprint = Mathf.Max(source.size.x * sx, source.size.y * sy);
            float longestSide = Mathf.Clamp(authoredFootprint, .45f, .85f);
            float scale = longestSide / Mathf.Max(.001f, Mathf.Max(spriteSize.x, spriteSize.y));
            visual.transform.localScale = new Vector3(scale / sx, scale / sy, 1);
            // Center even artwork with an off-center imported pivot on the original pickup position.
            visual.transform.localPosition = new Vector3(-icon.bounds.center.x * scale / sx,
                -icon.bounds.center.y * scale / sy, 0);
            var renderer = visual.AddComponent<SpriteRenderer>();
            renderer.sprite = icon;
            renderer.sharedMaterial = source.sharedMaterial;
            renderer.color = Color.white;
            renderer.flipX = source.flipX;
            renderer.flipY = source.flipY;
            renderer.sortingLayerID = source.sortingLayerID;
            renderer.sortingOrder = Mathf.Max(1, source.sortingOrder);
            source.enabled = false;
        }

        protected override void ActOnTrigger(Collider2D other)
        {
            if (Inventory.instance != null && Inventory.instance.AddItem(item))
                Object.Destroy(gameObject);
        }
    }
}
