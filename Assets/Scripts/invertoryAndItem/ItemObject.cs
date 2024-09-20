using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

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
        Destroy(gameObject);
    }
}
