using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ItemObject : MonoBehaviour
{
    public ItemData item;
    public GameObject Text;

    private void Start()
    {
        Text.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            Text.SetActive(true);
        }
    }
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            if(Input.GetKey(KeyCode.Z))
            {
                Inventory.instance.AddItem(item);
                Destroy(gameObject);
            }
        }
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            Text.SetActive(false);
        }
    }
}
