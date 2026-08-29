using System.Collections.Generic;
using UnityEngine;

namespace Interactable
{
    public class Furniture : InteractableObject
    {
        public FurnitureData furniture;

        protected override void ActOnTrigger(Collider2D other)
        {
            Dictionary<FurnitureType, Furnitures> furnitures = FurnitureController.Instance.furnitures;
            if (furniture == null || furnitures == null)
            {
                return;
            }
            if (furnitures.TryGetValue(furniture.furnitureType, out Furnitures target))
            {
                target.Interact(this);
            }
        }
    }
}