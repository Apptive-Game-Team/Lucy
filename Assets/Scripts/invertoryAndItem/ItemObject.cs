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
        }

        protected override void ActOnTrigger(Collider2D other)
        {
            if (Inventory.instance != null && Inventory.instance.AddItem(item))
                Object.Destroy(gameObject);
        }
    }
}
