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

    // Start is called before the first frame update
    void Start()
    {
        
    }

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
    }
}
