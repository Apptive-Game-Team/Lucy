using System.Collections.Generic;
using ScriptableObjects.ScriptableObject_items.Script;
using UnityEngine;

namespace Item
{
    [CreateAssetMenu(fileName = "ItemDataList", menuName = "New ItemDataList")]
    public class ItemDataList : ScriptableObject
    {
        public List<ItemData> items;
    }
}
