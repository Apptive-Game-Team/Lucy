using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FurnitureType
{
    Cabinet,
    Bookshelf,
    Drawer
}

[CreateAssetMenu(fileName = "Furniture", menuName = "New Furniture")]
public class FurnitureData : ScriptableObject
{
    public string furnitureName;
    public FurnitureType furnitureType;
}
