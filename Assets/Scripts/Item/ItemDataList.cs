using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDataList", menuName = "New ItemDataList")]
public class ItemDataList : ScriptableObject
{
    public List<ItemData> items;
}
