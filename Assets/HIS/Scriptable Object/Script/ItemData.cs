using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
    NONE,
    EQUIPABLE,
    CONSUMABLE,
    KEY,
}

public enum ConsumableType
{
    Stamina,
    Battery,
    CurMental,
    MaxMental,
    RemoteController
}

public enum ItemID
{
    NONE,
    BATTERY,
    FLASHLIGHT,
    CANDY,
    DOLL,
    RemoteController,
    KEY_3F
}

[Serializable]
public class ConsumableItemData
{
    public ConsumableType type;
    public float value;
}

[CreateAssetMenu(fileName = "Item", menuName = "New Item")]
public class ItemData : ScriptableObject
{
    [Header("Info")]
    public string displayName;
    public string description;
    public ItemType type;
    public Sprite icon;
    public ItemID itemId;

    [Header("Stacking")]
    public bool canStack;
    public int maxStackAmount;

    [Header("Consumable")]
    public ConsumableItemData[] consumables;
}
