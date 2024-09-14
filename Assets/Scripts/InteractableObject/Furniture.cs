using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Furniture : InteractableObject
{
    public FurnitureData furniture;

    protected override void ActOnTrigger(Collider2D other)
    {
        FurnitureController.Instance.furnitures[furniture.furnitureType].Interact();
    }
}