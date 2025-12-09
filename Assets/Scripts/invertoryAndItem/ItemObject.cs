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
            Inventory.instance.AddItem(item);
            Object.Destroy(gameObject);
        }
    }
}
