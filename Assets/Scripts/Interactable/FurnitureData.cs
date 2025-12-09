using UnityEngine;

namespace Interactable
{
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
}