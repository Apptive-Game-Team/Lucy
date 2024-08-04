using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemData : MonoBehaviour
{
    public enum ItemType
    {
        Equipable,
        Consumable
    }

    public enum ConsumableType
    {
        Hunger,
        Battery,
        Mental
    }

    [CreateAssetMenu(fileName = "Item", menuName = "New Item")]
    public class itemData : ScriptableObject
    {
        [Header("Info")]
        public string displayName;
        public string description;
        public ItemType type;
        public Sprite icon;
        public GameObject dropPerfab;

        [Header("Stacking")]
        public bool canStack;
        public int maxStackAmount;
    }
}
