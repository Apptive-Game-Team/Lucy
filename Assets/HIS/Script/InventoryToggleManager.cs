using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InventoryToggleManager : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent onOpenInventory;
    public UnityEvent onCloseInventory;

    public GameObject inventoryWindow;

    public GameObject useButton;
    public GameObject equipButton;
    public GameObject unEquipButton;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        bool isActive = inventoryWindow.activeSelf;

        if (isActive)
        {
            CloseInventory();
        }
        else
        {
            OpenInventory();
        }
    }

    public void OpenInventory()
    {
        inventoryWindow.SetActive(true);
        onOpenInventory?.Invoke();
    }

    public void CloseInventory()
    {
        Inventory.instance.ClearSelectItemWindow();
        inventoryWindow.SetActive(false);
        onCloseInventory?.Invoke();
        useButton.SetActive(false);
        equipButton.SetActive(false);
        unEquipButton.SetActive(false);
    }
}
