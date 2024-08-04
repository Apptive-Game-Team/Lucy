using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemData : MonoBehaviour
{
    public enum ItemType
    {
        Equipable,
        Consumable,
        JustObject
    }

    public enum ConsumableType
    {
        Hunger,
        Battery,
        Mental
    }

    [Serializable]
    public class ConsumableItemData
    {
        public ConsumableType type;
        public float value;
    }

    [CreateAssetMenu(fileName = "Item", menuName = "New Item")]
    public class itemData : ScriptableObject
    {
        [Header("Info")]
        public string displayName;
        public string description;
        public ItemType type;
        public Sprite icon;

        [Header("Stacking")]
        public bool canStack;
        public int maxStackAmount;

        [Header("Consumable")]
        public ConsumableItemData[] consumables;
    }
}
