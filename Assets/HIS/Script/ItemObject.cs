using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static ItemData;

public class ItemObject : MonoBehaviour
{
    public itemData item;
    public GameObject Text;

    private void Start()
    {
        Text.gameObject.SetActive(false);
    }
    private void OnTriggerEnter2d(Collider other)
    {
        if (other.gameObject.tag.Equals("Player"))
        {
            SetPromptText();
        }
    }
    private void SetPromptText()
    {
        Text.gameObject.SetActive(true);
    }
}
