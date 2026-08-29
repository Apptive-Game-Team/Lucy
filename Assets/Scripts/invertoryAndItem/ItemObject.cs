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
            Text.SetActive(false);
        }

        protected override void ActOnTrigger(Collider2D other)
        {
            // Only remove the pickup if it actually made it into the inventory,
            // otherwise a full inventory silently deletes the item.
            if (Inventory.instance.AddItem(item))
            {
                Object.Destroy(gameObject);
            }
        }
    }
}
